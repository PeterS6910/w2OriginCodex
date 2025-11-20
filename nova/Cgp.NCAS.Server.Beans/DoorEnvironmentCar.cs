using System;
using Contal.Cgp.Globals;
using Contal.Cgp.Server.Beans;

namespace Contal.Cgp.NCAS.Server.Beans
{
    public enum DoorEnvironmentCarAccessType : byte
    {
        None = 0,
        Free = 1,
        CardRequired = 2,
        AccessDenied = 3
    }

    [Serializable]
    public class DoorEnvironmentCar : AOrmObject
    {
        public const string COLUMN_ID_DOOR_ENVIRONMENT_CAR = "IdDoorEnvironmentCar";
        public const string COLUMN_DOOR_ENVIRONMENT = "IdDoorEnvironment";
        public const string COLUMN_CAR = "IdCar";
        public const string COLUMN_ACCESS_TYPE = "AccessType";

        public virtual Guid IdDoorEnvironmentCar { get; set; }
        public virtual DoorEnvironment DoorEnvironment { get; set; }
        public virtual Car Car { get; set; }
        public virtual DoorEnvironmentCarAccessType AccessType { get; set; }

        public override bool Compare(object obj)
        {
            var other = obj as DoorEnvironmentCar;
            return other != null && other.IdDoorEnvironmentCar.Equals(IdDoorEnvironmentCar);
        }

        public override string GetIdString()
        {
            return IdDoorEnvironmentCar.ToString();
        }

        public override ObjectType GetObjectType()
        {
            return ObjectType.NotSupport;
        }

        public override object GetId()
        {
            return IdDoorEnvironmentCar;
        }

        public override IModifyObject CreateModifyObject()
        {
            return null;
        }
    }
}
