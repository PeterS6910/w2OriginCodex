using Contal.Cgp.Client;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.Server.Beans;
using System;
using System.Linq;
using System.Windows.Forms;

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
            SetAccessTypeOptions(accessType);

            ApplyLocalization();
        }

        public CarDoorEnvironmentAccessType SelectedAccessType =>
              _cbAccessType.SelectedValue is CarDoorEnvironmentAccessType accessType
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

            _bOk.Text = GetString("General_bUpdate");
            _bCancel.Text = GetString("General_bCancel");

            SetAccessTypeOptions(SelectedAccessType);
        }

        private void SetAccessTypeOptions(CarDoorEnvironmentAccessType selectedAccessType)
        {
            _cbAccessType.DisplayMember = nameof(AccessTypeItem.Text);
            _cbAccessType.ValueMember = nameof(AccessTypeItem.Value);

            _cbAccessType.DataSource = Enum.GetValues(typeof(CarDoorEnvironmentAccessType))
                .Cast<CarDoorEnvironmentAccessType>()
                .Select(value => new AccessTypeItem
                {
                    Text = GetString($"CarDoorEnvironmentAccessType_{value}") ?? value.ToString(),
                    Value = value
                })
                .ToList();

            _cbAccessType.SelectedValue = selectedAccessType;
        }

        private class AccessTypeItem
        {
            public string Text { get; set; }

            public CarDoorEnvironmentAccessType Value { get; set; }
        }
    }
}
