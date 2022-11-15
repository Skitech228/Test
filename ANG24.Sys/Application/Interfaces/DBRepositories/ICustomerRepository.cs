using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface ICustomerRepository
    {
        IEnumerable<Customer> GetList();
        Customer Add(string Name);
        Customer Edit(int id, string name);
        bool Delete(int id);
        Customer Find(string Name);
        Customer Find(int Id);
        //служебная операция
        bool DeleteAll();
    }
}
