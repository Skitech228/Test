#region Using derectives

using System.Collections.Generic;
using System.Threading.Tasks;

#endregion

namespace SysTest.Win.EntityService
{
    public interface IService<Entity> where Entity : class
    {
        Task<bool> AddAsync(Entity entity);

        Task<bool> RemoveAsync(Entity entity);

        Task<bool> UpdateAsync(Entity entity);

        Task<Entity> GetByIdAsync(int id);

        Task<IEnumerable<Entity>> GetAllAsync();
    }
}