using Contal.Cgp.BaseLib;
using Contal.Cgp.Globals;
using Contal.Cgp.NCAS.RemotingCommon;
using Contal.Cgp.NCAS.Server;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.RemotingCommon;
using Contal.Cgp.Server.Beans;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace Contal.Cgp.Client
{
    public partial class CarEditForm :
#if DESIGNER
        Form
#else
        ACgpEditForm<Car>
#endif
    {
        public CarEditForm(Car car, ShowOptionsEditForm showOption)
            : base(car, showOption)
        {
            InitializeComponent();
            _editingObject = car;
            SetReferenceEditColors();
            _tcCar.SelectedIndexChanged += _tcCar_SelectedIndexChanged;
            _tcCar_SelectedIndexChanged(null, null);
            _tpDoorEnvironment.Text = GetString("CarEditForm_tpDoorEnvironment");
            _bAddDoorEnvironment.Text = GetString("CarEditForm_bAddDoorEnvironment");
            _bEditDoorEnvironment.Text = GetString("General_bEdit");
            _bDeleteDoorEnvironment.Text = GetString("CarEditForm_bDeleteDoorEnvironment");
            _tcDoorEnvironmentColumn.HeaderText = GetString("Name");
        }

        private readonly List<CarDoorEnvironment> _doorEnvironments = new List<CarDoorEnvironment>();

        protected override bool CheckValues()
        {
            return true;
        }

        protected override bool GetValues()
        {
            _editingObject.Lp = _eLp.Text;
            _editingObject.Brand = _eBrand.Text;
            _editingObject.ValidityDateFrom = _dpValidityDateFrom.Value;
            _editingObject.ValidityDateTo = _dpValidityDateTo.Value;
            _editingObject.Description = _eDescription.Text;

            return true;
        }

        protected override bool SaveToDatabaseInsert()
        {
            Exception error;
            var provider = GetCarProvider(out error);
            if (provider == null)
                return false;

            return provider.Cars.Insert(ref _editingObject, out error);
        }

        protected override bool SaveToDatabaseEdit()
        {
            Exception error;
            var provider = GetCarProvider(out error);
            if (provider == null)
                return false;

            return provider.Cars.Update(_editingObject, out error);
        }

        protected override bool SaveToDatabaseEditOnlyInDatabase()
        {
            Exception error;
            var provider = GetCarProvider(out error);
            if (provider == null)
                return false;

            return provider.Cars.UpdateOnlyInDatabase(_editingObject, out error);
        }

        protected override void RegisterEvents()
        {
        }

        protected override void UnregisterEvents()
        {
        }

        protected override void AfterTranslateForm()
        {
            Text = Insert
                ? GetString("CarEditFormInsertText")
                : GetString("CarEditFormEditText");

            CgpClientMainForm.Singleton.SetTextOpenWindow(this);
        }

        protected override void BeforeInsert()
        {
        }

        protected override void BeforeEdit()
        {
        }

        protected override void InternalReloadEditingObject(out bool allowEdit)
        {
            allowEdit = false;
        }

        public override void InternalReloadEditingObjectWithEditedData()
        {
        }

        protected override void SetValuesInsert()
        {
            _eLp.Text = string.Empty;
            _eBrand.Text = string.Empty;
            _dpValidityDateFrom.Value = null;
            _dpValidityDateTo.Value = null;
            _eDescription.Text = string.Empty;
            _lvAssignedCards.Items.Clear();
            _lvAvailableCards.Items.Clear();
            _doorEnvironments.Clear();
            _dgDoorEnvironments.DataGrid.DataSource = null;
        }

        protected override void SetValuesEdit()
        {
            _eLp.Text = _editingObject.Lp;
            _eBrand.Text = _editingObject.Brand;
            _dpValidityDateFrom.Value = _editingObject.ValidityDateFrom;
            _dpValidityDateTo.Value = _editingObject.ValidityDateTo;
            _eDescription.Text = _editingObject.Description;
            LoadCards();
            RefreshDoorEnvironmentsFromServer();
            LoadDoorEnvironments();
        }

        protected override void EditEnd()
        {
        }

        protected override void AfterInsert()
        {
        }

        protected override void AfterEdit()
        {
        }

        private void _tcCar_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (TabPage page in _tcCar.TabPages)
                page.BackColor = SystemColors.Control;
        }


        private void LoadCards()
        {
            _lvAssignedCards.Items.Clear();
            _lvAvailableCards.Items.Clear();
            if (_editingObject.IdCar == Guid.Empty)
                return;
            Exception error;
            var provider = GetCarProvider(out error);
            if (provider == null)
            {
                if (error != null)
                    MessageBox.Show(error.Message);
                return;
            }

            ICollection<Card> allCardsCollection = provider.Cards.List(out error);
            var activeCards = new List<Card>();
            if (allCardsCollection != null)
            {
                foreach (var card in allCardsCollection)
                {
                    if (card.State == (byte)CardState.Active || card.State == (byte)CardState.HybridActive)
                    {
                        if (card.GuidPerson == Guid.Empty)
                            continue;

                        activeCards.Add(card);
                    }
                }
            }
            IList<Card> assigned = provider.CarCards.GetCardsForCar(_editingObject.IdCar, out error);
            var assignedSet = new HashSet<Guid>();
            if (assigned != null)
            {
                foreach (var card in assigned)
                {
                    assignedSet.Add(card.IdCard);
                    string cardText = card.FullCardNumber.ToString();
                    if (card.Person != null && card.GuidPerson != Guid.Empty)
                    {
                        var person = provider.Persons.GetObjectById(card.GuidPerson);
                        cardText += " - " + person.FirstName + " " + person.Surname;
                    }

                    _lvAssignedCards.Items.Add(new ListViewItem(cardText) { Tag = card });
                }
            }
            foreach (var card in activeCards)
            {
                if (!assignedSet.Contains(card.IdCard))
                {
                    string cardText = card.FullCardNumber.ToString();
                    if (card.Person != null && card.GuidPerson != Guid.Empty)
                    {
                        var person = provider.Persons.GetObjectById(card.GuidPerson);
                        cardText += " - " + person.FirstName + " " + person.Surname;
                    }
                    _lvAvailableCards.Items.Add(new ListViewItem(cardText) { Tag = card });
                }
            }
        }

        private string BuildCardDescription(Card card)
        {
            string cardText = card.FullCardNumber.ToString();

            if (card.GuidPerson != Guid.Empty)
            {
                var provider = GetCarProvider(out var error);
                if (provider == null)
                {
                    if (error != null)
                        MessageBox.Show(error.Message);
                    return cardText;
                }

                Person person = provider.Persons.GetObjectById(card.GuidPerson);
                cardText += " - " + person?.FirstName + " " + person?.Surname;
            }

            return cardText;
        }

        private void _bAssignCard_Click(object sender, EventArgs e)
        {

            if (_lvAssignedCards.SelectedItems.Count == 0)
                return;
            foreach (ListViewItem item in _lvAssignedCards.SelectedItems)
            {
                var card = item.Tag as Card;
                if (card != null)
                {
                    var provider = GetCarProvider(out var providerError);
                    if (provider == null)
                    {
                        if (providerError != null)
                            MessageBox.Show(providerError.Message);
                        return;
                    }

                    provider.CarCards.UnassignCardFromCar(_editingObject.IdCar, card.IdCard);
                }
            }
            LoadCards();
        }

        private void _bUnassignCard_Click(object sender, EventArgs e)
        {
            if (_lvAvailableCards.SelectedItems.Count == 0)
                return;
            foreach (ListViewItem item in _lvAvailableCards.SelectedItems)
            {
                var card = item.Tag as Card;
                if (card != null)
                {
                    var provider = GetCarProvider(out var providerError);
                    if (provider == null)
                    {
                        if (providerError != null)
                            MessageBox.Show(providerError.Message);
                        return;
                    }

                    provider.CarCards.AssignCardToCar(_editingObject.IdCar, card.IdCard);
                }
            }
            LoadCards();
        }

        private void _bCancel_Click(object sender, EventArgs e)
        {
            Cancel_Click();
        }

        private void _bOk_Click(object sender, EventArgs e)
        {
            Ok_Click();
        }

        private void LoadDoorEnvironments()
        {
            _dgDoorEnvironments.DataGrid.DataSource = null;

            var view = _doorEnvironments
                .OrderBy(de => de.DoorEnvironment?.Name)
                .Select(de => new CarDoorEnvironmentView
                {
                    Symbol = ObjectImageList.Singleton.GetImageForAOrmObject(de.DoorEnvironment),
                    DoorEnvironmentId = de.DoorEnvironment?.IdDoorEnvironment ?? Guid.Empty,
                    DoorEnvironmentName = de.DoorEnvironment?.Name ?? string.Empty,
                    AccessType = de.AccessType
                })
                .ToList();

            _dgDoorEnvironments.DataGrid.DataSource = view;
        }

        private void RefreshDoorEnvironmentsFromServer()
        {
            var table = GetCarDoorEnvironmentsTable(out var error);
            if (error != null || table == null)
            {
                if (error != null)
                    MessageBox.Show(error.Message);
                return;
            }

            List<CarDoorEnvironment> carDoorEnvironments;
            try
            {
                carDoorEnvironments = table.List(out error)
                    ?.Where(cde => cde.Car != null && cde.Car.IdCar == _editingObject.IdCar)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            if (error != null || carDoorEnvironments == null)
                return;

            var doorEnvironmentsTable = GetNcasProvider(out var providerError)?.DoorEnvironments;
            if (providerError != null)
            {
                MessageBox.Show(providerError.Message);
                return;
            }
            if (doorEnvironmentsTable != null)
            {
                foreach (var carDoorEnvironment in carDoorEnvironments)
                {
                    var doorEnvironmentId = carDoorEnvironment.DoorEnvironment?.IdDoorEnvironment;
                    if (doorEnvironmentId != null && doorEnvironmentId != Guid.Empty)
                        carDoorEnvironment.DoorEnvironment = doorEnvironmentsTable.GetObjectById(doorEnvironmentId.Value);
                }
            }

            _doorEnvironments.Clear();
            _doorEnvironments.AddRange(carDoorEnvironments);
            LoadDoorEnvironments();
        }

        private void _tpDoorEnvironment_Enter(object sender, EventArgs e)
        {
            RefreshDoorEnvironmentsFromServer();
        }

        private void _bAddDoorEnvironment_Click(object sender, EventArgs e)
        {
            AddDoorEnvironment();
        }

        private void _bEditDoorEnvironment_Click(object sender, EventArgs e)
        {
            EditSelectedDoorEnvironment();
        }

        private void _bDeleteDoorEnvironment_Click(object sender, EventArgs e)
        {
            RemoveSelectedDoorEnvironment();
        }

        private void _dgDoorEnvironments_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            EditSelectedDoorEnvironment();
        }

        private void AddDoorEnvironment()
        {
            Exception error;
            var availableDoorEnvironments = GetAvailableDoorEnvironments(out error);
            if (error != null)
            {
                MessageBox.Show(error.Message);
                return;
            }

            if (availableDoorEnvironments == null || availableDoorEnvironments.Count == 0)
            {
                MessageBox.Show(GetString("CarEditForm_DoorEnvironmentAdd"));
                return;
            }

            using (var selectForm = new DoorEnvironmentSelectionForm(
                       availableDoorEnvironments,
                       GetString("CarEditForm_tpDoorEnvironment"),
                       GetString("Name"),
                       _tcAccessTypeColumn.HeaderText,
                       GetString("CarEditForm_bAddDoorEnvironment"),
                       GetString("General_bCancel")))
            {
                if (selectForm.ShowDialog(this) != DialogResult.OK)
                    return;

                foreach (var selectedDoorEnvironment in selectForm.SelectedDoorEnvironments)
                {
                    if (selectedDoorEnvironment == null)
                        continue;

                    if (_doorEnvironments.Any(de => de.DoorEnvironment != null && de.DoorEnvironment.IdDoorEnvironment == selectedDoorEnvironment.IdDoorEnvironment))
                        continue;

                    var carDoorEnvironment = new CarDoorEnvironment
                    {
                        DoorEnvironment = selectedDoorEnvironment,
                        Car = _editingObject,
                        AccessType = selectForm.SelectedAccessType
                    };
                    _doorEnvironments.Add(carDoorEnvironment);
                    if (_editingObject.IdCar != Guid.Empty)
                        TryInsertCarDoorEnvironment(carDoorEnvironment);
                }
                LoadDoorEnvironments();
            }
        }

        private void EditSelectedDoorEnvironment()
        {
            var selected = GetSelectedDoorEnvironment();
            if (selected == null)
                return;

            using (var editForm = new CarDoorEnvironmentAccessTypeForm(_tcAccessTypeColumn.HeaderText, selected.AccessType))
            {
                if (editForm.ShowDialog(this) != DialogResult.OK)
                    return;

                selected.AccessType = editForm.SelectedAccessType;

                if (selected.IdCarDoorEnvironment != Guid.Empty)
                    TryUpdateCarDoorEnvironment(selected);

                LoadDoorEnvironments();
            }
        }

        private void RemoveSelectedDoorEnvironment()
        {
            var selected = GetSelectedDoorEnvironment();
            if (selected == null)
                return;

            if (selected.IdCarDoorEnvironment != Guid.Empty)
                TryDeleteCarDoorEnvironment(selected);

            _doorEnvironments.Remove(selected);
            LoadDoorEnvironments();
        }

        private CarDoorEnvironment GetSelectedDoorEnvironment()
        {
            if (!(_dgDoorEnvironments.DataGrid.CurrentRow?.DataBoundItem is CarDoorEnvironmentView view))
                return null;

            if (view.DoorEnvironmentId != Guid.Empty)
                return _doorEnvironments.FirstOrDefault(de => de.DoorEnvironment != null && de.DoorEnvironment.IdDoorEnvironment == view.DoorEnvironmentId);

            return _doorEnvironments.FirstOrDefault(de => string.Equals(de.DoorEnvironment?.Name, view.DoorEnvironmentName));
        }

        private ICollection<DoorEnvironment> GetAvailableDoorEnvironments(out Exception error)
        {
            error = null;

            var ncasProvider = GetNcasProvider(out error);
            var doorEnvironmentsTable = ncasProvider?.DoorEnvironments;
            var allDoorEnvironments = doorEnvironmentsTable?.List(out error);
            if (doorEnvironmentsTable == null)
            {
                error = new MissingFieldException(
                    typeof(ICgpNCASRemotingProvider).FullName,
                    nameof(ICgpServerRemotingProvider.DoorEnvironments));
            }

            if (error != null || allDoorEnvironments == null)
                return allDoorEnvironments;

            var provider = GetCarProvider(out error);
            if (provider == null)
                return allDoorEnvironments;

            var assignedDoorEnvironmentIds = new HashSet<Guid>(
                _doorEnvironments
                    .Where(cde => cde.DoorEnvironment != null)
                    .Select(cde => cde.DoorEnvironment.IdDoorEnvironment));

            return allDoorEnvironments
                .Where(de => !assignedDoorEnvironmentIds.Contains(de.IdDoorEnvironment))
                .ToList();
        }

        private ICarDoorEnvironments GetCarDoorEnvironmentsTable(out Exception error)
        {
            error = null;

            var provider = GetNcasProvider(out error);
            if (provider == null)
                return null;
            var carDoorEnvironments = provider.CarDoorEnvironments;
            if (carDoorEnvironments == null)
            {
                error = new MissingMemberException(
                    typeof(ICgpNCASRemotingProvider).FullName,
                    nameof(ICgpNCASRemotingProvider.CarDoorEnvironments));
            }
            return carDoorEnvironments;
        }

        private void TryInsertCarDoorEnvironment(CarDoorEnvironment carDoorEnvironment)
        {
            try
            {
                var table = GetCarDoorEnvironmentsTable(out var error);
                if (error != null)
                {
                    MessageBox.Show(error.Message);
                    return;
                }

                if (table == null || carDoorEnvironment == null)
                    return;

                var insertResult = table.Insert(ref carDoorEnvironment, out var insertError);
                if (insertResult != true && insertError != null)
                    MessageBox.Show(insertError.Message);
            }
            catch (MissingMethodException)
            {
                MessageBox.Show("Current server version does not support car door environments. Please update the server.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TryUpdateCarDoorEnvironment(CarDoorEnvironment carDoorEnvironment)
        {
            try
            {
                var table = GetCarDoorEnvironmentsTable(out var error);
                if (error != null)
                {
                    MessageBox.Show(error.Message);
                    return;
                }


                if (table == null || carDoorEnvironment == null)
                    return;

                var updateResult = table.Update(carDoorEnvironment, out var updateError);

                if (updateResult != true && updateError != null)
                    MessageBox.Show(updateError.Message);
            }
            catch (MissingMethodException)
            {
                MessageBox.Show(
                    "Current server version does not support car door environments. Please update the server.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TryDeleteCarDoorEnvironment(CarDoorEnvironment carDoorEnvironment)
        {
            try
            {
                var table = GetCarDoorEnvironmentsTable(out var error);
                if (error != null)
                {
                    MessageBox.Show(error.Message);
                    return;
                }

                if (table == null || carDoorEnvironment == null)
                    return;
                var deleteResult = table.Delete(carDoorEnvironment, out var deleteError);

                if (deleteResult != true && deleteError != null)
                    MessageBox.Show(deleteError.Message);
            }
            catch (MissingMethodException)
            {
                MessageBox.Show(
                    "Current server version does not support car door environments. Please update the server.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private ICgpRemotingProvider GetCarProvider(out Exception error)
        {
            error = null;

            var provider = Plugin.MainServerProvider as ICgpRemotingProvider
                           ?? CgpClient.Singleton.MainServerProvider as ICgpRemotingProvider;
            if (provider == null)
            {
                error = new MissingFieldException(
                    typeof(ICgpRemotingProvider).FullName,
                    nameof(ICgpRemotingProvider));
            }

            return provider;
        }

        private ICgpNCASRemotingProvider GetNcasProvider(out Exception error)
        {
            error = null;

            var provider = Plugin.MainServerProvider as ICgpNCASRemotingProvider
                           ?? CgpClient.Singleton.MainServerProvider as ICgpNCASRemotingProvider;
            if (provider == null)
            {
                error = new MissingFieldException(
                    typeof(ICgpNCASRemotingProvider).FullName,
                    nameof(ICgpNCASRemotingProvider));
            }

            return provider;
        }

        private class CarDoorEnvironmentView
        {
            public Image Symbol { get; set; }
            public Guid DoorEnvironmentId { get; set; }
            public string DoorEnvironmentName { get; set; }

            public CarDoorEnvironmentAccessType AccessType { get; set; }
        }

        private class CarDoorEnvironmentAccessTypeForm : Form
        {
            private readonly ComboBox _cbAccessType;

            public CarDoorEnvironmentAccessTypeForm(string accessTypeTitle, CarDoorEnvironmentAccessType currentAccessType)
            {
                Text = "editCarDoorEnvironment";
                FormBorderStyle = FormBorderStyle.FixedDialog;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                ShowInTaskbar = false;
                Size = new Size(350, 170);

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2,
                    Padding = new Padding(10)
                };

                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                var label = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = accessTypeTitle,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                _cbAccessType = new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                _cbAccessType.DataSource = Enum.GetValues(typeof(CarDoorEnvironmentAccessType));
                _cbAccessType.SelectedItem = currentAccessType;

                var buttonsPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.RightToLeft,
                    Dock = DockStyle.Fill
                };

                var ok = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    AutoSize = true
                };

                var cancel = new Button
                {
                    Text = "Cancel",
                    DialogResult = DialogResult.Cancel,
                    AutoSize = true
                };

                buttonsPanel.Controls.Add(ok);
                buttonsPanel.Controls.Add(cancel);

                layout.Controls.Add(label, 0, 0);
                layout.Controls.Add(_cbAccessType, 1, 0);
                layout.Controls.Add(buttonsPanel, 0, 1);
                layout.SetColumnSpan(buttonsPanel, 2);

                Controls.Add(layout);
            }

            public CarDoorEnvironmentAccessType SelectedAccessType =>
                _cbAccessType.SelectedItem is CarDoorEnvironmentAccessType accessType
                    ? accessType
                    : CarDoorEnvironmentAccessType.None;
        }


        private class DoorEnvironmentSelectionForm : Form
        {
            private readonly ListView _lvDoorEnvironments;
            private readonly Button _bAdd;
            private readonly Button _bCancel;
            private readonly ComboBox _cbAccessType;

            internal IList<DoorEnvironment> SelectedDoorEnvironments =>
                _lvDoorEnvironments.CheckedItems
                    .Cast<ListViewItem>()
                    .Select(item => item.Tag as DoorEnvironment)
                    .Where(doorEnvironment => doorEnvironment != null)
                    .ToList();

            internal CarDoorEnvironmentAccessType SelectedAccessType =>
                _cbAccessType.SelectedItem is CarDoorEnvironmentAccessType accessType
                    ? accessType
                    : CarDoorEnvironmentAccessType.None;

            internal DoorEnvironmentSelectionForm(
                IEnumerable<DoorEnvironment> doorEnvironments,
                string title,
                string nameColumnHeader,
                string accessTypeHeader,
                string addText,
                string cancelText)
            {
                Text = title;
                Width = 400;
                Height = 500;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                FormBorderStyle = FormBorderStyle.FixedDialog;

                _lvDoorEnvironments = new ListView
                {
                    Dock = DockStyle.Top,
                    Height = 330,
                    View = View.Details,
                    CheckBoxes = true,
                    FullRowSelect = true
                };
                _lvDoorEnvironments.Columns.Add(nameColumnHeader, -2, HorizontalAlignment.Left);

                foreach (var doorEnvironment in doorEnvironments)
                {
                    var item = new ListViewItem(doorEnvironment.Name) { Tag = doorEnvironment };
                    _lvDoorEnvironments.Items.Add(item);
                }

                var accessTypePanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 40,
                    Padding = new Padding(10, 5, 10, 5)
                };

                var accessTypeLabel = new Label
                {
                    Text = accessTypeHeader,
                    Dock = DockStyle.Left,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Width = 120
                };

                _cbAccessType = new ComboBox
                {
                    Dock = DockStyle.Fill,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                _cbAccessType.DataSource = Enum.GetValues(typeof(CarDoorEnvironmentAccessType));

                accessTypePanel.Controls.Add(_cbAccessType);
                accessTypePanel.Controls.Add(accessTypeLabel);

                _bAdd = new Button
                {
                    Text = addText,
                    DialogResult = DialogResult.OK,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Width = 80,
                    Left = 200,
                    Top = 380
                };

                _bCancel = new Button
                {
                    Text = cancelText,
                    DialogResult = DialogResult.Cancel,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Width = 80,
                    Left = 290,
                    Top = 380
                };

                Controls.Add(_bAdd);
                Controls.Add(_bCancel);
                Controls.Add(accessTypePanel);
                Controls.Add(_lvDoorEnvironments);
            }
        }
    }
}
