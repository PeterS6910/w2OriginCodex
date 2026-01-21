using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.Server.Beans;
using System;
using System.Collections.Generic;

namespace Contal.Cgp.NCAS.RemotingCommon
{
    public interface IACLCars : IBaseOrmTable<ACLCar>
    {
        ICollection<ACLCar> GetAclCarsByCar(Guid idCar, out Exception error);
        IList<string> CarAclAssignment(IList<object> cars, IList<Guid> idAcls, DateTime? dateFrom, DateTime? dateTo);
    }
}
