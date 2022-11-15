using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IOrderTypeRepository
    {
        IEnumerable<OrderType> GetList();
        OrderType Add(string Name);
        OrderType Find(string Name);
        OrderType Edit(int id, string Name);
        bool Delete(int Id);
        bool RemoveAll();
    }
}
