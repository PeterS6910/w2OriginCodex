using Contal.Cgp.Globals;
using Contal.Cgp.NCAS.Server.Beans;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Contal.Cgp.Server.Beans
{
    [Serializable]
    public class Car : AOrmObjectWithVersion, IOrmObjectWithAlarmInstructions, IComparer
    {
        public const string COLUMNIDCAR = "IdCar";
        public const string COLUMNLP = "WholeName";
        public const string COLUMNBRAND = "Brand";
        public const string COLUMNVALIDITYDATEFROM = "ValidityDateFrom";
        public const string COLUMNVALIDITYDATETO = "ValidityDateTo";
        public const string COLUMNDESCRIPTION = "Description";
        public const string COLUMNDEPARTMENT = "Department";
        public const string COLUMNSECURITYLEVEL = "SecurityLevel";
        public const string COLUMN_TIMETEC_SYNC = "SynchronizedWithTimetec";
        public const string COLUMNUTC_DATE_STATE_LAST_CHANGE = "UtcDateStateLastChange";
        public const string COLUMNOBJECTTYPE = "ObjectType";
        public const string COLUMNCKUNIQUE = "CkUnique";
        public const string COLUMNVERSION = "Version";

        public virtual Guid IdCar { get; set; }
        private string _lp;
        public virtual string Lp
        {
            get { return _lp ?? WholeName; }
            set
            {
                _lp = value;
                WholeName = value;
            }
        }
        public virtual string WholeName { get; set; }
        public virtual string Brand { get; set; }
        public virtual DateTime? ValidityDateFrom { get; set; }
        public virtual DateTime? ValidityDateTo { get; set; }
        public virtual string Description { get; set; }
        public virtual bool SynchronizedWithTimetec { get; set; }
        public virtual DateTime UtcDateStateLastChange { get; set; }
        public virtual byte ObjectType
        {
            get { return (byte)GetObjectType(); }
            set { }
        }
        public virtual Guid CkUnique { get; set; }

        public Car()
        {
            CkUnique = Guid.NewGuid();
            UtcDateStateLastChange = DateTime.UtcNow;
            SecurityLevel = CarSecurityLevel.StandardLprAndCard;
        }

        public override string ToString()
        {
            return Lp ?? WholeName;
        }

        public override bool Compare(object obj)
        {
            var car = obj as Car;
            return car != null && car.IdCar == IdCar;
        }

        public override bool Contains(string expression)
        {
            expression = expression.ToLower();
            if (!string.IsNullOrEmpty(Lp) && Lp.ToLower().Contains(expression))
                return true;
            if (!string.IsNullOrEmpty(Brand) && Brand.ToLower().Contains(expression))
                return true;
            return !string.IsNullOrEmpty(Description) && Description.ToLower().Contains(expression);
        }

        public override string GetIdString()
        {
            return IdCar.ToString();
        }

        public override object GetId()
        {
            return IdCar;
        }

        public override IModifyObject CreateModifyObject()
        {
            return new CarModifyObj(this);
        }

        public override ObjectType GetObjectType()
        {
            return Globals.ObjectType.Car;
        }

        public virtual string GetLocalAlarmInstruction()
        {
            return null;
        }

        #region IComparer Members
        public virtual int Compare(object x, object y)
        {
            var cX = x as Car;
            var cY = y as Car;

            if (cX == null && cY == null)
                return 0;
            if (cX != null && cY == null)
                return 1;
            if (cX == null && cY != null)
                return -1;
            return string.Compare(cX.Lp, cY.Lp, StringComparison.Ordinal);
        }
        #endregion
    }

    [Serializable]
    public class CarShort : IShortObject
    {
        public const string COLUMNIDCAR = "IdCar";
        public const string COLUMNLP = "Lp";
        public const string COLUMNBRAND = "Brand";
        public const string COLUMNVALIDITYDATEFROM = "ValidityDateFrom";
        public const string COLUMNVALIDITYDATETO = "ValidityDateTo";
        public const string COLUMNSECURITYLEVEL = "SecurityLevel";
        public const string COLUMNDEPARTMENT = "Department";
        public const string COLUMNVERSION = "Version";
        public const string COLUMNDESCRIPTION = "Description";
        public const string COLUMN_SYMBOL = "Symbol";

        public Guid IdCar { get; set; }
        public string Lp { get; set; }
        public string Brand { get; set; }
        public DateTime? ValidityDateFrom { get; set; }
        public DateTime? ValidityDateTo { get; set; }
        public int Version { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public CarSecurityLevel SecurityLevel { get; set; }
        public Image Symbol { get; set; }

        public CarShort(Car car)
        {
            IdCar = car.IdCar;
            Lp = car.Lp ?? car.WholeName;
            Brand = car.Brand;
            ValidityDateFrom = car.ValidityDateFrom;
            ValidityDateTo = car.ValidityDateTo;
            Version = car.Version;
            Department = car.Department?.FolderName;
            SecurityLevel = car.SecurityLevel;
            Description = car.Description;
        }

        public object Id { get { return IdCar; } }
        public string Name { get { return Lp; } }
        public ObjectType ObjectType { get { return ObjectType.Car; } }
        public string GetSubTypeImageString(object value) { return string.Empty; }
    }

    [Serializable]
    public enum CarSecurityLevel : byte
    {
        VipLprOnly = 0,
        StandardLprAndCard = 1,
        HigherSecurityLprAndPin = 2,
        AlternativeLprAndCardOrPin = 3
    }

    [Serializable]
    public class CarModifyObj : AModifyObject
    {
        public override ObjectType GetOrmObjectType { get { return ObjectType.Car; } }

        public CarModifyObj(Car car)
        {
            Id = car.IdCar;
            FullName = car.Lp;
            Description = car.Description;
        }
    }
}
