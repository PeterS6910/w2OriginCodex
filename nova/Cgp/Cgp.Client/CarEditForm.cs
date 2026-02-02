using Cgp.Components;
using Contal.Cgp.BaseLib;
using Contal.Cgp.Components;
using Contal.Cgp.Globals;
using Contal.Cgp.NCAS.RemotingCommon;
using Contal.Cgp.NCAS.Server;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.RemotingCommon;
using Contal.Cgp.Server.Beans;
using Contal.IwQuick;
using Contal.IwQuick.Sys;
using Contal.IwQuick.Threads;
using Contal.IwQuick.UI;
using Contal.IwQuick.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using SqlDeleteReferenceConstraintException = Contal.IwQuick.SqlDeleteReferenceConstraintException;
using SqlUniqueException = Contal.IwQuick.SqlUniqueException;

namespace Contal.Cgp.Client
{
    public partial class CarEditForm :
#if DESIGNER
        Form
#else
                ACgpEditForm<Car>, ICgpDataGridView
#endif
    {
        private BindingSource _aclCarsBindingSource;
        private ListOfObjects _actAccessControlLists;
        private ACLCar _editingAclCar;
        private bool _isLoadingAclCars;
        public CarEditForm(Car car, ShowOptionsEditForm showOption)
            : base(car, showOption)
        {
            InitializeComponent();
            _tbdpAclDateFrom.LocalizationHelper = LocalizationHelper;
            _tbdpAclDateTo.LocalizationHelper = LocalizationHelper;
            _editingObject = car;
            _ilbCards.ImageList = ObjectImageList.Singleton.ClientObjectImages;
            _cdgvAclCars.LocalizationHelper = LocalizationHelper;
            _cdgvAclCars.CgpDataGridEvents = this;
            _cdgvAclCars.ImageList = ObjectImageList.Singleton.GetAllObjectImages();
            _cdgvAclCars.EnabledInsertButton = false;
            _cdgvAclCars.DataGrid.AllowUserToAddRows = false;
            _cdgvAclCars.DataGrid.AllowUserToDeleteRows = false;
            _cdgvAclCars.DataGrid.AllowUserToResizeRows = false;
            _cdgvAclCars.DataGrid.MultiSelect = true;
            _cdgvAclCars.DataGrid.ReadOnly = true;
            _cdgvAclCars.DataGrid.RowHeadersVisible = false;
            _cdgvAclCars.DataGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            SetReferenceEditColors();
            _tcCar.SelectedIndexChanged += _tcCar_SelectedIndexChanged;
            _tcCar_SelectedIndexChanged(null, null);
            ResetAclValues();
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
            ResetAclValues();
        }

        protected override void SetValuesEdit()
        {
            _eLp.Text = _editingObject.Lp;
            _eBrand.Text = _editingObject.Brand;
            _dpValidityDateFrom.Value = _editingObject.ValidityDateFrom;
            _dpValidityDateTo.Value = _editingObject.ValidityDateTo;
            _eDescription.Text = _editingObject.Description;
            LoadCards(_eFilterCards.Text);
            ResetAclValues();
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
            if (_tcCar.SelectedTab == _tpAccessControlList)
                LoadAclCars();
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

        private void _tbmAccessControlList_DoubleClick(object sender, EventArgs e)
        {
            ModifyAccessControlList();
        }

        private void _tbmAccessControlList_ButtonPopupMenuItemClick(ToolStripItem item, int index)
        {
            if (item.Name == "_tsiAclModify")
            {
                ModifyAccessControlList();
            }
            else if (item.Name == "_tsiAclCreate")
            {
                CreateAccessControlList();
            }
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
            if (_isLoadingAclCars)
                return;

            _isLoadingAclCars = true;
            try
            {
                if (_editingObject.IdCar == Guid.Empty)
                {
                    UpdateAclCarsGrid(new List<ACLCar>());
                    return;
                }

                var ncasProvider = GetNcasProvider();
                if (ncasProvider == null)
                {
                    UpdateAclCarsGrid(new List<ACLCar>());
                    return;
                }

                Exception error;
                IList<ACLCar> aclCars = null;
                try
                {
                    aclCars = ncasProvider.ACLCars.GetAclCarsByCar(_editingObject.IdCar, out error);
                }
                catch (MissingMethodException)
                {
                    Dialog.Error(CgpClient.Singleton.LocalizationHelper.GetString("ErrorLoadTable"));
                    UpdateAclCarsGrid(new List<ACLCar>());
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

                    UpdateAclCarsGrid(new List<ACLCar>());
                    return;
                }
                UpdateAclCarsGrid(aclCars ?? new List<ACLCar>());
            }
            finally
            {
                _isLoadingAclCars = false;
            }
        }

        private void ClearAclCarsGrid()
        {
            UpdateAclCarsGrid(new List<ACLCar>());
        }

        private void UpdateAclCarsGrid(IList<ACLCar> aclCars)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<IList<ACLCar>>(UpdateAclCarsGrid), aclCars);
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

        private void ModifyAccessControlList()
        {
            if (CgpClient.Singleton.IsConnectionLost(true))
                return;

            var ncasProvider = GetNcasProvider();
            if (ncasProvider == null)
                return;

            try
            {
                var listAccessControlList = new List<AOrmObject>();
                Exception error;
                var listAccessControlListFromDatabase = ncasProvider.AccessControlLists.List(out error);
                if (error != null)
                {
                    Dialog.Error(error.Message);
                    return;
                }

                foreach (var accessControlList in listAccessControlListFromDatabase)
                {
                    listAccessControlList.Add(accessControlList);
                }

                var formAdd = new ListboxFormAdd(listAccessControlList, GetString("AccessGroup_AccessControlLists"));
                if (_bAclCreate.Visible)
                {
                    ListOfObjects outAccessControlLists;
                    formAdd.ShowDialogMultiSelect(out outAccessControlLists);
                    if (outAccessControlLists != null)
                    {
                        _actAccessControlLists = outAccessControlLists;
                        RefreshAccessControlList();
                    }
                }
                else
                {
                    object outAccessControlList;
                    formAdd.ShowDialog(out outAccessControlList);
                    if (outAccessControlList is AccessControlList)
                    {
                        var outAccessControlLists = new ListOfObjects();
                        outAccessControlLists.Objects.Add(outAccessControlList);
                        _actAccessControlLists = outAccessControlLists;
                        RefreshAccessControlList();
                    }
                }
            }
            catch
            {
            }
        }

        private void CreateAccessControlList()
        {
            if (CgpClient.Singleton.IsConnectionLost(true))
                return;

            try
            {
                var formType = Type.GetType("Contal.Cgp.NCAS.Client.NCASAccessControlListsForm, Cgp.NCAS.Client");
                if (formType == null)
                {
                    Dialog.Error(GetString("ErrorInsertFailed"));
                    return;
                }

                var singletonProperty = formType.GetProperty("Singleton", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var formInstance = singletonProperty?.GetValue(null, null);
                if (formInstance == null)
                {
                    Dialog.Error(GetString("ErrorInsertFailed"));
                    return;
                }

                var accessControlList = new AccessControlList();
                var openMethod = formType.GetMethod("OpenInsertFromEdit", new[]
                {
                    typeof(AccessControlList).MakeByRefType(),
                    typeof(Action<object>)
                });

                if (openMethod == null)
                {
                    Dialog.Error(GetString("ErrorInsertFailed"));
                    return;
                }

                openMethod.Invoke(formInstance, new object[] { accessControlList, new Action<object>(DoAfterAccessControlListCreated) });
            }
            catch (Exception ex)
            {
                HandledExceptionAdapter.Examine(ex);
            }
        }

        private void DoAfterAccessControlListCreated(object newAccessControlList)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object>(DoAfterAccessControlListCreated), newAccessControlList);
            }
            else if (newAccessControlList is AccessControlList)
            {
                _actAccessControlLists = new ListOfObjects();
                _actAccessControlLists.Objects.Add(newAccessControlList as AccessControlList);
                RefreshAccessControlList();
            }
        }

        private void RefreshAccessControlList()
        {
            if (_actAccessControlLists != null && _actAccessControlLists.Count > 0)
            {
                _tbmAccessControlList.Text = _actAccessControlLists.ToString();
                _tbmAccessControlList.TextImage = GetAccessControlListImage();
            }
            else
            {
                _tbmAccessControlList.Text = string.Empty;
                _tbmAccessControlList.TextImage = null;
            }
        }

        private Image GetAccessControlListImage()
        {
            var images = ObjectImageList.Singleton.GetAllObjectImages();
            var key = ObjectType.AccessControlList.ToString();
            return images.Images.ContainsKey(key) ? images.Images[key] : null;
        }

        private bool ControlAclValues()
        {
            if (_actAccessControlLists == null || _actAccessControlLists.Count == 0)
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne, _tbmAccessControlList.ImageTextBox,
                    GetString("ErrorEntryAccessControlList"), ControlNotificationSettings.Default);
                return false;
            }

            if ((_tbdpAclDateTo.Value != null && _tbdpAclDateFrom.Value != null) && _tbdpAclDateFrom.Value > _tbdpAclDateTo.Value)
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne,
                    _tbdpAclDateTo.TextBox, GetString("ErrorACLDateRange"), ControlNotificationSettings.Default);
                return false;
            }

            return true;
        }

        private void _bAclCreate_Click(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false))
                return;

            if (_editingObject.IdCar == Guid.Empty)
            {
                Ok_Click(false);
            }

            if (_editingObject.IdCar == Guid.Empty)
                return;

            if (!ControlAclValues())
                return;

            if (!GetAclValues(out var aclCars))
                return;

            if (!Dialog.Question(GetString("QuestionInsertACLCar")))
                return;

            CreateAclCars(aclCars);
        }

        private void _bAclUpdate_Click(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false))
                return;

            if (_editingAclCar == null)
                return;

            if (!GetAclValues(_editingAclCar))
                return;

            if (!Dialog.Question(GetString("QuestionUpdateACLCar")))
                return;

            UpdateAclCar();
        }

        private void _bAclCancel_Click(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false))
                return;

            _editingAclCar = null;
            _bAclUpdate.Visible = false;
            _bAclCancel.Visible = false;
            _bAclCreate.Visible = true;
            ResetAclValues();
        }

        private void CreateAclCars(IList<ACLCar> aclCars)
        {
            SafeThread<IList<ACLCar>>.StartThread(DoCreateAclCars, aclCars);
        }

        private void DoCreateAclCars(IList<ACLCar> aclCars)
        {
            var ncasProvider = GetNcasProvider();
            if (ncasProvider == null)
                return;

            if (aclCars != null && aclCars.Count > 0)
            {
                foreach (var aclCar in aclCars)
                {
                    Exception error;
                    var insertAclCar = aclCar;
                    ncasProvider.ACLCars.Insert(ref insertAclCar, out error);
                    if (error != null)
                    {
                        if (error is AccessDeniedException)
                        {
                            Dialog.Error(GetString("ErrorInsertAccessDenied"));
                        }
                        else if (error is SqlUniqueException)
                        {
                            Dialog.Error(GetString("ErrorInsertFailed"));
                        }
                        else
                        {
                            Dialog.Error(GetString("ErrorInsertFailed"));
                        }

                        LoadAclCars();
                        return;
                    }
                }

                ResetAclValues();
                LoadAclCars();
            }
        }

        private void UpdateAclCar()
        {
            SafeThread.StartThread(DoUpdateAclCar);
        }

        private void DoUpdateAclCar()
        {
            var ncasProvider = GetNcasProvider();
            if (ncasProvider == null)
                return;

            Exception error;
            ncasProvider.ACLCars.Update(_editingAclCar, out error);
            if (error != null)
            {
                if (error is AccessDeniedException)
                {
                    Dialog.Error(GetString("ErrorEditAccessDenied"));
                }
                else if (error is IncoherentDataException)
                {
                    if (Dialog.WarningQuestion(GetString("QuestionLoadActualData")))
                    {
                        _editingAclCar = ncasProvider.ACLCars.GetObjectForEdit(_editingAclCar.IdACLCar, out error);
                        SetAclValues();
                    }
                    else
                    {
                        _editingAclCar = ncasProvider.ACLCars.GetObjectForEdit(_editingAclCar.IdACLCar, out error);
                    }
                }
                else
                {
                    Dialog.Error(GetString("ErrorEditFailed") + ": " + error.Message);
                }

                return;
            }

            ncasProvider.ACLCars.EditEnd(_editingAclCar);
            _bAclCancel_Click(null, null);
            ResetAclValues();
            LoadAclCars();
        }

        private void DeleteAclCars(ICollection<ACLCar> aclCars)
        {
            SafeThread<ACLCar>.StartThread(DoDeleteAclCars, aclCars);
        }

        private void DoDeleteAclCars(ICollection<ACLCar> aclCars)
        {
            var ncasProvider = GetNcasProvider();
            if (ncasProvider == null)
                return;

            if (aclCars != null)
            {
                foreach (var aclCar in aclCars)
                {
                    if (_editingAclCar != null && aclCar.Compare(_editingAclCar))
                    {
                        Dialog.Error(GetString("ErrorDeleteEditing"));
                        continue;
                    }

                    Exception error;
                    ncasProvider.ACLCars.Delete(aclCar, out error);
                    if (error != null)
                    {
                        if (error is SqlDeleteReferenceConstraintException)
                            Dialog.Error(GetString("ErrorDeleteRowInRelationship"));
                        else
                            Dialog.Error(GetString("ErrorDeleteFailed"));
                    }
                    else
                    {
                        LoadAclCars();
                    }
                }
            }
        }

        private void SetAclValues()
        {
            if (_editingAclCar == null)
                return;

            _actAccessControlLists = new ListOfObjects();
            _actAccessControlLists.Objects.Add(_editingAclCar.AccessControlList);

            RefreshAccessControlList();
            _tbdpAclDateFrom.Value = _editingAclCar.DateFrom;
            _tbdpAclDateTo.Value = _editingAclCar.DateTo;
        }

        private void ResetAclValues()
        {
            _actAccessControlLists = null;
            RefreshAccessControlList();
            _tbdpAclDateFrom.Value = DateTime.Now.Date;
            _tbdpAclDateTo.Value = null;
            _bAclUpdate.Visible = false;
            _bAclCancel.Visible = false;
            _bAclCreate.Visible = true;
        }

        private bool GetAclValues(ACLCar aclCar)
        {
            if (!ControlAclValues())
                return false;

            if (_actAccessControlLists.Objects.Count == 0)
                return false;

            try
            {
                GetAclValues(aclCar, _actAccessControlLists.Objects[0] as AccessControlList);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool GetAclValues(out IList<ACLCar> aclCars)
        {
            aclCars = null;

            if (!ControlAclValues())
                return false;

            try
            {
                aclCars = new List<ACLCar>();
                foreach (var obj in _actAccessControlLists.Objects)
                {
                    var aclCar = new ACLCar();
                    GetAclValues(aclCar, obj as AccessControlList);
                    aclCars.Add(aclCar);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void GetAclValues(ACLCar aclCar, AccessControlList accessControlList)
        {
            aclCar.AccessControlList = accessControlList;
            aclCar.Car = _editingObject;
            aclCar.DateFrom = GetDateFromPicker(_tbdpAclDateFrom);
            aclCar.DateTo = GetDateFromPicker(_tbdpAclDateTo);
        }

        private static DateTime? GetDateFromPicker(TextBoxDatePicker picker)
        {
            if (picker.Text != string.Empty)
            {
                DateTime dateTime;
                if (DateTime.TryParse(picker.Text, out dateTime))
                {
                    return dateTime;
                }
            }

            return null;
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

        public void EditClick()
        {
            if (CgpClient.Singleton.IsConnectionLost(false))
                return;

            if (_aclCarsBindingSource == null || _aclCarsBindingSource.Count == 0)
                return;

            var ncasProvider = GetNcasProvider();
            if (ncasProvider == null)
                return;

            _editingAclCar = _aclCarsBindingSource[_aclCarsBindingSource.Position] as ACLCar;
            if (_editingAclCar == null)
                return;

            Exception error;
            _editingAclCar = ncasProvider.ACLCars.GetObjectForEdit(_editingAclCar.IdACLCar, out error);
            if (_editingAclCar == null)
                return;

            SetAclValues();
            _bAclCreate.Visible = false;
            _bAclUpdate.Visible = true;
            _bAclCancel.Visible = true;
        }

        public void EditClick(ICollection<int> indexes)
        {
        }

        public void DeleteClick()
        {
            if (CgpClient.Singleton.IsConnectionLost(false))
                return;

            if (_aclCarsBindingSource == null || _aclCarsBindingSource.Count == 0)
                return;

            var aclCars = new LinkedList<ACLCar>();
            foreach (var selectedRow in _cdgvAclCars.DataGrid.SelectedRows)
            {
                aclCars.AddLast(_aclCarsBindingSource[((DataGridViewRow)selectedRow).Index] as ACLCar);
            }

            DeleteAclCars(aclCars);
        }

        public void DeleteClick(ICollection<int> indexes)
        {
            if (CgpClient.Singleton.IsConnectionLost(false))
                return;

            if (_aclCarsBindingSource == null || _aclCarsBindingSource.Count == 0)
                return;

            var items =
                indexes.Select(index => (IShortObject)new AccessControlListShort(((ACLCar)_aclCarsBindingSource.List[index]).AccessControlList))
                    .ToList();

            var dialog = new DeleteDataGridItemsDialog(
                ObjectImageList.Singleton.GetAllObjectImages(),
                items,
                CgpClient.Singleton.LocalizationHelper)
            {
                SelectItem = items.FirstOrDefault(item => item.Id.Equals((((ACLCar)_aclCarsBindingSource.Current).AccessControlList.IdAccessControlList)))
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var aclCars = new LinkedList<ACLCar>();

                if (dialog.DeleteAll)
                {
                    foreach (var index in indexes)
                    {
                        if (_aclCarsBindingSource.Count > index)
                            aclCars.AddLast(_aclCarsBindingSource[index] as ACLCar);
                    }
                }
                else
                {
                    foreach (var item in dialog.SelectedItems)
                    {
                        var aclCar =
                            _aclCarsBindingSource.List.Cast<ACLCar>()
                                .FirstOrDefault(car => car.AccessControlList.IdAccessControlList.Equals(item.Id));

                        aclCars.AddLast(aclCar);
                    }
                }

                DeleteAclCars(aclCars);
            }
        }

        public void InsertClick()
        {
        }

        private ICgpNCASRemotingProvider GetNcasProvider()
        {
            var plugin = CgpClient.Singleton.PluginManager.GetLoadedPlugin("NCAS plugin");
            if (plugin != null)
            {
                var providerProperty = plugin.GetType().GetProperty(
                    "MainServerProvider",
                    BindingFlags.Public | BindingFlags.Instance);

                var provider = providerProperty?.GetValue(plugin, null) as ICgpNCASRemotingProvider;
                if (provider != null)
                    return provider;
            }

            return CgpClient.Singleton.MainServerProvider as ICgpNCASRemotingProvider;
        }
    }
}
