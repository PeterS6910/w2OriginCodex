using Cgp.Components;
using Contal.Cgp.BaseLib;
using Contal.Cgp.Components;
using Contal.Cgp.Globals;
using Contal.Cgp.RemotingCommon;
using Contal.Cgp.Server.Beans;
using Contal.IwQuick;
using Contal.IwQuick.Sys;
using Contal.IwQuick.Threads;
using Contal.IwQuick.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
        private UserFoldersStructure _departmentFilter;
        private UserFoldersStructure _actDepartment;

        public CarEditForm(Car car, ShowOptionsEditForm showOption)
            : base(car, showOption)
        {
            InitializeComponent();
            _tbmDepartment.ImageTextBox.ContextMenuStrip = _tbmDepartment.ButtonPopupMenu;
            _editingObject = car;
            _ilbCards.ImageList = ObjectImageList.Singleton.ClientObjectImages;
            SetReferenceEditColors();
            FillSecurityLevelComboBox();

            if (showOption != ShowOptionsEditForm.InsertWithData)
            {
                if (Insert)
                {
                    _tcCar.TabPages.Remove(_tpAccessControlList);
                }
                else
                {
                    LoadNCASPluginTabPages(_editingObject, true);
                }
            }
        }


        protected override bool CheckValues()
        {
            return true;
        }

        protected override bool GetValues()
        {
            var provider = GetCarProvider(out var providerError);
            if (provider == null)
            {
                if (providerError != null)
                    MessageBox.Show(providerError.Message);

                return false;
            }
            _editingObject.Lp = _eLp.Text;
            _editingObject.Brand = _eBrand.Text;
            _editingObject.ValidityDateFrom = _dpValidityDateFrom.Value;
            _editingObject.ValidityDateTo = _dpValidityDateTo.Value;
            _editingObject.SecurityLevel = GetSelectedSecurityLevel();
            _editingObject.Department = _actDepartment;
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

            _tsiDepartmentModify.Text = GetString("General_bModify");
            _tsiDepartmentRemove.Text = GetString("General_bRemove");

            FillSecurityLevelComboBox();

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
            SelectSecurityLevel(CarSecurityLevel.StandardLprAndCard);
            SetActDepartment(null);
            _eDescription.Text = string.Empty;
            _ilbCards.Items.Clear();
            _eFilterCards.Text = string.Empty;
            _bApply.Enabled = false;
        }

        protected override void SetValuesEdit()
        {
            _eLp.Text = _editingObject.Lp;
            _eBrand.Text = _editingObject.Brand;
            _dpValidityDateFrom.Value = _editingObject.ValidityDateFrom;
            _dpValidityDateTo.Value = _editingObject.ValidityDateTo;
            SelectSecurityLevel(_editingObject.SecurityLevel);
            SetDepartmentFullName(_editingObject.Department);
            SetActDepartment(_editingObject.Department);
            _eDescription.Text = _editingObject.Description;
            LoadCards(_eFilterCards.Text);
            _bApply.Enabled = false;
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

        private void LoadNCASPluginTabPages(Car car, bool allowEdit)
        {
            if (!CgpClient.Singleton.LoadPluginControlToForm("NCAS plugin",
               car,
               _tpAccessControlList,
               this,
               allowEdit))
            {
                _tcCar.TabPages.Remove(_tpAccessControlList);
            }
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
                    bool textMatch = string.IsNullOrEmpty(filter) || cardText.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) == 0;
                    bool departmentMatch = _departmentFilter == null || IsCardInDepartment(provider, card, _departmentFilter);

                    if (textMatch && departmentMatch)
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

        private void FillSecurityLevelComboBox()
        {
            var selectedSecurityLevel = GetSelectedSecurityLevel();

            _eSecurityLevel.Items.Clear();

            foreach (CarSecurityLevel securityLevel in Enum.GetValues(typeof(CarSecurityLevel)))
            {
                _eSecurityLevel.Items.Add(new CarSecurityLevelItem(securityLevel, localizationKey => GetString(localizationKey)));
            }

            SelectSecurityLevel(selectedSecurityLevel);
        }

        private void SelectSecurityLevel(CarSecurityLevel securityLevel)
        {
            foreach (var item in _eSecurityLevel.Items.OfType<CarSecurityLevelItem>())
            {
                if (item.SecurityLevel != securityLevel)
                    continue;

                _eSecurityLevel.SelectedItem = item;
                return;
            }

            if (_eSecurityLevel.Items.Count > 0)
                _eSecurityLevel.SelectedIndex = 0;
        }

        private CarSecurityLevel GetSelectedSecurityLevel()
        {
            var selectedItem = _eSecurityLevel.SelectedItem as CarSecurityLevelItem;
            if (selectedItem != null)
                return selectedItem.SecurityLevel;

            return CarSecurityLevel.StandardLprAndCard;
        }

        private class CarSecurityLevelItem
        {
            private readonly Func<string, string> _getString;

            public CarSecurityLevel SecurityLevel { get; private set; }

            public CarSecurityLevelItem(CarSecurityLevel securityLevel, Func<string, string> getString)
            {
                SecurityLevel = securityLevel;
                _getString = getString;
            }

            public override string ToString()
            {
                var localizationKey = $"CarEditForm_SecurityLevel_{SecurityLevel}";
                var localizedText = _getString(localizationKey);

                return string.IsNullOrWhiteSpace(localizedText)
                    ? SecurityLevel.ToString()
                    : localizedText;
            }
        }

        private static bool IsCardInDepartment(ICgpServerRemotingProvider provider, Card card, UserFoldersStructure departmentFilter)
        {
            if (provider == null || card == null || departmentFilter == null || card.GuidPerson == Guid.Empty)
                return false;

            var person = provider.Persons.GetObjectById(card.GuidPerson);
            return string.Equals(person?.Department?.FolderName, departmentFilter.FolderName, StringComparison.CurrentCultureIgnoreCase);
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
                if (!cardLookup.TryGetValue(modifyObject.GetId, out var card))
                    continue;

                if (card.GuidPerson == Guid.Empty)
                    continue;

                string displayName = BuildCardDescription(card, provider);

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

        private void _bApply_Click(object sender, EventArgs e)
        {
            if (Apply_Click())
            {
                SetValuesEdit();
                ResetWasChangedValues();
                _bApply.Enabled = false;
            }
        }

        protected override void EditTextChanger(object sender, EventArgs e)
        {
            base.EditTextChanger(sender, e);
            _bApply.Enabled = true;
        }

        private void SetDepartmentFullName(UserFoldersStructure department)
        {
            if (department == null)
                return;

            department.SetFullFolderName(
                CgpClient.Singleton.MainServerProvider.UserFoldersSutructures.GetFullDepartmentName(
                    department.GetIdString(),
                    department.FolderName,
                    @"\\",
                    CgpClient.Singleton.LocalizationHelper.GetString("SelectStructuredSubSiteForm_RootNode")));
        }

        private void SetActDepartment(UserFoldersStructure department)
        {
            _actDepartment = department;
            if (_actDepartment == null)
            {
                _tbmDepartment.Text = string.Empty;
                _tbmDepartment.TextImage = null;
            }
            else
            {
                _tbmDepartment.Text = _actDepartment.ToString();
                _tbmDepartment.TextImage = ObjectImageList.Singleton.GetImageForAOrmObject(_actDepartment);
            }
        }

        private void ModifyActDepartment()
        {
            if (CgpClient.Singleton.IsConnectionLost(true))
                return;

            try
            {
                var carId = Insert ? null : (object)_editingObject.IdCar;

                var listDepartments = CgpClient.Singleton.MainServerProvider.UserFoldersSutructures.ListDepartments(
                    CgpClient.Singleton.LocalizationHelper.GetString("SelectStructuredSubSiteForm_RootNode"),
                    @"\",
                    carId,
                    out var error);

                if (error != null)
                    throw error;

                var formAdd = new ListboxFormAdd(
                    listDepartments,
                    CgpClient.Singleton.LocalizationHelper.GetString(
                        "UserFoldersStructuresFormUserFoldersStructuresForm"));

                object outUserFolder;
                formAdd.ShowDialog(out outUserFolder);
                if (outUserFolder is UserFoldersStructure userFolder)
                {
                    SetDepartmentFullName(userFolder);
                    SetActDepartment(userFolder);
                    CgpClientMainForm.Singleton.AddToRecentList(_actDepartment);
                    EditTextChanger(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                HandledExceptionAdapter.Examine(ex);
            }
        }

        private void DepartmentButtonPopupMenuItemClick(ToolStripItem item, int index)
        {
            if (item.Name == "_tsiDepartmentModify")
            {
                ModifyActDepartment();
            }
            else if (item.Name == "_tsiDepartmentRemove")
            {
                SetActDepartment(null);
                EditTextChanger(this, EventArgs.Empty);
            }
        }

        private void DepartmentTextBoxDoubleClick(object sender, EventArgs e)
        {
            if (_actDepartment == null)
                return;

            try
            {
                DbsSupport.OpenEditForm(_actDepartment);
            }
            catch (Exception error)
            {
                HandledExceptionAdapter.Examine(error);
            }
        }

        private void _bDepartmentFilter_Click(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(true)) return;

            try
            {
                Exception error;
                var listDepartments = CgpClient.Singleton.MainServerProvider.UserFoldersSutructures.ListDepartments(
                    CgpClient.Singleton.LocalizationHelper.GetString("SelectStructuredSubSiteForm_RootNode"),
                    @"\", null, out error);

                if (error != null) throw error;

                var formAdd = new ListboxFormAdd(
                    listDepartments,
                    CgpClient.Singleton.LocalizationHelper.GetString(
                        "UserFoldersStructuresFormUserFoldersStructuresForm"));

                object outUserFolder;
                formAdd.ShowDialog(out outUserFolder);
                if (outUserFolder != null)
                {
                    _departmentFilter = outUserFolder as UserFoldersStructure;
                    _tbDepartmentFilter.Text = _departmentFilter?.FolderName ?? string.Empty;
                    LoadCards(_eFilterCards.Text);
                }
            }
            catch (Exception ex)
            {
                HandledExceptionAdapter.Examine(ex);
            }
        }

        private void _bDepartmentFilterClear_Click(object sender, EventArgs e)
        {
            _departmentFilter = null;
            _tbDepartmentFilter.Text = string.Empty;
            LoadCards(_eFilterCards.Text);
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
