using System;
using Contal.Cgp.Globals;
using Contal.Cgp.Server.Beans;
using Contal.IwQuick.Data;
using TimeZone = Contal.Cgp.Server.Beans.TimeZone;

namespace Contal.Cgp.NCAS.Server.Beans
{
    [Serializable()]
    [LwSerialize(845)]
    [LwSerializeMode(LwSerializationMode.Selective, DirectMemberType.All)]
    [LwSerializeNoParent]
    public class AccessZoneCar : AOrmObjectWithVersion
    {
        public const string COLUMNIDACCESSZONECAR = "IdAccessZoneCar";
        public const string COLUMNCAR = "Car";
        public const string COLUMNGUIDCAR = "GuidCar";
        public const string COLUMNLPRCAMERA = "LprCamera";
        public const string COLUMNGUIDLPRCAMERA = "GuidLprCamera";
        public const string COLUMNTIMEZONE = "TimeZone";
        public const string COLUMNGUIDTIMEZONE = "GuidTimeZone";
        public const string COLUMNDESCRIPTION = "Description";
        public const string COLUMNVERSION = "Version";

        [LwSerializeAttribute()]
        public virtual Guid IdAccessZoneCar { get; set; }
        public virtual Car Car { get; set; }
        private Guid _guidCar = Guid.Empty;
        [LwSerializeAttribute()]
        public virtual Guid GuidCar { get { return _guidCar; } set { _guidCar = value; } }
        public virtual LprCamera LprCamera { get; set; }
        [LwSerializeAttribute()]
        public virtual Guid GuidLprCamera { get; set; }
        public virtual TimeZone TimeZone { get; set; }
        private Guid _guidTimeZone = Guid.Empty;
        [LwSerializeAttribute()]
        public virtual Guid GuidTimeZone { get { return _guidTimeZone; } set { _guidTimeZone = value; } }
        public virtual string Description { get; set; }

        public override bool Compare(object obj)
        {
            if (obj == null)
                return false;

            if (obj is AccessZoneCar)
            {
                return (obj as AccessZoneCar).IdAccessZoneCar == IdAccessZoneCar;
            }

            return false;
        }

        public virtual void PrepareToSend()
        {
            GuidCar = Car?.IdCar ?? Guid.Empty;
            GuidLprCamera = LprCamera?.IdLprCamera ?? Guid.Empty;
            GuidTimeZone = TimeZone?.IdTimeZone ?? Guid.Empty;
        }

        public override string GetIdString()
        {
            return IdAccessZoneCar.ToString();
        }

        public override object GetId()
        {
            return IdAccessZoneCar;
        }

        public override IModifyObject CreateModifyObject()
        {
            return null;
        }

        public override ObjectType GetObjectType()
        {
            return ObjectType.NotSupport;
        }
    }


    public class AccessZoneCarShort : IShortObject
    {
        public AccessZoneCar AccessZoneCar { get; private set; }

        public AccessZoneCarShort(AccessZoneCar accessZoneCar)
        {
            AccessZoneCar = accessZoneCar;
        }

        public ObjectType ObjectType
        {
            get { return ObjectType.NotSupport; }
        }

        public string GetSubTypeImageString(object value)
        {
            return string.Empty;
        }

        public string Name
        {
            get { return AccessZoneCar.LprCamera != null ? AccessZoneCar.LprCamera.ToString() : string.Empty; }
        }

        public object Id
        {
            get { return AccessZoneCar.IdAccessZoneCar; }
        }
    }
}
