using Contal.Cgp.NCAS.Server.Beans;
using Contal.Cgp.Server.Beans;
using System;
using System.Collections.Generic;

namespace Contal.Cgp.NCAS.RemotingCommon
{
    public interface ICarDoorEnvironments : IBaseOrmTable<CarDoorEnvironment>
    {
        ICollection<CarDoorEnvironment> GetByDoorEnvironmentId(Guid idDoorEnvironment, out Exception error);
    }
}
