using Contal.Cgp.Globals;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.NCAS.Server.DB;
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Contal.Cgp.NCAS.Server.LprCameraIntegration
{
    internal sealed class PushIntegration : ASingleton<PushIntegration>
    {
        private readonly ConcurrentDictionary<Guid, SessionEntry> _sessions = new ConcurrentDictionary<Guid, SessionEntry>();
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
        }

        internal void ProcessPayload(Guid cameraId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            if (message.Length > 1024 * 1024)
            {
                UpdateCamera(cameraId, null);
                return;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                var payload = serializer.DeserializeObject(message);

                var plate = EnumerateStrings(payload).Select(NormalizePlate).FirstOrDefault(IsLikelyPlate);
                UpdateCamera(cameraId, plate);
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
                UpdateCamera(cameraId, null);
            }
        }

        internal void UpdateCamera(Guid cameraId, string lastPlate)
        {
            try
            {
                var camera = LprCameras.Singleton.GetById(cameraId);
                if (camera == null)
                    return;

                var changed = false;

                if (!camera.IsOnline)
                {
                    camera.IsOnline = true;
                    changed = true;
                }

                var now = DateTime.UtcNow;
                if (!camera.LastHeartbeatAt.HasValue || Math.Abs((camera.LastHeartbeatAt.Value - now).TotalSeconds) > 1)
                {
                    camera.LastHeartbeatAt = now;
                    changed = true;
                }

                if (!string.IsNullOrWhiteSpace(lastPlate))
                {
                    var normalized = NormalizePlate(lastPlate);
                    if (!string.IsNullOrEmpty(normalized) && !string.Equals(camera.LastLicensePlate, normalized, StringComparison.Ordinal))
                    {
                        camera.LastLicensePlate = normalized;
                        changed = true;
                    }
                }

                if (changed)
                    LprCameras.Singleton.Update(camera);
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
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
            }
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

            foreach (var ch in value)
            {
                if (!char.IsLetterOrDigit(ch) && ch != '-' && ch != ' ' && ch != '_' && ch != '.')
                    return false;
            }

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

            private static bool ContainsNanopack(string text)
            {
                return !string.IsNullOrWhiteSpace(text) && text.IndexOf("nanopack", StringComparison.OrdinalIgnoreCase) >= 0;
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
                        var cameraId = camera?.IdLprCamera ?? Guid.Empty;
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

                    var cameraId = GetCamera()?.IdLprCamera ?? Guid.Empty;
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

                if (TryBuildUri(camera.PortSsl, "wss", host, out var uri))
                    return uri;

                if (TryBuildUri(camera.Port, "ws", host, out uri))
                    return uri;

                return null;
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

                var cameraId = GetCamera()?.IdLprCamera ?? Guid.Empty;
                if (cameraId != Guid.Empty)
                    _owner.MarkOffline(cameraId);

                _cancellation.Dispose();
            }
        }
    }
}
