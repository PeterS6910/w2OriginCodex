using System;
using Contal.Cgp.Server.Beans;
using Contal.Cgp.Globals;
using Contal.IwQuick.Data;

namespace Contal.Cgp.NCAS.Server.Beans
{
    [Serializable]
    [LwSerialize(305)]
    [LwSerializeMode(LwSerializationMode.Selective, DirectMemberType.All)]
    [LwSerializeNoParent]
    public class ACLCar : AOrmObjectWithVersion
    {
        public const string COLUMN_ID_ACL_CAR = "IdACLCar";
        public const string COLUMN_ACCESS_CONTROL_LIST = "AccessControlList";
        public const string COLUMN_GUID_ACCESS_CONTROL_LIST = "GuidAccessControlList";
        public const string COLUMN_CAR = "Car";
        public const string COLUMN_GUID_CAR = "GuidCar";
        public const string COLUMN_DATE_FROM = "DateFrom";
        public const string COLUMN_DATE_TO = "DateTo";
        public const string COLUMN_VERSION = "Version";

        [LwSerialize]
        public virtual Guid IdACLCar { get; set; }

        public virtual AccessControlList AccessControlList { get; set; }

        [LwSerialize]
        public virtual Guid GuidAccessControlList { get; set; }

        public virtual Car Car { get; set; }

        [LwSerialize]
        public virtual Guid GuidCar { get; set; }

        [LwSerialize]
        public virtual DateTime? DateFrom { get; set; }

        [LwSerialize]
        public virtual DateTime? DateTo { get; set; }

        public ACLCar()
        {
            GuidCar = Guid.Empty;
            GuidAccessControlList = Guid.Empty;
        }

        public override string ToString()
        {
            return AccessControlList + " - " + Car;
        }

        public override bool Compare(object obj)
        {
            var aclCar = obj as ACLCar;

            return aclCar != null && aclCar.IdACLCar == IdACLCar;
        }

        public virtual void PrepareToSend()
        {
            GuidAccessControlList =
                AccessControlList != null
                    ? AccessControlList.IdAccessControlList
                    : Guid.Empty;

            GuidCar =
                Car != null
                    ? Car.IdCar
                    : Guid.Empty;
        }

        public override string GetIdString()
        {
            return IdACLCar.ToString();
        }

        public override object GetId()
        {
            return IdACLCar;
        }

        public override IModifyObject CreateModifyObject()
        {
            return null;
        }

        public override ObjectType GetObjectType()
        {
            return ObjectType.NotSupport;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ACLCar;

            if (other == null)
                return false;

            return
                IdACLCar == other.IdACLCar
                   && DateFrom == other.DateFrom
                   && DateTo == other.DateTo;
        }
    }
}
