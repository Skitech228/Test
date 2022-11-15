using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IOrderRepository
    {
        IEnumerable<Order> GetList();
        Order Find(int Id);
        Order Add(Order order);
        Order Edit(int id, Order order);
        bool Delete(int id);
        bool DeleteAll();

        TestTarget GetTestTarget(int OrderId);
        bool AddTestTarget(int OrderId, TestTarget testTarget);
        bool DeleteTestTarget(int OrderId);

        EnergyObject GetEnergyObject(int OrderId);
        bool AddEnergyObject(int OrderId, EnergyObject energyObject);
        bool DeleteEnergyObject(int OrderId);

        IEnumerable<Device> GetDevices(int OrderId);
        bool AddDevice(int OrderId, Device device);
        bool DeleteDevice(int OrderId, Device device);

        IEnumerable<Customer> GetCustomers(int OrderId);
        bool AddCustomer(int OrderId, Customer customer);
        bool DeleteCustomer(int OrderId, Customer customer);

        TestObject GetTestObject(int OrderId);
        TestObject AddTestObject(int OrderId, TestObject testObject);
        bool DeleteTestObject(int OrderId);
    }
}
