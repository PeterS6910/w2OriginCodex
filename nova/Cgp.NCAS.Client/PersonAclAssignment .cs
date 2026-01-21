using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Contal.Cgp.Client;
using Contal.Cgp.Client.PluginSupport;
using Contal.Cgp.Globals;
using Contal.Cgp.Server.Beans;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.BaseLib;
using Contal.IwQuick;
using Contal.IwQuick.Threads;
using Contal.IwQuick.UI;

namespace Contal.Cgp.NCAS.Client
{
        public partial class PersonAclAssignment :
#if DESIGNER    
        Form
#else
        PluginMainForm<NCASClient>
#endif
    {
        private ListOfObjects _actAccessControlLists;
        private List<Object> _persons;
        private List<Object> _cars;
        private List<Guid> _acls;
        private bool _assigningCars;
        private DVoid2Void _dAfterTranslateForm;

        public override NCASClient Plugin
        {
            get { return NCASClient.Singleton; }
        }

        public PersonAclAssignment()
            : base(NCASClient.LocalizationHelper, CgpClientMainForm.Singleton)
        {
            InitializeComponent();
            _tbdpDateFrom.LocalizationHelper = LocalizationHelper;
            _tbdpDateTo.LocalizationHelper = LocalizationHelper;
            Enter += RunOnEnter;
            Disposed += RunOnDisposed;
            LocalizationHelper.TranslateForm(this);
            _tbdpDateFrom.Value = DateTime.Now.Date;
        }

        void RunOnEnter(object sender, EventArgs e)
        {
            if (CgpClient.Singleton.IsConnectionLost(false)) return;
            if (_dAfterTranslateForm == null)
            {
                _dAfterTranslateForm = AfterTranslateForm;
                LocalizationHelper.LanguageChanged += _dAfterTranslateForm;
            }
        }

        void RunOnDisposed(object sender, EventArgs e)
        {
            LocalizationHelper.LanguageChanged -= _dAfterTranslateForm;
        }

        public void ModifyAccesControlList()
        {
            if (CgpClient.Singleton.IsConnectionLost(true)) return;

            try
            {
                List<AOrmObject> listAccessControlList = new List<AOrmObject>();

                Exception error;
                ICollection<AccessControlList> listAccessControlListFromDatabase = Plugin.MainServerProvider.AccessControlLists.List(out error);
                foreach (AccessControlList accessControlList in listAccessControlListFromDatabase)
                {
                    listAccessControlList.Add(accessControlList);
                }

                ListboxFormAdd formAdd = new ListboxFormAdd(listAccessControlList, GetString("NCASAccessControlListsFormNCASAccessControlListsForm"));
                ListOfObjects outAccessControlLists;
                formAdd.ShowDialogMultiSelect(out outAccessControlLists);
                if (outAccessControlLists != null)
                {
                    _actAccessControlLists = outAccessControlLists;
                    DoAssingAcl();
                    ShowDialog();
                }
                else
                {
                    Close();
                }
            }
            catch
            {
            }
        }

        private void DoAssingAcl()
        {
            _acls = new List<Guid>();
            foreach (Object obj in _actAccessControlLists)
            {
                _acls.Add((obj as AccessControlList).IdAccessControlList);
            }
        }

        public override void SpecialAction(List<Object> persons)
        {
            if (persons == null || persons.Count == 0)
            {
                Close();
            }
            else
            {
                SetAssignmentTargets(persons);
                ModifyAccesControlList();
            }
        }

        private void _bCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch {}
        }

        private void _bContinue_Click(object sender, EventArgs e)
        {
            if (!ControlValues()) 
                return;

            _bContinue.Enabled = false;
            _bCancel.Enabled = false;
            _tbdpDateFrom.Enabled = false;
            _tbdpDateTo.Enabled = false;
            Cursor = Cursors.WaitCursor;

            SafeThread.StartThread(() =>
            {
                var errors = _assigningCars
                    ? Plugin.MainServerProvider.ACLCars.CarAclAssignment(
                        _cars,
                        _acls,
                        _tbdpDateFrom.Value,
                        _tbdpDateTo.Value)
                    : Plugin.MainServerProvider.ACLPersons.PersonAclAssignment(
                        _persons,
                        _acls,
                        _tbdpDateFrom.Value,
                        _tbdpDateTo.Value);

                BeginInvoke(new Action(() =>
                {
                    Dialog.Info(errors == null
                        ? GetString(GetAssignmentOkKey())
                        : GetString(GetAssignmentErrorsKey()));

                    Close();
                }));
            });
        }

        private bool ControlValues()
        {
            if ((_tbdpDateTo.Value != null && _tbdpDateFrom.Value != null) && _tbdpDateFrom.Value > _tbdpDateTo.Value)
            {
                ControlNotification.Singleton.Error(NotificationPriority.JustOne,
                   _tbdpDateTo.TextBox, GetString("ErrorACLDateRange"), ControlNotificationSettings.Default);
                return false;
            }

            return true;
        }

        private void SetAssignmentTargets(List<Object> objects)
        {
            _persons = null;
            _cars = null;
            _assigningCars = false;

            if (objects == null || objects.Count == 0)
                return;

            var typedObjects = objects.OfType<IdAndObjectType>().ToList();
            if (typedObjects.Count > 0)
            {
                if (typedObjects.All(item => item.ObjectType == ObjectType.Car))
                {
                    _assigningCars = true;
                    _cars = typedObjects.Select(item => item.Id).ToList();
                }
                else
                {
                    _persons = typedObjects.Select(item => item.Id).ToList();
                }
            }
            else
            {
                _persons = objects;
            }

            Text = GetString(_assigningCars
                ? "CarAclAssignmentCarAclAssignment"
                : "PersonAclAssignmentPersonAclAssignment");
        }

        private string GetAssignmentOkKey()
        {
            return _assigningCars
                ? "CarAclAssignmentOk"
                : "PersonAclAssignmentOk";
        }

        private string GetAssignmentErrorsKey()
        {
            return _assigningCars
                ? "CarAclAssignmentErrors"
                : "PersonAclAssignmentErrors";
        }
    }
}
