using Contal.Cgp.BaseLib;
using Contal.Cgp.Globals;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.NCAS.Server.DB;
using Contal.Cgp.Server.Beans;
using Contal.Cgp.Server;
using Contal.Cgp.Server.DB;
using Contal.IwQuick;
using Contal.IwQuick.CrossPlatform;
using Contal.IwQuick.Sys;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Contal.Cgp.NCAS.Server.LprCameraIntegration
{
    internal sealed class PushIntegration : ASingleton<PushIntegration>
    {
        private readonly ConcurrentDictionary<Guid, SessionEntry> _sessions = new ConcurrentDictionary<Guid, SessionEntry>();
        private readonly ConcurrentDictionary<Guid, PlateState> _recentPlates = new ConcurrentDictionary<Guid, PlateState>();
        private readonly IReadOnlyCollection<IPushCameraProvider> _providers;
        private readonly object _sessionLock = new object();

        private PushIntegration() : base(null)
        {
            var login = ConfigurationManager.AppSettings["Nanopack5AuthenticationLogin"] ?? string.Empty;
            var password = ConfigurationManager.AppSettings["Nanopack5AuthenticationPassword"] ?? string.Empty;

            var ignoreValue = ConfigurationManager.AppSettings["Nanopack5IgnoreCertificateErrors"];
            var ignoreCertificateErrors = string.IsNullOrWhiteSpace(ignoreValue) || bool.TryParse(ignoreValue, out var parsed) && parsed;

            _providers = new[]
            {
                (IPushCameraProvider)new Nanopack5PushProvider(this, login, password, ignoreCertificateErrors)
            };
        }

        public void Start(DbWatcher watcher)
        {
            InitializeExistingCameras();

            if (watcher != null)
                watcher.CgpDBObjectChanged += DbObjectChanged;
        }

        public void Stop(DbWatcher watcher)
        {
            if (watcher != null)
                watcher.CgpDBObjectChanged -= DbObjectChanged;

            lock (_sessionLock)
            {
                foreach (var session in _sessions.Values)
                    session.Session.Dispose();

                _sessions.Clear();
            }
            _recentPlates.Clear();
        }

        private void InitializeExistingCameras()
        {
            try
            {
                var cameras = LprCameras.Singleton.List();
                if (cameras == null)
                    return;

                foreach (var camera in cameras)
                    EnsureSession(camera);
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
            }
        }

        private void DbObjectChanged(object objectId, ObjectType objectType, ObjectDatabaseAction action)
        {
            if (objectType != ObjectType.LprCamera)
                return;

            if (!TryGetId(objectId, out var id))
                return;

            if (action == ObjectDatabaseAction.Delete)
            {
                RemoveSession(id);
                return;
            }

            try
            {
                var camera = LprCameras.Singleton.GetById(id);
                EnsureSession(camera);
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
            }
        }

        private static bool TryGetId(object value, out Guid id)
        {
            switch (value)
            {
                case Guid guid:
                    id = guid;
                    return true;
                case null:
                    id = Guid.Empty;
                    return false;
                default:
                    return Guid.TryParse(value.ToString(), out id);
            }
        }

        private void EnsureSession(LprCamera camera)
        {
            if (camera == null || camera.IdLprCamera == Guid.Empty)
                return;

            var provider = _providers.FirstOrDefault(p => p.Supports(camera));
            if (provider == null)
            {
                RemoveSession(camera.IdLprCamera);
                return;
            }

            lock (_sessionLock)
            {
                if (_sessions.TryGetValue(camera.IdLprCamera, out var existing))
                {
                    if (existing.Provider == provider)
                    {
                        existing.Session.UpdateCamera(camera);
                        existing.Session.Start();
                        return;
                    }

                    _sessions.TryRemove(camera.IdLprCamera, out _);
                    existing.Session.Dispose();
                }

                var session = provider.CreateSession(camera);
                var entry = new SessionEntry(provider, session);
                _sessions[camera.IdLprCamera] = entry;
                session.Start();
            }
        }

        private void RemoveSession(Guid id)
        {
            lock (_sessionLock)
            {
                if (_sessions.TryRemove(id, out var entry))
                    entry.Session.Dispose();
            }
            _recentPlates.TryRemove(id, out _);
        }

        private bool TryRebindSession(Guid previousId, LprCamera updatedCamera, Nanopack5PushSession session)
        {
            if (session == null || updatedCamera == null)
                return false;

            var newId = updatedCamera.IdLprCamera;
            if (newId == Guid.Empty)
                return false;

            lock (_sessionLock)
            {
                if (!_sessions.TryGetValue(previousId, out var entry) || entry.Session != session)
                    return false;

                if (previousId == newId)
                {
                    entry.Session.UpdateCamera(updatedCamera);
                    return true;
                }

                if (_sessions.ContainsKey(newId))
                    return false;

                if (!_sessions.TryRemove(previousId, out entry))
                    return false;

                entry.Session.UpdateCamera(updatedCamera);
                _sessions[newId] = entry;

                if (_recentPlates.TryRemove(previousId, out var plate))
                    _recentPlates[newId] = plate;
            }

            return true;
        }

        internal void UpdateCameraTransport(Guid cameraId, string scheme)
        {
            if (cameraId == Guid.Empty || string.IsNullOrWhiteSpace(scheme))
                return;

            try
            {
                var camera = LprCameras.Singleton.GetById(cameraId);
                if (camera == null)
                    return;

                var updatedDescription = UpdateDescriptionWithProtocol(camera.Description, scheme);
                if (!string.Equals(updatedDescription, camera.Description, StringComparison.Ordinal))
                {
                    camera.Description = updatedDescription;
                    LprCameras.Singleton.Update(camera);
                }
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
            }
        }

        private static string UpdateDescriptionWithProtocol(string description, string scheme)
        {
            var normalizedScheme = scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) ? "HTTPS" : "HTTP";
            var marker = "Push protocol:";

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(description))
            {
                parts.AddRange(description
                    .Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(segment => segment.IndexOf(marker, StringComparison.OrdinalIgnoreCase) < 0));
            }

            parts.Add($"{marker} {normalizedScheme}");
            return string.Join(" | ", parts);
        }

        internal void ProcessPayload(Guid cameraId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (message.Length > 1024 * 1024)
            {
                ResetRecentPlate(cameraId);
                UpdateCamera(cameraId, null);
                return;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                var payload = serializer.DeserializeObject(message);

                var plate = EnumerateDecisionPlates(payload).Select(NormalizePlate).FirstOrDefault(IsLikelyPlate);
                if (string.IsNullOrEmpty(plate))
                    plate = EnumerateStrings(payload).Select(NormalizePlate).FirstOrDefault(IsLikelyPlate);
                if (!string.IsNullOrEmpty(plate) && !IsIgnoredPlate(plate))
                    TryUpdatePlate(cameraId, plate);
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
                ResetRecentPlate(cameraId);
                UpdateCamera(cameraId, null);
            }
        }

        internal void UpdateCamera(Guid cameraId, string lastPlate)
        {
            try
            {
                if (cameraId == Guid.Empty)
                    return;

                var camera = LprCameras.Singleton.GetById(cameraId);
                if (camera == null)
                    return;

                string normalized = null;
                if (!string.IsNullOrWhiteSpace(lastPlate))
                    normalized = NormalizePlate(lastPlate);

                if (string.IsNullOrEmpty(normalized))
                {

                    if (!string.IsNullOrEmpty(camera.LastLicensePlate))
                    {
                        camera.LastLicensePlate = null;
                        LprCameras.Singleton.Update(camera);
                    }
                    return;
                }

                if (!string.Equals(camera.LastLicensePlate, normalized, StringComparison.Ordinal))
                {
                    camera.LastLicensePlate = normalized;
                    LprCameras.Singleton.Update(camera);
                }
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
            }
        }

        internal void MarkOffline(Guid cameraId)
        {
            try
            {
                var camera = LprCameras.Singleton.GetById(cameraId);
                if (camera == null || !camera.IsOnline)
                    return;

                camera.IsOnline = false;
                camera.LastHeartbeatAt = DateTime.UtcNow;
                LprCameras.Singleton.Update(camera);
                ResetRecentPlate(cameraId);
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
            }
        }

        private void TryUpdatePlate(Guid cameraId, string plate)
        {
            var normalized = NormalizePlate(plate);
            if (string.IsNullOrEmpty(normalized))
                return;

            var now = DateTime.UtcNow;

            while (true)
            {
                if (!_recentPlates.TryGetValue(cameraId, out var existing))
                {
                    if (_recentPlates.TryAdd(cameraId, new PlateState(normalized, now)))
                    {
                        UpdateCamera(cameraId, normalized);
                        TryGrantVipImmediateAccess(cameraId, normalized);
                        return;
                    }

                    continue;
                }

                if (string.Equals(existing.Plate, normalized, StringComparison.Ordinal) &&
                    now - existing.Timestamp < PlateState.SuppressionInterval)
                    return;

                if (_recentPlates.TryUpdate(cameraId, new PlateState(normalized, now), existing))
                {
                    UpdateCamera(cameraId, normalized);
                    TryGrantVipImmediateAccess(cameraId, normalized);
                    return;
                }
            }
        }

        private void TryGrantVipImmediateAccess(Guid cameraId, string normalizedPlate)
        {
            if (cameraId == Guid.Empty || string.IsNullOrWhiteSpace(normalizedPlate))
                return;

            try
            {
                var doors = DoorEnvironments.Singleton.List();
                if (doors == null || doors.Count == 0)
                    return;

                var relatedDoorEnvironments =
                    doors.Where(door => IsDoorAssignedToCamera(door, cameraId))
                        .ToList();

                if (relatedDoorEnvironments.Count != 1)
                    return;

                var relatedDoorEnvironment = relatedDoorEnvironments[0];
                var now = DateTime.Now;

                var cars = Cars.Singleton.List()?
                    .Where(car => IsMatchingPlate(car, normalizedPlate) && IsCarValidNow(car, now))
                    .ToList();
                if (cars == null || cars.Count == 0)
                    return;

                foreach (var car in cars)
                {

                    var aclCars = ACLCars.Singleton.GetAssignedAclCars(car.IdCar);
                    if (aclCars == null || aclCars.Count == 0)
                        continue;

                    foreach (var aclCar in aclCars)
                    {
                        if (aclCar == null || !IsAclCarValidNow(aclCar, now))
                            continue;
                        var acl = aclCar.AccessControlList;
                        if (acl == null || acl.ACLSettings == null || acl.ACLSettings.Count == 0)
                            continue;

                        var aclSetting = acl.ACLSettings.FirstOrDefault(
                            setting =>
                                setting != null &&
                                setting.CardReaderObjectType == (byte)ObjectType.DoorEnvironment &&
                                setting.GuidCardReaderObject == relatedDoorEnvironment.IdDoorEnvironment &&
                                setting.Disabled != true);

                        if (aclSetting == null)
                            continue;

                        var aclTimeZone = aclSetting.TimeZone;
                        if (aclTimeZone != null && !aclTimeZone.IsOn(now))
                            continue;

                        var accessResult = DoorEnvironments.Singleton.DoorEnvironmentAccessGranted(relatedDoorEnvironment);
                        if (accessResult != true)
                            continue;

                        Eventlogs.Singleton.InsertEvent(
                            "Access granted",
                            GetType().Assembly.GetName().Name,
                            new[] { relatedDoorEnvironment.IdDoorEnvironment, cameraId, car.IdCar },
                            string.Format(
                                "VIP immediate access; plate={0}; doorEnvironmentId={1}",
                                normalizedPlate,
                                relatedDoorEnvironment.IdDoorEnvironment));

                        return;
                    }
                }
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
            }
        }

        private static bool IsDoorAssignedToCamera(DoorEnvironment doorEnvironment, Guid cameraId)
        {
            if (doorEnvironment == null || cameraId == Guid.Empty)
                return false;

            return doorEnvironment.LprCameraInternal != null && doorEnvironment.LprCameraInternal.IdLprCamera == cameraId
                   || doorEnvironment.LprCameraExternal != null && doorEnvironment.LprCameraExternal.IdLprCamera == cameraId;
        }

        private static bool IsMatchingPlate(Car car, string normalizedPlate)
        {
            if (car == null || string.IsNullOrWhiteSpace(normalizedPlate))
                return false;

            return string.Equals(NormalizePlate(car.Lp), normalizedPlate, StringComparison.Ordinal)
                   || string.Equals(NormalizePlate(car.WholeName), normalizedPlate, StringComparison.Ordinal);
        }

        private static bool IsCarValidNow(Car car, DateTime now)
        {
            if (car == null)
                return false;

            if (car.ValidityDateFrom.HasValue && car.ValidityDateFrom.Value > now)
                return false;

            if (car.ValidityDateTo.HasValue && car.ValidityDateTo.Value < now)
                return false;

            return true;
        }

        private static bool IsAclCarValidNow(ACLCar aclCar, DateTime now)
        {
            if (aclCar == null)
                return false;

            if (aclCar.DateFrom.HasValue && aclCar.DateFrom.Value > now)
                return false;

            if (aclCar.DateTo.HasValue && aclCar.DateTo.Value < now)
                return false;

            return true;
        }


        private void ResetRecentPlate(Guid cameraId)
        {
            _recentPlates.TryRemove(cameraId, out _);
        }

        private static bool IsIgnoredPlate(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return true;

            return plate.Equals("UNKNOW", StringComparison.OrdinalIgnoreCase) ||
                   plate.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase) ||
                   plate.Equals("REAR", StringComparison.OrdinalIgnoreCase) ||
                   plate.Equals("FRONT", StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> EnumerateStrings(object payload)
        {
            if (payload == null)
                yield break;

            switch (payload)
            {
                case string text:
                    yield return text;
                    break;
                case IDictionary<string, object> dictionary:
                    foreach (var value in dictionary.Values)
                    {
                        foreach (var item in EnumerateStrings(value))
                            yield return item;
                    }
                    break;
                case object[] array:
                    foreach (var value in array)
                    {
                        foreach (var item in EnumerateStrings(value))
                            yield return item;
                    }
                    break;
            }
        }

        private static IEnumerable<string> EnumerateDecisionPlates(object payload)
        {
            foreach (var decision in EnumerateDecisionNodes(payload))
            {
                foreach (var plate in EnumerateDecisionPlateValues(decision))
                    yield return plate;
            }
        }

        private static IEnumerable<object> EnumerateDecisionNodes(object payload)
        {
            if (payload == null)
                yield break;

            switch (payload)
            {
                case IDictionary<string, object> dictionary:
                    foreach (var kvp in dictionary)
                    {
                        if (string.Equals(kvp.Key, "decision", StringComparison.OrdinalIgnoreCase))
                            yield return kvp.Value;

                        foreach (var nested in EnumerateDecisionNodes(kvp.Value))
                            yield return nested;
                    }
                    break;
                case object[] array:
                    foreach (var value in array)
                    {
                        foreach (var nested in EnumerateDecisionNodes(value))
                            yield return nested;
                    }
                    break;
            }
        }

        private static IEnumerable<string> EnumerateDecisionPlateValues(object value)
        {
            if (value == null)
                yield break;

            switch (value)
            {
                case string text:
                    yield return text;
                    break;
                case IDictionary<string, object> dictionary:
                    var primary = TryGetString(dictionary, "@plate") ?? TryGetString(dictionary, "plate");
                    if (!string.IsNullOrWhiteSpace(primary))
                        yield return primary;

                    foreach (var child in dictionary.Values)
                    {
                        foreach (var nested in EnumerateDecisionPlateValues(child))
                            yield return nested;
                    }
                    break;
                case object[] array:
                    foreach (var item in array)
                    {
                        foreach (var nested in EnumerateDecisionPlateValues(item))
                            yield return nested;
                    }
                    break;
            }
        }

        private static string TryGetString(IDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null || string.IsNullOrEmpty(key))
                return null;

            foreach (var kvp in dictionary)
            {
                if (!string.Equals(kvp.Key, key, StringComparison.OrdinalIgnoreCase))
                    continue;

                return ConvertToString(kvp.Value);
            }

            return null;
        }

        private static string ConvertToString(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case string text:
                    return text;
                case char[] chars:
                    return new string(chars);
                case IDictionary<string, object> _:
                case object[] _:
                    return null;
                default:
                    return value.ToString();
            }
        }


        private static string NormalizePlate(string plate)
        {
            return string.IsNullOrWhiteSpace(plate) ? null : plate.Trim().ToUpperInvariant();
        }

        private static bool IsLikelyPlate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (value.Length < 2 || value.Length > 16)
                return false;

            var containsLetter = false;

            foreach (var ch in value)
            {
                if (!char.IsLetterOrDigit(ch) && ch != '-' && ch != ' ' && ch != '_' && ch != '.')
                    return false;
                if (char.IsLetter(ch))
                    containsLetter = true;
            }
            if (!containsLetter)
                return false;
            return true;
        }

        private sealed class SessionEntry
        {
            internal SessionEntry(IPushCameraProvider provider, IPushCameraSession session)
            {
                Provider = provider;
                Session = session;
            }

            internal IPushCameraProvider Provider { get; }

            internal IPushCameraSession Session { get; }
        }

        private sealed class PlateState
        {
            internal static readonly TimeSpan SuppressionInterval = TimeSpan.FromSeconds(2);

            internal PlateState(string plate, DateTime timestamp)
            {
                Plate = plate;
                Timestamp = timestamp;
            }

            internal string Plate { get; }

            internal DateTime Timestamp { get; }
        }

        private interface IPushCameraProvider
        {
            bool Supports(LprCamera camera);

            IPushCameraSession CreateSession(LprCamera camera);
        }

        private interface IPushCameraSession : IDisposable
        {
            void UpdateCamera(LprCamera camera);

            void Start();
        }

        private sealed class Nanopack5PushProvider : IPushCameraProvider
        {
            private readonly PushIntegration _integration;
            private readonly string _login;
            private readonly string _password;
            private readonly bool _ignoreCertificateErrors;

            internal Nanopack5PushProvider(PushIntegration integration, string login, string password, bool ignoreCertificateErrors)
            {
                _integration = integration;
                _login = login ?? string.Empty;
                _password = password ?? string.Empty;
                _ignoreCertificateErrors = ignoreCertificateErrors;
            }

            public bool Supports(LprCamera camera)
            {
                if (camera == null)
                    return false;

                return ContainsNanopack(camera.Name) || ContainsNanopack(camera.Description);
            }

            public IPushCameraSession CreateSession(LprCamera camera)
            {
                return new Nanopack5PushSession(_integration, camera, _login, _password, _ignoreCertificateErrors);
            }

            private static readonly char[] NanopackNameSeparators = { ' ', '-', '_' };
            private static bool ContainsNanopack(string text)
            {
                if (string.IsNullOrWhiteSpace(text))
                    return false;

                if (text.IndexOf("nanopack", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                var tokens = text.Split(NanopackNameSeparators, StringSplitOptions.RemoveEmptyEntries);
                foreach (var token in tokens)
                {
                    if (token.Equals("nanopack", StringComparison.OrdinalIgnoreCase) ||
                        token.Equals("nanopack5", StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }
        }

        private sealed class Nanopack5PushSession : IPushCameraSession
        {
            private readonly PushIntegration _owner;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
            private readonly string _login;
            private readonly string _password;
            private readonly bool _ignoreCertificateErrors;

            private readonly object _syncRoot = new object();
            private LprCamera _camera;
            private Task _runTask;

            internal Nanopack5PushSession(PushIntegration owner, LprCamera camera, string login, string password, bool ignoreCertificateErrors)
            {
                _owner = owner;
                _camera = camera;
                _login = login ?? string.Empty;
                _password = password ?? string.Empty;
                _ignoreCertificateErrors = ignoreCertificateErrors;
            }

            public void UpdateCamera(LprCamera camera)
            {
                if (camera == null)
                    return;

                lock (_syncRoot)
                    _camera = camera;
            }

            public void Start()
            {
                lock (_syncRoot)
                {
                    if (_runTask == null)
                        _runTask = Task.Run(() => RunAsync(_cancellation.Token));
                }
            }

            private async Task RunAsync(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    var camera = GetCamera();
                    var uri = BuildUri(camera);
                    if (uri == null)
                    {
                        await DelayAsync(TimeSpan.FromSeconds(30), token).ConfigureAwait(false);
                        continue;
                    }

                    try
                    {
                        using (var socket = CreateClient())
                        {
                            using (CertificateValidationScope.Create(_ignoreCertificateErrors))
                            {
                                await socket.ConnectAsync(uri, token).ConfigureAwait(false);
                            }

                            var boundId = EnsureCameraBinding();
                            if (boundId != Guid.Empty)
                                _owner.UpdateCameraTransport(boundId, uri.Scheme);

                            await SendAuthenticationAsync(socket, token).ConfigureAwait(false);
                            await SendEnableStreamsAsync(socket, token).ConfigureAwait(false);
                            await ReceiveLoopAsync(socket, token).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception error)
                    {
                        HandledExceptionAdapter.Examine(error);
                    }

                    if (!token.IsCancellationRequested)
                    {
                        var cameraId = EnsureCameraBinding();
                        if (cameraId != Guid.Empty)
                            _owner.MarkOffline(cameraId);

                        await DelayAsync(TimeSpan.FromSeconds(5), token).ConfigureAwait(false);
                    }
                }
            }

            private LprCamera GetCamera()
            {
                lock (_syncRoot)
                    return _camera;
            }

            private ClientWebSocket CreateClient()
            {
                var socket = new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.FromSeconds(20)
                    }
                };
                return socket;
            }

            private sealed class CertificateValidationScope : IDisposable
            {
                private static readonly IDisposable _empty = new EmptyDisposable();

                private readonly RemoteCertificateValidationCallback _previousCallback;
                private readonly RemoteCertificateValidationCallback _currentCallback;
                private readonly bool _usesGlobalCallback;

                private CertificateValidationScope(RemoteCertificateValidationCallback previous,
                                                   RemoteCertificateValidationCallback current,
                                                   bool usesGlobalCallback)
                {
                    _previousCallback = previous;
                    _currentCallback = current;
                    _usesGlobalCallback = usesGlobalCallback;
                }

                public static IDisposable Create(bool ignoreCertificateErrors)
                {
                    if (!ignoreCertificateErrors)
                        return _empty;

                    var previous = ServicePointManager.ServerCertificateValidationCallback;
                    RemoteCertificateValidationCallback current = (sender, certificate, chain, errors) => true;
                    ServicePointManager.ServerCertificateValidationCallback = current;

                    return new CertificateValidationScope(previous, current, true);
                }

                public void Dispose()
                {
                    if (_usesGlobalCallback &&
                        ServicePointManager.ServerCertificateValidationCallback == _currentCallback)
                    {
                        ServicePointManager.ServerCertificateValidationCallback = _previousCallback;
                    }
                }

                private sealed class EmptyDisposable : IDisposable
                {
                    public void Dispose()
                    {
                    }
                }
            }

            private async Task SendAuthenticationAsync(ClientWebSocket socket, CancellationToken token)
            {
                if (string.IsNullOrEmpty(_login))
                    return;

                var message = Serialize(new Dictionary<string, object>
                {
                    {
                        "setAuthentication", new Dictionary<string, object>
                        {
                            {"@login", _login},
                            {"@password", _password}
                        }
                    }
                });

                await SendAsync(socket, message, token).ConfigureAwait(false);
            }

            private static async Task SendEnableStreamsAsync(ClientWebSocket socket, CancellationToken token)
            {
                var message = Serialize(new Dictionary<string, object>
                {
                    {
                        "setEnableStreams", new Dictionary<string, object>
                        {
                            {"@configChanges", false},
                            {"@infoChanges", false},
                            {"@traces", true}
                        }
                    }
                });

                await SendAsync(socket, message, token).ConfigureAwait(false);
            }

            private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken token)
            {
                var buffer = new byte[64 * 1024];
                var builder = new StringBuilder();

                while (!token.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;
                    try
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await SafeCloseAsync(socket, token).ConfigureAwait(false);
                        break;
                    }

                    if (result.MessageType != WebSocketMessageType.Text)
                    {
                        if (result.EndOfMessage)
                            builder.Clear();
                        continue;
                    }

                    builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                    if (!result.EndOfMessage)
                        continue;

                    var payload = builder.ToString();
                    builder.Clear();

                    var cameraId = EnsureCameraBinding();
                    if (cameraId != Guid.Empty)
                        _owner.ProcessPayload(cameraId, payload);
                }
            }

            private static async Task SendAsync(ClientWebSocket socket, string message, CancellationToken token)
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, token).ConfigureAwait(false);
            }

            private static string Serialize(object value)
            {
                var serializer = new JavaScriptSerializer();
                return serializer.Serialize(value);
            }

            private static async Task SafeCloseAsync(ClientWebSocket socket, CancellationToken token)
            {
                try
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", token).ConfigureAwait(false);
                }
                catch
                {
                }
            }

            private static Uri BuildUri(LprCamera camera)
            {
                if (camera == null)
                    return null;

                var host = camera.IpAddress;
                if (string.IsNullOrWhiteSpace(host))
                    return null;

                if (TryBuildUri(ResolvePort(camera, secure: false), "ws", host, out var uri))
                    return uri;

                if (TryBuildUri(ResolvePort(camera, secure: true), "wss", host, out uri))
                    return uri;

                return null;
            }

            private static string ResolvePort(LprCamera camera, bool secure)
            {
                if (camera == null)
                    return null;

                var current = secure ? camera.PortSsl : camera.Port;
                if (!string.IsNullOrWhiteSpace(current))
                    return current;

                if (camera.IdLprCamera == Guid.Empty)
                    return null;

                try
                {
                    var storedCamera = LprCameras.Singleton.GetById(camera.IdLprCamera);
                    if (storedCamera == null)
                        return null;

                    var storedPort = secure ? storedCamera.PortSsl : storedCamera.Port;
                    if (string.IsNullOrWhiteSpace(storedPort))
                        return null;

                    if (secure)
                        camera.PortSsl = storedPort;
                    else
                        camera.Port = storedPort;

                    return storedPort;
                }
                catch (Exception error)
                {
                    HandledExceptionAdapter.Examine(error);
                    return null;
                }
            }


            private static bool TryBuildUri(string portText, string scheme, string host, out Uri uri)
            {
                uri = null;
                if (!int.TryParse(portText, out var port))
                    return false;

                try
                {
                    uri = new UriBuilder
                    {
                        Scheme = scheme,
                        Host = host,
                        Port = port,
                        Path = "/async"
                    }.Uri;
                    return true;
                }
                catch
                {
                    uri = null;
                    return false;
                }
            }

            private static async Task DelayAsync(TimeSpan delay, CancellationToken token)
            {
                try
                {
                    await Task.Delay(delay, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
            }

            public void Dispose()
            {
                _cancellation.Cancel();

                Task runTask;
                lock (_syncRoot)
                {
                    runTask = _runTask;
                    _runTask = null;
                }

                if (runTask != null)
                {
                    try
                    {
                        runTask.Wait(TimeSpan.FromSeconds(5));
                    }
                    catch
                    {
                    }
                }

                var cameraId = EnsureCameraBinding();
                if (cameraId != Guid.Empty)
                    _owner.MarkOffline(cameraId);

                _cancellation.Dispose();
            }

            private Guid EnsureCameraBinding()
            {
                var camera = GetCamera();
                if (camera == null)
                    return Guid.Empty;

                var cameraId = camera.IdLprCamera;
                if (cameraId != Guid.Empty && LprCameras.Singleton.GetById(cameraId) != null)
                    return cameraId;

                var replacement = FindCameraByIp(camera.IpAddress);
                if (replacement == null)
                    return cameraId;

                if (cameraId == replacement.IdLprCamera)
                {
                    lock (_syncRoot)
                        _camera = replacement;
                    return replacement.IdLprCamera;
                }

                if (_owner.TryRebindSession(cameraId, replacement, this))
                {
                    lock (_syncRoot)
                        _camera = replacement;
                    return replacement.IdLprCamera;
                }

                return cameraId;
            }

            private static LprCamera FindCameraByIp(string ipAddress)
            {
                if (string.IsNullOrWhiteSpace(ipAddress))
                    return null;

                var filters = new List<FilterSettings>
                {
                    new FilterSettings(LprCamera.COLUMNIPADDRESS, ipAddress.Trim(), ComparerModes.EQUALL)
                };

                var matches = LprCameras.Singleton.SelectByCriteria(filters);
                return matches?.FirstOrDefault();
            }
        }
    }
}
