using System;
using System.Windows.Forms;
using Contal.Cgp.Client;
using Contal.Cgp.NCAS.Server.Beans;

namespace Contal.Cgp.NCAS.Client
{
    public partial class NCASCarDoorEnvironmentEditForm : CgpTranslateForm
    {
        public NCASCarDoorEnvironmentEditForm(
            string doorEnvironmentName,
            string carName,
            CarDoorEnvironmentAccessType accessType)
            : base(NCASClient.LocalizationHelper)
        {
            InitializeComponent();

            _tbDoorEnvironment.Text = doorEnvironmentName;
            _tbCar.Text = carName;

            _cbAccessType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbAccessType.DataSource = Enum.GetValues(typeof(CarDoorEnvironmentAccessType));
            _cbAccessType.SelectedItem = accessType;

            ApplyLocalization();
        }

        public CarDoorEnvironmentAccessType SelectedAccessType =>
            _cbAccessType.SelectedItem is CarDoorEnvironmentAccessType accessType
                ? accessType
                : CarDoorEnvironmentAccessType.None;

        protected override void AfterTranslateForm()
        {
            base.AfterTranslateForm();
            ApplyLocalization();
        }

        private void ApplyLocalization()
        {
            Text = GetString("NCASCarDoorEnvironmentEditFormNCASCarDoorEnvironmentEditForm");
            _lDoorEnvironment.Text = GetString("NCASCarDoorEnvironmentEditForm_lDoorEnvironment");
            _lCar.Text = GetString("NCASCarDoorEnvironmentEditForm_lCar");
            _lAccessType.Text = GetString("NCASCarDoorEnvironmentEditForm_lAccessType");

            _bOk.Text = GetString("General_bOk");
            _bCancel.Text = GetString("General_bCancel");
        }
    }
}
