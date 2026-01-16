using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.Server.Beans;
using System;
using System.Collections.Generic;

namespace Contal.Cgp.NCAS.RemotingCommon
{
    public interface IAccessZoneCars : IBaseOrmTable<AccessZoneCar>
    {
        ICollection<AccessZoneCar> GetAccessZonesByCar(Guid idCar, out Exception error);
        ICollection<AccessZoneCar> GetAssignedAccessZones(Guid idCar);
    }
}
