using Contal.Cgp.BaseLib;
using Contal.Cgp.Client;
using Contal.Cgp.Server.Beans.Extern;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Contal.Cgp.NCAS.Client
{
    public partial class EventlogsDoorEnvironmentEditForm : UserControl
    {
        private readonly Guid _doorEnvironmentId;
        private const string LocalizationPrefix = "EventlogsDoorEnvironmentEditForm";
        private static readonly IList<string> _defaultEventlogTypes = new List<string>
        {
            Eventlog.TYPEDSMNORMALACCESS,
            Eventlog.TYPEDSMACCESSPERMITTED,
            Eventlog.TYPEACCESSDENIED
        };

        private class EventlogRow
        {
            public DateTime EventlogDateTime { get; set; }
            public string Type { get; set; }
            public string EventSources { get; set; }
        }

        public EventlogsDoorEnvironmentEditForm(Guid doorEnvironmentId)
        {
            _doorEnvironmentId = doorEnvironmentId;

            InitializeComponent();

            _bRefresh.Text = NCASClient.LocalizationHelper.GetString("EventlogsDoorEnvironmentEfitForm_bRefresh");
            _dgcDateTime.Name = Eventlog.COLUMN_EVENTLOG_DATE_TIME;
            _dgcDateTime.HeaderText = NCASClient.LocalizationHelper.GetString(LocalizationPrefix + "_dgcDateTime");
            _dgcType.Name = Eventlog.COLUMN_TYPE;
            _dgcType.HeaderText = NCASClient.LocalizationHelper.GetString(LocalizationPrefix + "_dgcType");
            _dgcDescription.Name = Eventlog.COLUMN_EVENTSOURCES;
            _dgcDescription.HeaderText = NCASClient.LocalizationHelper.GetString(LocalizationPrefix + "_dgcEventSources");

        }

        private void _bRefresh_Click(object sender, EventArgs e)
        {
            RefreshData();
        }

        public DataGridView DataGrid
        {
            get { return _dgEventlogs; }
        }

        public void RefreshData()
        {
            if (CgpClient.Singleton.MainServerProvider == null || CgpClient.Singleton.IsConnectionLost(false))
                return;

            var filterSettings = new List<FilterSettings>
            {
                new FilterSettings(Eventlog.COLUMN_EVENTSOURCES, new List<Guid> { _doorEnvironmentId }, ComparerModes.IN),
                new FilterSettings(Eventlog.COLUMN_TYPE, _defaultEventlogTypes, ComparerModes.IN)
            };

            var serverGeneralOptions = CgpClient.Singleton.MainServerProvider.ServerGenaralOptionsProvider.ReturnServerGeneralOptions();
            var maxRows = serverGeneralOptions != null && serverGeneralOptions.EventlogsCountToDisplay > 0
                ? serverGeneralOptions.EventlogsCountToDisplay
                : 10;

            var eventlogs = CgpClient.Singleton.MainServerProvider.Eventlogs.SelectRangeByCriteria(
                filterSettings,
                null,
                0,
                maxRows);

            var rows = eventlogs
                .OrderByDescending(e => e.EventlogDateTime)
                .Select(eventlog => new EventlogRow
                {
                    EventlogDateTime = eventlog.EventlogDateTime,
                    Type = eventlog.Type,
                    EventSources = GetEventSourcesText(eventlog)
                })
                .Take(maxRows)
                .ToList();

            _dgEventlogs.DataSource = rows;
        }

        private static string GetEventSourcesText(Eventlog eventlog)
        {
            if (eventlog == null)
                return string.Empty;

            var eventSources = eventlog.EventSources;

            if ((eventSources == null || eventSources.Count == 0) && CgpClient.Singleton?.MainServerProvider?.Eventlogs != null)
            {
                var eventlogWithEventSources = CgpClient.Singleton.MainServerProvider.Eventlogs.GetObjectById(eventlog.IdEventlog);
                eventSources = eventlogWithEventSources?.EventSources;
            }

            if (eventSources == null || eventSources.Count == 0)
                return string.Empty;

            var eventSourceNames = eventSources
                .Select(eventSource => eventSource?.EventSourceObjectGuid ?? Guid.Empty)
                .Where(eventSourceGuid => eventSourceGuid != Guid.Empty)
                .Select(eventSourceGuid =>
                {
                    var objectType = CgpClient.Singleton.MainServerProvider.CentralNameRegisters.GetObjectTypeFromGuid(eventSourceGuid);
                    var ormObject = DbsSupport.GetTableObject(objectType, eventSourceGuid);

                    return ormObject != null ? ormObject.ToString() : eventSourceGuid.ToString();
                })
                .Distinct()
                .ToList();

            return string.Join(", ", eventSourceNames);
        }
    }
}
