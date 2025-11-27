using System;
using Contal.Cgp.Globals;
using Contal.Cgp.Server.Beans;

namespace Contal.Cgp.NCAS.Server.Beans
{
    public enum CarDoorEnvironmentAccessType : byte
    {
        None = 0,
        Free = 1,
        CardRequired = 2,
        AccessDenied = 3
    }

    [Serializable]
    public class CarDoorEnvironment : AOrmObject
    {
        public const string COLUMN_ID_CAR_DOOR_ENVIRONMENT = "IdCarDoorEnvironment";
        public const string COLUMN_ID_DOOR_ENVIRONMENT = "IdDoorEnvironment";
        public const string COLUMN_ID_CAR = "IdCar";
        public const string COLUMN_ACCESS_TYPE = "AccessType";

        public virtual Guid IdCarDoorEnvironment { get; set; }
        public virtual DoorEnvironment DoorEnvironment { get; set; }
        public virtual Car Car { get; set; }
        public virtual CarDoorEnvironmentAccessType AccessType { get; set; }

        public override bool Compare(object obj)
        {
            var other = obj as CarDoorEnvironment;
            return other != null && other.IdCarDoorEnvironment.Equals(IdCarDoorEnvironment);
        }

        public override string GetIdString()
        {
            return IdCarDoorEnvironment.ToString();
        }

        public override ObjectType GetObjectType()
        {
            return ObjectType.NotSupport;
        }

        public override object GetId()
        {
            return IdCarDoorEnvironment;
        }

        public override IModifyObject CreateModifyObject()
        {
            return null;
        }
    }

    [Serializable]
    public class CarDoorEnvironmentShort : IShortObject
    {
        public const string COLUMN_ID_CAR_DOOR_ENVIRONMENT = "IdCarDoorEnvironment";
        public const string COLUMN_NAME = "Name";
        public const string COLUMN_ACCESS_TYPE = "AccessType";

        public Guid IdCarDoorEnvironment { get; set; }
        public string Name { get; set; }
        public CarDoorEnvironmentAccessType AccessType { get; set; }

        public CarDoorEnvironmentShort(CarDoorEnvironment carDoorEnvironment)
        {
            if (carDoorEnvironment == null)
                throw new ArgumentNullException(nameof(carDoorEnvironment));

            IdCarDoorEnvironment = carDoorEnvironment.IdCarDoorEnvironment;
            Name = carDoorEnvironment.DoorEnvironment?.Name;
            AccessType = carDoorEnvironment.AccessType;
        }

        public override string ToString()
        {
            return Name;
        }

        ObjectType IShortObject.ObjectType => ObjectType.NotSupport;

        string IShortObject.GetSubTypeImageString(object value)
        {
            return string.Empty;
        }

        string IShortObject.Name => Name;

        object IShortObject.Id => IdCarDoorEnvironment;
    }
}
