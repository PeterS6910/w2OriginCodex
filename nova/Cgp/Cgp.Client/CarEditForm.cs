using Contal.Cgp.BaseLib;
using Contal.Cgp.Globals;
using Contal.Cgp.RemotingCommon;
using Contal.Cgp.NCAS.RemotingCommon;
using Contal.Cgp.NCAS.Server;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.Server.Beans;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

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
        }


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
        }

        protected override void SetValuesEdit()
        {
            _eLp.Text = _editingObject.Lp;
            _eBrand.Text = _editingObject.Brand;
            _dpValidityDateFrom.Value = _editingObject.ValidityDateFrom;
            _dpValidityDateTo.Value = _editingObject.ValidityDateTo;
            _eDescription.Text = _editingObject.Description;
            LoadCards();
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

        private ICgpServerRemotingProvider GetCarProvider(out Exception error)
        {
            error = null;

            var provider = CgpClient.Singleton.MainServerProvider as ICgpServerRemotingProvider
                           ?? CgpClient.Singleton.MainServerProvider as ICgpServerRemotingProvider;
            if (provider == null)
            {
                error = new MissingFieldException(
                    typeof(ICgpServerRemotingProvider).FullName,
                    nameof(ICgpServerRemotingProvider));
            }

            return provider;
        }
    }
}
