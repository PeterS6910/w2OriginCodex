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
        private readonly Button _bRefresh;
        private readonly DataGridView _dgEventlogs;

        private class EventlogRow
        {
            public DateTime EventlogDateTime { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
        }

        public EventlogsDoorEnvironmentEditForm(Guid doorEnvironmentId)
        {
            _doorEnvironmentId = doorEnvironmentId;

            _bRefresh = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Location = new Point(8, 8),
                Size = new Size(95, 23),
                Text = NCASClient.LocalizationHelper.GetString("EventlogsDoorEnvironmentEfitForm_bRefresh")
            };
            _bRefresh.Click += (sender, args) => RefreshData();

            _dgEventlogs = new DataGridView
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(8, 40),
                Size = new Size(830, 380),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = false,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };

            _dgEventlogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(EventlogRow.EventlogDateTime),
                Name = Eventlog.COLUMN_EVENTLOG_DATE_TIME,
                HeaderText = CgpClient.Singleton.LocalizationHelper.GetString(Eventlog.COLUMN_EVENTLOG_DATE_TIME),
                Width = 165,
                DefaultCellStyle = { Format = "dd.MM.yyyy HH:mm:ss" }
            });

            _dgEventlogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(EventlogRow.Type),
                Name = Eventlog.COLUMN_TYPE,
                HeaderText = CgpClient.Singleton.LocalizationHelper.GetString(Eventlog.COLUMN_TYPE),
                Width = 180
            });

            _dgEventlogs.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(EventlogRow.Description),
                Name = Eventlog.COLUMN_DESCRIPTION,
                HeaderText = CgpClient.Singleton.LocalizationHelper.GetString(Eventlog.COLUMN_DESCRIPTION),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                MinimumWidth = 140
            });

            Controls.Add(_bRefresh);
            Controls.Add(_dgEventlogs);
        }

        public void RefreshData()
        {
            if (CgpClient.Singleton.MainServerProvider == null || CgpClient.Singleton.IsConnectionLost(false))
                return;

            var filterSettings = new List<FilterSettings>
            {
                new FilterSettings(Eventlog.COLUMN_EVENTSOURCES, new List<Guid> { _doorEnvironmentId }, ComparerModes.IN)
            };

            var serverGeneralOptions = CgpClient.Singleton.MainServerProvider.ServerGenaralOptionsProvider.ReturnServerGeneralOptions();
            var maxRows = serverGeneralOptions != null && serverGeneralOptions.EventlogsCountToDisplay > 0
                ? serverGeneralOptions.EventlogsCountToDisplay
                : 100;

            var eventlogs = CgpClient.Singleton.MainServerProvider.Eventlogs.SelectRangeByCriteria(
                filterSettings,
                null,
                0,
                maxRows);

            var rows = eventlogs
                .Select(eventlog => new EventlogRow
                {
                    EventlogDateTime = eventlog.EventlogDateTime,
                    Type = eventlog.Type,
                    Description = eventlog.Description
                })
                .ToList();

            _dgEventlogs.DataSource = rows;
        }
    }
}
