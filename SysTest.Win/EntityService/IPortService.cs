#region Using derectives

using System.Collections.Generic;
using System.Threading.Tasks;
using SysTest.Win.Database.Entity;

#endregion

namespace SysTest.Win.EntityService
{
    public interface IPortService : IService<Port>
    {
        Task<List<string>> GetAllPortsAsync();

        Task<List<Port>> GetAllTCPPortsAsync();

        Task<List<Port>> GetAllBTPortsAsync();
        
        Task<List<Port>> GetAllConnectedPortsAsync();
    }
}