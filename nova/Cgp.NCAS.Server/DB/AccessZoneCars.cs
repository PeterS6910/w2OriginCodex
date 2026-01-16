using System;
using System.Collections.Generic;
using System.Linq;

using Contal.Cgp.NCAS.RemotingCommon;
using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.Server.Beans;
using Contal.Cgp.Server.DB;
using Contal.Cgp.Globals;

namespace Contal.Cgp.NCAS.Server.DB
{
    public sealed class AccessZoneCars :
        ANcasBaseOrmTable<AccessZoneCars, AccessZoneCar>,
        IAccessZoneCars
    {
        private AccessZoneCars()
            : base(
                  null,
                  new CudPreparationForObjectWithVersion<AccessZoneCar>())
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

        protected override void LoadObjectsInRelationship(AccessZoneCar obj)
        {
            if (obj == null)
                return;

            if (obj.Car != null)
            {
                obj.Car = Cars.Singleton.GetById(obj.Car.IdCar);
            }

            if (obj.LprCamera != null)
            {
                obj.LprCamera = LprCameras.Singleton.GetById(obj.LprCamera.IdLprCamera);
            }

            if (obj.TimeZone != null)
            {
                obj.TimeZone = TimeZones.Singleton.GetById(obj.TimeZone.IdTimeZone);
            }
        }

        public ICollection<AccessZoneCar> GetAccessZonesByCar(
            Guid idCar,
            out Exception error)
        {
            error = null;

            try
            {
                Car car = Cars.Singleton.GetById(idCar);
                if (car != null)
                {
                    ICollection<AccessZoneCar> accessZoneCarList =
                        GetAssignedAccessZones(
                            car.IdCar);

                    if (accessZoneCarList != null)
                        return accessZoneCarList.OrderBy(azc => azc.ToString()).ToList();
                }
            }
            catch (Exception exError)
            {
                error = exError;
            }

            return null;
        }

        public ICollection<AccessZoneCar> GetAssignedAccessZones(Guid idCar)
        {
            ICollection<AccessZoneCar> accessZonesDB =
                SelectLinq<AccessZoneCar>(
                    accessZoneCar =>
                        accessZoneCar.Car != null &&
                        accessZoneCar.Car.IdCar == idCar);

            if (accessZonesDB == null)
                return null;

            foreach (AccessZoneCar accessZoneCar in accessZonesDB)
                LoadObjectsInRelationship(accessZoneCar);

            return accessZonesDB;
        }

        public override ObjectType ObjectType
        {
            get { return ObjectType.NotSupport; }
        }
    }
}
