using Contal.Cgp.BaseLib;
using Contal.Cgp.Globals;
using Contal.Cgp.NCAS.Globals;
using Contal.Cgp.NCAS.RemotingCommon;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.BaseLib;
using Contal.Cgp.Server;
using Contal.Cgp.Server.Beans;
using Contal.Cgp.Server.DB;
using System;
using System.Collections.Generic;

namespace Contal.Cgp.NCAS.Server.DB
{
    public sealed class CarDoorEnvironments :
        ANcasBaseOrmTable<CarDoorEnvironments, CarDoorEnvironment>,
        ICarDoorEnvironments
    {
        private CarDoorEnvironments()
            : base(
                  null)
        {
        }

        public override AOrmObject GetObjectParent(AOrmObject ormObject)
        {
            var carDoorEnvironment = ormObject as CarDoorEnvironment;

            return carDoorEnvironment?.DoorEnvironment;
        }

        protected override void LoadObjectsInRelationship(CarDoorEnvironment obj)
        {
            if (obj?.DoorEnvironment != null)
            {
                obj.DoorEnvironment = DoorEnvironments.Singleton.GetById(obj.DoorEnvironment.IdDoorEnvironment);
            }

            if (obj?.Car != null)
            {
                obj.Car = Cars.Singleton.GetById(obj.Car.IdCar);
            }
        }

        public override bool HasAccessView(Login login)
        {
            return AccessChecker.HasAccessControl(
                NCASAccess.GetAccessesForGroup(AccessNcasGroups.DOOR_ENVIRONMENTS),
                login);
        }

        public override bool HasAccessInsert(Login login)
        {
            return AccessChecker.HasAccessControl(
                NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsInsertDeletePerform),
                login);
        }

        public override bool HasAccessUpdate(Login login)
        {
            return AccessChecker.HasAccessControl(
                NCASAccess.GetAccessesForGroup(AccessNcasGroups.DOOR_ENVIRONMENTS),
                login);
        }

        public override bool HasAccessDelete(Login login)
        {
            return AccessChecker.HasAccessControl(
                NCASAccess.GetAccess(AccessNCAS.DoorEnvironmentsInsertDeletePerform),
                login);
        }

        public override void CUDSpecial(CarDoorEnvironment ormObject, ObjectDatabaseAction objectDatabaseAction)
        {
        }

        public override ObjectType ObjectType => ObjectType.NotSupport;

        public ICollection<CarDoorEnvironment> GetByDoorEnvironmentId(
    Guid idDoorEnvironment,
    out Exception error)
        {
            error = null;

            var filterSettings = new List<FilterSettings>
            {
                new FilterSettings(
                    CarDoorEnvironment.COLUMN_DOOR_ENVIRONMENT,
                    idDoorEnvironment,
                    ComparerModes.EQUALL)
            };

            return SelectByCriteria(filterSettings, out error);
        }
    }
}
