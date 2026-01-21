using System;
using System.Collections.Generic;
using System.Linq;

using Contal.Cgp.NCAS.RemotingCommon;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.NCAS.Server.CcuDataReplicator;
using Contal.Cgp.Server.Beans;
using Contal.Cgp.Server.DB;
using Contal.Cgp.Globals;

namespace Contal.Cgp.NCAS.Server.DB
{
    public sealed class ACLCars :
        ANcasBaseOrmTable<ACLCars, ACLCar>,
        IACLCars
    {
        private ACLCars()
            : base(
                  null,
                  new CudPreparationForObjectWithVersion<ACLCar>())
        {
        }

        public override bool HasAccessView(Login login)
        {
            return AccessChecker.HasAccessControl(
                BaseAccess.GetAccess(LoginAccess.PersonsAccessControlListsAccessZonesView),
                login);
        }

        public override bool HasAccessInsert(Login login)
        {
            return AccessChecker.HasAccessControl(
                BaseAccess.GetAccess(LoginAccess.PersonsAccessControlListsAccessZonesAdmin),
                login);
        }

        public override bool HasAccessUpdate(Login login)
        {
            return AccessChecker.HasAccessControl(
                BaseAccess.GetAccess(LoginAccess.PersonsAccessControlListsAccessZonesAdmin),
                login);
        }

        public override bool HasAccessDelete(Login login)
        {
            return AccessChecker.HasAccessControl(
                BaseAccess.GetAccess(LoginAccess.PersonsAccessControlListsAccessZonesAdmin),
                login);
        }

        protected override void LoadObjectsInRelationship(ACLCar obj)
        {
            if (obj == null)
                return;

            if (obj.AccessControlList != null)
            {
                obj.AccessControlList = AccessControlLists.Singleton.GetById(obj.AccessControlList.IdAccessControlList);
            }

            if (obj.Car != null)
            {
                obj.Car = Cars.Singleton.GetById(obj.Car.IdCar);
            }
        }

        public ICollection<ACLCar> GetAclCarsByCar(Guid idCar, out Exception error)
        {
            error = null;
            ICollection<ACLCar> resultAclCar = new List<ACLCar>();

            try
            {
                var car = Cars.Singleton.GetById(idCar);
                if (car != null)
                {
                    var aclCars = GetAssignedAclCars(car.IdCar);

                    if (aclCars != null)
                        return aclCars.OrderBy(acl => acl.ToString()).ToList();
                }
            }
            catch (Exception exError)
            {
                error = exError;
            }

            return resultAclCar;
        }

        public IList<string> CarAclAssignment(
            IList<object> cars,
            IList<Guid> idAcls,
            DateTime? dateFrom,
            DateTime? dateTo)
        {
            IList<string> errorsStr = new List<string>();
            var acls = new List<AOrmObjectWithVersion>();

            foreach (var idAcl in idAcls)
            {
                var acl = AccessControlLists.Singleton.GetById(idAcl);
                if (acl == null) continue;

                acls.Add(acl);

                foreach (var objIdCar in cars)
                {
                    var car = Cars.Singleton.GetById(objIdCar);
                    if (car == null) continue;
                    try
                    {
                        AssignAclCar(car, acl, dateFrom, dateTo);
                    }
                    catch
                    {
                        errorsStr.Add(GetErrorString(car, acl));
                    }
                }
            }

            if (acls.Count > 0)
                DataReplicationManager.Singleton.SendModifiedObjectsToCcus(acls);

            return errorsStr.Count != 0
                ? errorsStr
                : null;
        }

        private static string GetErrorString(Car car, AccessControlList acl)
        {
            var resultStr = string.Empty;
            try
            {
                if (car != null)
                    resultStr += car + " ";

                if (acl != null)
                    resultStr += acl.ToString();
            }
            catch { }
            return resultStr;
        }

        private ACLCar AssignAclCar(Car car, AccessControlList acl, DateTime? dateFrom, DateTime? dateTo)
        {
            if (car == null || acl == null)
                return null;

            if (dateFrom != null && dateTo != null && (dateFrom.Value > dateTo.Value))
                return null;

            var aclCar = GetAssignedAclCar(car, acl);
            if (aclCar == null)
            {
                aclCar = new ACLCar
                {
                    AccessControlList = acl,
                    Car = car,
                    DateFrom = dateFrom,
                    DateTo = dateTo
                };

                if (InsertOnlyInDatabase(ref aclCar))
                    return aclCar;
            }
            else
            {
                if (aclCar.DateFrom != dateFrom || aclCar.DateTo != dateTo)
                {
                    aclCar = GetObjectForEdit(aclCar.IdACLCar);
                    aclCar.DateFrom = dateFrom;
                    aclCar.DateTo = dateTo;

                    if (UpdateOnlyInDatabase(aclCar))
                        return aclCar;
                }
            }

            return null;
        }

        private ACLCar GetAssignedAclCar(Car car, AccessControlList acl)
        {
            var result = SelectLinq<ACLCar>(aclCar => aclCar.Car == car && aclCar.AccessControlList == acl);
            if (result != null && result.Count > 0)
                return result.First();

            return null;
        }

        public ICollection<ACLCar> GetAssignedAclCars(Guid idCar)
        {
            var aclCarsDb = SelectLinq<ACLCar>(
                aclCar =>
                    aclCar.Car != null &&
                    aclCar.Car.IdCar == idCar);

            if (aclCarsDb == null)
                return null;

            foreach (var aclCar in aclCarsDb)
                LoadObjectsInRelationship(aclCar);

            return aclCarsDb;
        }

        public override ObjectType ObjectType
        {
            get { return ObjectType.NotSupport; }
        }
    }
}
