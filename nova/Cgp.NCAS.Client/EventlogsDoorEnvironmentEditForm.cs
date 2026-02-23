using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Contal.Cgp.BaseLib;
using Contal.Cgp.Client;
using Contal.Cgp.Server.Beans.Extern;

namespace Contal.Cgp.NCAS.Client
{
    public partial class EventlogsDoorEnvironmentEditForm : UserControl
    {
        private readonly Guid _doorEnvironmentId;
        private static readonly IList<string> _defaultEventlogTypes = new List<string>
        {
            Eventlog.TYPEDSMACCESSRESTRICTED,
            Eventlog.TYPEDSMACCESSPERMITTED,
            Eventlog.TYPEACCESSDENIED
        };

        private class EventlogRow
        {
            public DateTime EventlogDateTime { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
        }

        public EventlogsDoorEnvironmentEditForm(Guid doorEnvironmentId)
        {
            _doorEnvironmentId = doorEnvironmentId;

            InitializeComponent();

            _bRefresh.Text = NCASClient.LocalizationHelper.GetString("EventlogsDoorEnvironmentEfitForm_bRefresh");
            _dgcDateTime.Name = Eventlog.COLUMN_EVENTLOG_DATE_TIME;
            _dgcDateTime.HeaderText = CgpClient.Singleton.LocalizationHelper.GetString(Eventlog.COLUMN_EVENTLOG_DATE_TIME);
            _dgcType.Name = Eventlog.COLUMN_TYPE;
            _dgcType.HeaderText = CgpClient.Singleton.LocalizationHelper.GetString(Eventlog.COLUMN_TYPE);
            _dgcDescription.Name = Eventlog.COLUMN_DESCRIPTION;
            _dgcDescription.HeaderText = CgpClient.Singleton.LocalizationHelper.GetString(Eventlog.COLUMN_DESCRIPTION);

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
                    Description = eventlog.Description
                })
                .Take(maxRows)
                .ToList();

            _dgEventlogs.DataSource = rows;
        }
    }
}
