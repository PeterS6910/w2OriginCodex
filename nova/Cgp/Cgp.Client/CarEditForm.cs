using Contal.Cgp.BaseLib;
using Contal.Cgp.Globals;
using Contal.Cgp.RemotingCommon;
using Contal.Cgp.NCAS.RemotingCommon;
using Contal.Cgp.NCAS.Server;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.Server.Beans;
using Contal.IwQuick;
using Contal.IwQuick.UI;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Contal.IwQuick.Sys;

namespace Contal.Cgp.Client
{
    public partial class CarEditForm :
#if DESIGNER
        Form
#else
        ACgpEditForm<Car>
#endif
    {
        private BindingSource _aclCarsBindingSource;
        public CarEditForm(Car car, ShowOptionsEditForm showOption)
            : base(car, showOption)
        {
            InitializeComponent();
            _editingObject = car;
            _ilbCards.ImageList = ObjectImageList.Singleton.ClientObjectImages;
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
            _ilbCards.Items.Clear();
            _eFilterCards.Text = string.Empty;
        }

        protected override void SetValuesEdit()
        {
            _eLp.Text = _editingObject.Lp;
            _eBrand.Text = _editingObject.Brand;
            _dpValidityDateFrom.Value = _editingObject.ValidityDateFrom;
            _dpValidityDateTo.Value = _editingObject.ValidityDateTo;
            _eDescription.Text = _editingObject.Description;
            LoadCards(_eFilterCards.Text);
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


        private void LoadCards(string filter)
        {
            _ilbCards.Items.Clear();
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

            IList<Card> assigned = provider.CarCards.GetCardsForCar(_editingObject.IdCar, out error);
            if (assigned != null)
            {
                foreach (var card in assigned)
                {
                    string cardText = BuildCardDescription(card, provider);
                    if (string.IsNullOrEmpty(filter) || cardText.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        _ilbCards.Items.Add(new ImageListBoxItem(new CardDisplayItem(card, cardText), card.GetSubTypeImageString("State")));
                    }
                }
            }
        }

        private string BuildCardDescription(Card card, ICgpServerRemotingProvider provider)
        {
            string cardText = card.ToString();
            string personName = GetPersonDisplayName(provider, card.GuidPerson);
            if (!string.IsNullOrWhiteSpace(personName))
            {
                cardText = $"{cardText}-{personName}";
            }

            return cardText;
        }

        private string GetPersonDisplayName(ICgpServerRemotingProvider provider, Guid personId)
        {
            if (personId == Guid.Empty || provider == null)
                return string.Empty;

            Person person = provider.Persons.GetObjectById(personId);
            return person == null ? string.Empty : person.ToString();
        }


        private void _bAddCard_Click(object sender, EventArgs e)
        {
            if (AddCards())
            {
                LoadCards(_eFilterCards.Text);
            }
        }

        private void _bCreateCard_Click(object sender, EventArgs e)
        {
            if (_editingObject.IdCar == Guid.Empty)
            {
                Ok_Click(false);
            }

            if (_editingObject.IdCar != Guid.Empty)
            {
                var person = SelectPersonForNewCard();
                if (person == null)
                    return;

                Card card = new Card();
                card.Person = person;
                CardsForm.Singleton.OpenInsertFromEdit(ref card, DoAfterCreateCard);
            }
        }

        private void DoAfterCreateCard(object card)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>(DoAfterCreateCard), card);
            }
            else
            {
                var createdCard = card as Card;
                if (createdCard != null && _editingObject.IdCar != Guid.Empty)
                {
                    var provider = GetCarProvider(out var providerError);
                    if (provider == null)
                    {
                        if (providerError != null)
                            MessageBox.Show(providerError.Message);
                    }
                    else
                    {
                        provider.CarCards.AssignCardToCar(_editingObject.IdCar, createdCard.IdCard);
                    }
                }

                LoadCards(_eFilterCards.Text);
            }
        }

        private Person SelectPersonForNewCard()
        {
            if (CgpClient.Singleton.IsConnectionLost(true))
                return null;

            try
            {
                Exception error;
                IList<IModifyObject> listPersons =
                    CgpClient.Singleton.MainServerProvider.Persons.ListModifyObjects(out error);

                if (error != null)
                {
                    MessageBox.Show(error.Message);
                    return null;
                }

                if (listPersons == null || listPersons.Count == 0)
                    return null;

                ListboxFormAdd formAdd = new ListboxFormAdd(listPersons, GetString("PersonsFormPersonsForm"));
                IModifyObject outModObj;
                formAdd.ShowDialog(out outModObj);

                if (outModObj == null)
                    return null;

                return CgpClient.Singleton.MainServerProvider.Persons.GetObjectById(outModObj.GetId);
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
                MessageBox.Show(error.Message);
                return null;
            }
        }

        private bool AddCards()
        {
            if (_editingObject.IdCar == Guid.Empty)
            {
                Ok_Click(false);
                return false;
            }

            var provider = GetCarProvider(out var providerError);
            if (provider == null)
            {
                if (providerError != null)
                    MessageBox.Show(providerError.Message);
                return false;
            }

            IList<IModifyObject> listCards = new List<IModifyObject>();
            listCards = provider.Cards.ModifyObjectsFormCarAddCard(_editingObject.IdCar, out var error);
            if (error != null)
            {
                MessageBox.Show(error.Message);
                return false;
            }

            IList<object> cardIds = GetCardIdsFromModifyObjects(listCards);
            IList<Card> availableCards = provider.Cards.GetCardsByListGuids(cardIds);
            var cardLookup = new Dictionary<Guid, Card>();
            if (availableCards != null)
            {
                foreach (var card in availableCards)
                {
                    cardLookup[card.IdCard] = card;
                }
            }

            List<IModifyObject> displayCards = new List<IModifyObject>();
            foreach (var modifyObject in listCards)
            {
                string displayName = modifyObject.FullName;
                if (cardLookup.TryGetValue(modifyObject.GetId, out var card))
                {
                    displayName = BuildCardDescription(card, provider);
                }

                displayCards.Add(new CardModifyDisplayObject(modifyObject, displayName));
            }

            ListboxFormAdd formAdd = new ListboxFormAdd(displayCards, GetString("CardsFormCardsForm"), true);
            ListOfObjects outCards;
            formAdd.ShowDialogMultiSelect(out outCards);
            if (outCards != null)
            {
                IList<Card> selectedCards = provider.Cards.GetCardsByListGuids(CalGuidFromListObj(outCards));
                if (selectedCards != null)
                {
                    foreach (var card in selectedCards)
                    {
                        provider.CarCards.AssignCardToCar(_editingObject.IdCar, card.IdCard);
                    }

                }
                return true;
            }
            return false;
        }

        private IList<object> CalGuidFromListObj(ListOfObjects cards)
        {
            IList<object> listGuids = new List<object>();
            foreach (object cardLo in cards)
            {
                listGuids.Add((cardLo as IModifyObject).GetId);
            }
            return listGuids;
        }

        private IList<object> GetCardIdsFromModifyObjects(IEnumerable<IModifyObject> cards)
        {
            IList<object> listGuids = new List<object>();
            foreach (var card in cards)
            {
                listGuids.Add(card.GetId);
            }

            return listGuids;
        }

        private void _bDeleteCard_Click(object sender, EventArgs e)
        {
            ListBox.SelectedObjectCollection selectedItems = _ilbCards.SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
                return;

            var provider = GetCarProvider(out var providerError);
            if (provider == null)
            {
                if (providerError != null)
                    MessageBox.Show(providerError.Message);
                return;
            }

            foreach (object obj in selectedItems)
            {
                var card = GetCardFromListItem(obj as ImageListBoxItem);
                if (card != null)
                {
                    provider.CarCards.UnassignCardFromCar(_editingObject.IdCar, card.IdCard);
                }
            }

            LoadCards(_eFilterCards.Text);
        }

        private void _eFilterCards_KeyUp(object sender, KeyEventArgs e)
        {
            LoadCards(_eFilterCards.Text);
        }

        private void _tpCards_Enter(object sender, EventArgs e)
        {
            LoadCards(_eFilterCards.Text);
        }

        private void _tpAccessControlList_Enter(object sender, EventArgs e)
        {
            LoadAclCars();
        }

        private void _ilbCards_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var card = GetCardFromSelectedItem();
            if (card != null)
                CardsForm.Singleton.OpenEditForm(card, DoAfterCardEdited);
        }

        private void DoAfterCardEdited(object card)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>(DoAfterCardEdited), card);
            }
            else
            {
                LoadCards(_eFilterCards.Text);
            }
        }

        private Card GetCardFromSelectedItem()
        {
            return GetCardFromListItem(_ilbCards.SelectedItem as ImageListBoxItem);
        }

        private Card GetCardFromListItem(ImageListBoxItem item)
        {
            if (item == null)
                return null;

            if (item.MyObject is CardDisplayItem cardDisplayItem)
                return cardDisplayItem.Card;

            return item.MyObject as Card;
        }

        private sealed class CardDisplayItem
        {
            public CardDisplayItem(Card card, string displayText)
            {
                Card = card;
                DisplayText = displayText;
            }

            public Card Card { get; }
            public string DisplayText { get; }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        private sealed class CardModifyDisplayObject : IModifyObject
        {
            private readonly IModifyObject _inner;

            public CardModifyDisplayObject(IModifyObject inner, string fullName)
            {
                _inner = inner;
                FullName = fullName;
            }

            public string FullName { get; set; }

            public bool Contains(string expression)
            {
                if (string.IsNullOrEmpty(expression))
                    return true;

                if (_inner.Contains(expression))
                    return true;

                return AOrmObject.RemoveDiacritism(FullName)
                    .ToLower()
                    .Contains(AOrmObject.RemoveDiacritism(expression).ToLower());
            }

            public ObjectType GetOrmObjectType => _inner.GetOrmObjectType;

            public string GetObjectSubType(byte option)
            {
                return _inner.GetObjectSubType(option);
            }

            public Guid GetId => _inner.GetId;

            public override string ToString()
            {
                return FullName;
            }
        }

        private void _bCancel_Click(object sender, EventArgs e)
        {
            Cancel_Click();
        }

        private void _bOk_Click(object sender, EventArgs e)
        {
            Ok_Click();
        }

        private void LoadAclCars()
        {
            if (CgpClient.Singleton.IsConnectionLost(true))
                return;

            if (_editingObject.IdCar == Guid.Empty)
            {
                _aclCarsBindingSource = null;
                _cdgvAclCars.DataGrid.DataSource = null;
                return;
            }

            var ncasProvider = CgpClient.Singleton.MainServerProvider as ICgpNCASRemotingProvider;
            if (ncasProvider == null)
                return;

            Exception error;
            IList<ACLCar> aclCars = null;
            try
            {
                aclCars = ncasProvider.ACLCars.GetAclCarsByCar(_editingObject.IdCar, out error);
            }
            catch (MissingMethodException)
            {
                _aclCarsBindingSource = null;
                _cdgvAclCars.DataGrid.DataSource = null;
                Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorLoadTable"));
                return;
            }
            if (error != null)
            {
                if (error is AccessDeniedException)
                {
                    Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorLoadTableAccessDenied"));
                }
                else
                {
                    Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorLoadTable"));
                }

                _aclCarsBindingSource = null;
                _cdgvAclCars.DataGrid.DataSource = null;
                return;
            }

            _aclCarsBindingSource = new BindingSource
            {
                DataSource = aclCars
            };

            _cdgvAclCars.ModifyGridView(
                _aclCarsBindingSource,
                ACLCar.COLUMN_ACCESS_CONTROL_LIST,
                ACLCar.COLUMN_DATE_FROM,
                ACLCar.COLUMN_DATE_TO);
            _cdgvAclCars.DataGrid.Columns[ACLCar.COLUMN_DATE_FROM].DefaultCellStyle.Format =
                "MM-dd-yyyy HH:mm:ss";
            _cdgvAclCars.DataGrid.Columns[ACLCar.COLUMN_DATE_FROM].AutoSizeMode =
                DataGridViewAutoSizeColumnMode.DisplayedCells;
            _cdgvAclCars.DataGrid.Columns[ACLCar.COLUMN_DATE_TO].DefaultCellStyle.Format =
                "MM-dd-yyyy HH:mm:ss";
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
