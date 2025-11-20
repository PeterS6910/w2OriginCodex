using Contal.Cgp.BaseLib;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.Server.Beans;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

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
            _cbSecurityLevel.DataSource = Enum.GetValues(typeof(CarSecurityLevel));
            SetReferenceEditColors();
            _tcCar.SelectedIndexChanged += _tcCar_SelectedIndexChanged;
            _tcCar_SelectedIndexChanged(null, null);
            _tpDoorEnvironment.Text = GetString("CarEditForm_tpDoorEnvironment");
            _bAddDoorEnvironment.Text = GetString("CarEditForm_bAddDoorEnvironment");
            _bCreateDoorEnvironment.Text = GetString("CarEditForm_bCreateDoorEnvironment");
            _bDeleteDoorEnvironment.Text = GetString("CarEditForm_bDeleteDoorEnvironment");
        }

        private readonly List<DoorEnvironment> _doorEnvironments = new List<DoorEnvironment>();

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
            if (_cbSecurityLevel.SelectedItem != null)
                _editingObject.SecurityLevel = (CarSecurityLevel)_cbSecurityLevel.SelectedItem;
            else
                _editingObject.SecurityLevel = CarSecurityLevel.None;
            _editingObject.Description = _eDescription.Text;

            return true;
        }

        protected override bool SaveToDatabaseInsert()
        {
            Exception error;
            return CgpClient.Singleton.MainServerProvider.Cars.Insert(ref _editingObject, out error);
        }

        protected override bool SaveToDatabaseEdit()
        {
            Exception error;
            return CgpClient.Singleton.MainServerProvider.Cars.Update(_editingObject, out error);
        }

        protected override bool SaveToDatabaseEditOnlyInDatabase()
        {
            Exception error;
            return CgpClient.Singleton.MainServerProvider.Cars.UpdateOnlyInDatabase(_editingObject, out error);
        }

        protected override void RegisterEvents()
        {
        }

        protected override void UnregisterEvents()
        {
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
            _cbSecurityLevel.SelectedItem = CarSecurityLevel.None;
            _eDescription.Text = string.Empty;
            _lvAssignedCards.Items.Clear();
            _lvAvailableCards.Items.Clear();
            _lvDoorEnvironments.Items.Clear();
            _doorEnvironments.Clear();
        }

        protected override void SetValuesEdit()
        {
            _eLp.Text = _editingObject.Lp;
            _eBrand.Text = _editingObject.Brand;
            _dpValidityDateFrom.Value = _editingObject.ValidityDateFrom;
            _dpValidityDateTo.Value = _editingObject.ValidityDateTo;
            _cbSecurityLevel.SelectedItem = _editingObject.SecurityLevel;
            _eDescription.Text = _editingObject.Description;
            LoadCards();
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
            ICollection<Card> allCardsCollection =
                CgpClient.Singleton.MainServerProvider.Cards.List(out error);
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
            IList<Card> assigned = CgpClient.Singleton.MainServerProvider.CarCards.GetCardsForCar(_editingObject.IdCar, out error);
            var assignedSet = new HashSet<Guid>();
            if (assigned != null)
            {
                foreach (var card in assigned)
                {
                    assignedSet.Add(card.IdCard);
                    string cardText = card.FullCardNumber.ToString();
                    if (card.Person != null && card.GuidPerson != Guid.Empty)
                    {
                        var person = CgpClient.Singleton.MainServerProvider.Persons.GetObjectById(card.GuidPerson);
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
                        var person = CgpClient.Singleton.MainServerProvider.Persons.GetObjectById(card.GuidPerson);
                        cardText += " - " + person.FirstName + " " + person.Surname;
                    }
                    _lvAvailableCards.Items.Add(new ListViewItem(cardText) { Tag = card });
                }
            }
        }

        private static string BuildCardDescription(Card card)
        {
            string cardText = card.FullCardNumber.ToString();

            if (card.GuidPerson != Guid.Empty)
            {
                Person person = CgpClient.Singleton.MainServerProvider.Persons.GetObjectById(card.GuidPerson);
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
                    CgpClient.Singleton.MainServerProvider.CarCards.UnassignCardFromCar(_editingObject.IdCar, card.IdCard);
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
                    CgpClient.Singleton.MainServerProvider.CarCards.AssignCardToCar(_editingObject.IdCar, card.IdCard);
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
            _lvDoorEnvironments.Items.Clear();
            foreach (var doorEnvironment in _doorEnvironments.OrderBy(de => de.Name))
            {
                _lvDoorEnvironments.Items.Add(
                    new ListViewItem(doorEnvironment.Name) { Tag = doorEnvironment });
            }
        }

        private void _bAddDoorEnvironment_Click(object sender, EventArgs e)
        {
            Exception error;
            var doorEnvironments = CgpClient.Singleton.MainServerProvider.DoorEnvironments.List(out error);
            if (error != null)
            {
                MessageBox.Show(error.Message);
                return;
            }

            var availableDoorEnvironments = doorEnvironments
                ?.Where(de => _doorEnvironments.All(added => added.IdDoorEnvironment != de.IdDoorEnvironment))
                .OrderBy(de => de.Name)
                .ToList();

            if (availableDoorEnvironments == null || availableDoorEnvironments.Count == 0)
            {
                MessageBox.Show(GetString("CarEditForm_DoorEnvironmentAdd"));
                return;
            }

            using (var selectForm = new DoorEnvironmentSelectionForm(
                       availableDoorEnvironments,
                       GetString("CarEditForm_tpDoorEnvironment"),
                       GetString("Name"),
                       GetString("CarEditForm_bAddDoorEnvironment"),
                       GetString("General_bCancel")))
            {
                if (selectForm.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (var selectedDoorEnvironment in selectForm.SelectedDoorEnvironments)
                    {
                        if (_doorEnvironments.Any(de => de.IdDoorEnvironment == selectedDoorEnvironment.IdDoorEnvironment))
                            continue;

                        _doorEnvironments.Add(selectedDoorEnvironment);
                    }

                    LoadDoorEnvironments();
                }
            }
        }

        private void _bCreateDoorEnvironment_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GetString("CarEditForm_DoorEnvironmentCreate"));
        }

        private void _bDeleteDoorEnvironment_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GetString("CarEditForm_DoorEnvironmentDelete"));
        }


        private class DoorEnvironmentSelectionForm : Form
        {
            private readonly ListView _lvDoorEnvironments;
            private readonly Button _bAdd;
            private readonly Button _bCancel;

            internal IList<DoorEnvironment> SelectedDoorEnvironments =>
                _lvDoorEnvironments.CheckedItems
                    .Cast<ListViewItem>()
                    .Select(item => item.Tag as DoorEnvironment)
                    .Where(doorEnvironment => doorEnvironment != null)
                    .ToList();

            internal DoorEnvironmentSelectionForm(
                IEnumerable<DoorEnvironment> doorEnvironments,
                string title,
                string nameColumnHeader,
                string addText,
                string cancelText)
            {
                Text = title;
                Width = 400;
                Height = 450;
                StartPosition = FormStartPosition.CenterParent;
                MinimizeBox = false;
                MaximizeBox = false;
                FormBorderStyle = FormBorderStyle.FixedDialog;

                _lvDoorEnvironments = new ListView
                {
                    Dock = DockStyle.Top,
                    Height = 350,
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

                _bAdd = new Button
                {
                    Text = addText,
                    DialogResult = DialogResult.OK,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Width = 80,
                    Left = 200,
                    Top = 360
                };

                _bCancel = new Button
                {
                    Text = cancelText,
                    DialogResult = DialogResult.Cancel,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Width = 80,
                    Left = 290,
                    Top = 360
                };

                AcceptButton = _bAdd;
                CancelButton = _bCancel;

                Controls.Add(_lvDoorEnvironments);
                Controls.Add(_bAdd);
                Controls.Add(_bCancel);
            }
        }
    }
}
