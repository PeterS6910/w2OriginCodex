using Contal.Cgp.BaseLib;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.Server.Beans;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
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
            _bCreateDoorEnvironment.Text = GetString("CarEditForm_bCreateDoorEnvironment");
            _bDeleteDoorEnvironment.Text = GetString("CarEditForm_bDeleteDoorEnvironment");
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
            _lvDoorEnvironments.Items.Clear();
            _doorEnvironments.Clear();
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
            foreach (var doorEnvironment in _doorEnvironments.OrderBy(de => de.DoorEnvironment?.Name))
            {
                var item = new ListViewItem(doorEnvironment.DoorEnvironment?.Name ?? string.Empty)
                {
                    Tag = doorEnvironment
                };
                item.SubItems.Add(doorEnvironment.AccessType.ToString());
                _lvDoorEnvironments.Items.Add(item);
            }
        }

        private void RefreshDoorEnvironmentsFromServer()
        {
            var provider = CgpClient.Singleton.MainServerProvider;
            var carDoorEnvironments = InvokeListProvider<CarDoorEnvironment>(provider, "CarDoorEnvironments", out var error)
                ?.Where(cde => cde.Car != null && cde.Car.IdCar == _editingObject.IdCar)
                .ToList();

            if (error != null || carDoorEnvironments == null)
                return;

            _doorEnvironments.Clear();
            _doorEnvironments.AddRange(carDoorEnvironments);
        }

        private void _bAddDoorEnvironment_Click(object sender, EventArgs e)
        {
            Exception error;
            var doorEnvironments = GetDoorEnvironments(out error);
            if (error != null)
            {
                MessageBox.Show(error.Message);
                return;
            }

            var availableDoorEnvironments = doorEnvironments
                ?.Where(de => _doorEnvironments.All(added => added.DoorEnvironment == null || added.DoorEnvironment.IdDoorEnvironment != de.IdDoorEnvironment))
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
                        if (_doorEnvironments.Any(de => de.DoorEnvironment != null && de.DoorEnvironment.IdDoorEnvironment == selectedDoorEnvironment.IdDoorEnvironment))
                            continue;

                        _doorEnvironments.Add(new CarDoorEnvironment
                        {
                            DoorEnvironment = selectedDoorEnvironment,
                            Car = _editingObject,
                            AccessType = CarDoorEnvironmentAccessType.None
                        });
                    }

                    LoadDoorEnvironments();
                }
            }
        }

        private ICollection<DoorEnvironment> GetDoorEnvironments(out Exception error)
        {
            error = null;

            var provider = CgpClient.Singleton.MainServerProvider;
            var allDoorEnvironments = InvokeListProvider<DoorEnvironment>(provider, "DoorEnvironments", out error);
            if (error != null || allDoorEnvironments == null)
                return allDoorEnvironments;

            var carDoorEnvironments = InvokeListProvider<CarDoorEnvironment>(provider, "CarDoorEnvironments", out error);
            if (error != null)
                return null;

            if (carDoorEnvironments == null)
                return allDoorEnvironments.ToList();

            var assignedDoorEnvironmentIds = new HashSet<Guid>(
                carDoorEnvironments
                    .Where(cde => cde.Car != null && cde.Car.IdCar == _editingObject.IdCar && cde.DoorEnvironment != null)
                    .Select(cde => cde.DoorEnvironment.IdDoorEnvironment));

            return allDoorEnvironments
                .Where(de => !assignedDoorEnvironmentIds.Contains(de.IdDoorEnvironment))
                .ToList();
        }

        private static ICollection<T> InvokeListProvider<T>(object provider, string propertyName, out Exception error) where T : class
        {
            error = null;
            if (provider == null)
                return null;

            var providerType = provider.GetType();
            var providerProperty = providerType.GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (providerProperty == null)
            {
                providerProperty = providerType
                    .GetInterfaces()
                    .Select(iface => iface.GetProperty(
                        propertyName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                    .FirstOrDefault(prop => prop != null);
            }

            object tableProvider = null;

            if (providerProperty != null)
            {
                tableProvider = providerProperty.GetValue(provider, null);
            }
            else
            {
                var isRemoteProxy = RemotingServices.IsTransparentProxy(provider);

                if (!isRemoteProxy)
                {
                    var providerPropertyDescriptor = TypeDescriptor.GetProperties(provider).Find(propertyName, true);
                    if (providerPropertyDescriptor != null)
                    {
                        tableProvider = providerPropertyDescriptor.GetValue(provider);
                    }
                }
                if (tableProvider == null)
                {
                    error = isRemoteProxy
    ? new NotSupportedException(
        $"The remoted provider type '{providerType.FullName}' does not expose property '{propertyName}'.")
    : new MissingMemberException(providerType.FullName, propertyName);
                    return null;
                }
            }

            if (tableProvider == null)
                return null;

            var listMethod = tableProvider.GetType().GetMethod("List", new[] { typeof(Exception).MakeByRefType() });
            if (listMethod == null)
            {
                error = new MissingMethodException(tableProvider.GetType().FullName, "List");
                return null;
            }

            var parameters = new object[] { null };
            var result = listMethod.Invoke(tableProvider, parameters) as ICollection<T>;
            error = parameters[0] as Exception;
            return result;
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
