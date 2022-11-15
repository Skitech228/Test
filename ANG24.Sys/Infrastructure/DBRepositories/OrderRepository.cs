using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext db;
        public OrderRepository(ApplicationDbContext db) => this.db = db;
        public Order Add(Order order)
        {
            Order add = null;
            var select = db.Orders.FirstOrDefault(x => x.Number == order.Number);
            if (select == null)
            {
                var newEl = new Order
                {
                    Number = order.Number,
                    CreateDate = order.CreateDate,
                    Description = order.Description,
                    EnergyObject = db.EnergyObjects.Where(x => x.Id == order.EnergyObject.Id).FirstOrDefault(),
                    TestTarget = db.TestTargets.Where(x => x.Id == order.TestTarget.Id).FirstOrDefault(),
                    TestObject = db.TestObjects.Where(x => x.Id == order.TestObject.Id).FirstOrDefault(),
                    OrderType = db.OrderTypes.Where(x => x.Id == order.OrderType.Id).FirstOrDefault()
                };
                if (order.Customers != null)
                    foreach (var item in order.Customers)
                        newEl.Customers.Add(db.Customers.Where(x => x.Id == item.Id).FirstOrDefault());
                if (order.Devices != null)
                    foreach (var item in order.Devices)
                        newEl.Devices.Add(db.Devices.Where(x => x.Id == item.Id).FirstOrDefault());
                db.Orders.Add(newEl);
                db.SaveChanges();
            }
            return add;
        }
        public Order Edit(int id, Order order)
        {
            Order edit = null;
            var select = db.Orders.Include(x => x.EnergyObject)
                                  .Include(x => x.TestTarget)
                                  .Include(x => x.TestObject)
                                  .Include(x => x.OrderType)
                                  .Include(x => x.Customers)
                                  .Include(x => x.Devices)
                                  .FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Number = order.Number;
                select.CreateDate = order.CreateDate;
                select.Description = order.Description;
                select.EnergyObject = db.EnergyObjects.Where(x => x.Id == order.EnergyObject.Id).FirstOrDefault();
                select.TestTarget = db.TestTargets.Where(x => x.Id == order.TestTarget.Id).FirstOrDefault();
                select.TestObject = db.TestObjects.Where(x => x.Id == order.TestObject.Id).FirstOrDefault();
                select.OrderType = db.OrderTypes.Where(x => x.Id == order.OrderType.Id).FirstOrDefault();
                select.Customers.Clear();
                if (order.Customers != null)
                {
                    var customers = order.Customers.ToList();
                    foreach (var item in customers)
                        select.Customers.Add(db.Customers.Where(x => x.Id == item.Id).FirstOrDefault());
                }
                select.Devices.Clear();
                if (order.Devices != null)
                {
                    var devices = order.Devices.ToList();
                    foreach (var item in devices)
                        select.Devices.Add(db.Devices.Where(x => x.Id == item.Id).FirstOrDefault());
                }
                db.SaveChanges();
                edit = select;
            }
            return edit;
        }
        public Order Find(int Id)
        {
            Order find = null;
            var select = db.Orders.Include(x => x.Customers)
                                  .Include(x => x.Devices)
                                  .Include(x => x.EnergyObject)
                                  .Include(x => x.TestObject)
                                  .Include(x => x.TestTarget)
                                  .Include(x => x.OrderType)
                                  .Include(x => x.ResultValues)
                                  .Include(x => x.FazeMeteringResults)
                                  .Where(x => x.Id == Id)
                                  .FirstOrDefault();

            if (select != null)
            {
                List<Device> devicesBuffer = new List<Device>();
                foreach (var item in select.Devices)
                {
                    var target = db.Devices.Include(x => x.DeviceParameters)
                                           .Include(x => x.DeviceGroup)
                                           .Where(x => x.Id == item.Id)
                                           .FirstOrDefault();
                    if (target != null)
                    {
                        List<DeviceParameter> parametersBuffer = new List<DeviceParameter>();
                        foreach (var deviceParameter in target.DeviceParameters)
                        {
                            var parameter = db.DeviceParameters
                                                            .Include(x => x.ParameterAdditions)
                                                            .Include(x => x.Unit)
                                                            .Include(x => x.Unit.Prefix)
                                                            .Where(x => x.Id == deviceParameter.Id)
                                                            .FirstOrDefault();
                            if (parameter != null)
                                parametersBuffer.Add(parameter);
                        }
                        target.DeviceParameters.Clear();
                        target.DeviceParameters = parametersBuffer.ToList();
                        devicesBuffer.Add(target);
                    }
                }
                select.Devices = devicesBuffer.ToList();

                if (select.TestObject != null)
                {
                    var testObject = db.TestObjects.Include(x => x.Fazes).Where(x => x.Id == select.TestObject.Id).FirstOrDefault();
                    if (testObject != null) select.TestObject = testObject;
                }
                find = select;
            }
            return find;
        }
        public bool Delete(int id)
        {
            bool result = false;
            var select = db.Orders.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                db.Orders.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public IEnumerable<Order> GetList()
        {
            return db.Orders.Include(x => x.OrderType)
                            .Include(x => x.TestObject)
                            .Include(x => x.TestTarget)
                            .Include(x => x.Customers)
                            .Include(x => x.EnergyObject)
                            .Include(x => x.Devices)
                            .ToList();
        }
        public bool DeleteAll()
        {
            bool result = false;
            if (db.Orders.Count() != 0)
            {
                var select = db.Orders.ToList();
                foreach (var item in select)
                {
                    db.Orders.Remove(item);
                }
                db.SaveChanges();
                if (db.Orders.Count() == 0)
                    result = true;
            }
            return result;
        }
        public bool AddTestTarget(int orderId, TestTarget testTarget)
        {
            bool result = false;
            var select = db.Orders.FirstOrDefault(x => x.Id == orderId);
            var selTestTarget = db.TestTargets.FirstOrDefault(x => x.Name == testTarget.Name);
            if (select != null)
                if (selTestTarget != null)
                {
                    select.TestTarget = selTestTarget;
                    db.SaveChanges();
                    result = true;
                }
            return result;
        }
        public bool DeleteTestTarget(int orderId)
        {
            bool result = false;
            var select = db.Orders.Include(x => x.TestTarget).FirstOrDefault(x => x.Id == orderId);
            if (select != null)
            {
                select.TestTarget = null;
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public TestTarget GetTestTarget(int orderId) => db.Orders.Include(x => x.TestTarget).FirstOrDefault(x => x.Id == orderId)?.TestTarget;
        public bool AddDevice(int orderId, Device device)
        {
            bool result = false;
            var select = db.Orders.FirstOrDefault(x => x.Id == orderId);
            if (select != null)
            {
                var devices = db.Devices.FirstOrDefault(x => x.Name == device.Name);
                if (devices != null)
                    if (!select.Devices.ToList().Contains(devices))
                    {
                        select.Devices.Add(devices);
                        db.SaveChanges();
                        result = true;
                    }
            }
            return result;
        }
        public bool DeleteDevice(int orderId, Device device)
        {

            bool result = false;
            var select = db.Orders.Include(x => x.Devices).FirstOrDefault(x => x.Id == orderId);
            if (select != null)
            {
                var devices = db.Devices.FirstOrDefault(x => x.Name == device.Name);
                if (devices != null)
                {
                    var localResult = select.Devices.Remove(devices);
                    if (localResult)
                    {
                        db.SaveChanges();
                        result = true;
                    }
                }
            }
            return result;
        }
        public IEnumerable<Device> GetDevices(int orderId) => db.Orders.Include(x => x.Devices).FirstOrDefault(x => x.Id == orderId)?.Devices?.ToList();
        public bool AddCustomer(int orderId, Customer customer)
        {
            bool result = false;
            var select = db.Orders.FirstOrDefault(x => x.Id == orderId);
            if (select != null)
            {
                var customers = db.Customers.FirstOrDefault(x => x.Id == customer.Id);
                if (customers != null)
                    if (!select.Customers.ToList().Contains(customers))
                    {
                        select.Customers.Add(customers);
                        db.SaveChanges();
                        result = true;
                    }
            }
            return result;
        }
        public bool DeleteCustomer(int orderId, Customer customer)
        {
            bool result = false;
            var select = db.Orders.Include(x => x.Customers).FirstOrDefault(x => x.Id == orderId);
            if (select != null)
            {
                var customers = db.Customers.FirstOrDefault(x => x.Name == customer.Name);
                if (customers != null)
                {
                    var localResult = select.Customers.Remove(customers);
                    if (localResult)
                    {
                        db.SaveChanges();
                        result = true;
                    }
                }
            }
            return result;
        }
        public IEnumerable<Customer> GetCustomers(int orderId) => db.Orders.Include(x => x.Customers).FirstOrDefault(x => x.Id == orderId)?.Customers?.ToList();
        public bool AddEnergyObject(int orderId, EnergyObject energyObject)
        {
            bool result = false;
            var select = db.Orders.FirstOrDefault(x => x.Id == orderId);
            var selEnergyObject = db.EnergyObjects.FirstOrDefault(x => x.Name == energyObject.Name);
            if (select != null)
            {
                if (selEnergyObject != null)
                {
                    select.EnergyObject = selEnergyObject;
                    db.SaveChanges();
                    result = true;
                }
            }
            return result;
        }
        public bool DeleteEnergyObject(int orderId)
        {
            bool result = false;

            var select = db.Orders.Include(x => x.EnergyObject).FirstOrDefault(x => x.Id == orderId);
            if (select != null)
            {
                select.EnergyObject = null;
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public EnergyObject GetEnergyObject(int orderId) => db.Orders.Include(x => x.TestTarget).FirstOrDefault(x => x.Id == orderId)?.EnergyObject;
        public TestObject AddTestObject(int orderId, TestObject testObject)
        {
            TestObject add = null;
            var select = db.Orders.Include(x => x.TestObject)
                                  .Where(x => x.Id == orderId)
                                  .FirstOrDefault();
            if (select != null)
            {
                var selectTestObject = db.TestObjects.Where(x => x.Id == testObject.Id).FirstOrDefault();
                if (selectTestObject != null)
                {
                    select.TestObject = selectTestObject;
                    db.SaveChanges();
                    add = selectTestObject;
                }
                else throw new ArgumentNullException(nameof(selectTestObject), "Не удалось найти указанный элемент");
            }
            else throw new ArgumentNullException(nameof(select), "Не удалось найти указанный элемент");
            return add;
        }
        public bool DeleteTestObject(int orderId)
        {
            bool res = false;
            var select = db.Orders.Include(x => x.TestObject)
                                  .Where(x => x.Id == orderId)
                                  .FirstOrDefault();
            if (select != null)
            {
                select.TestObject = null;
                db.SaveChanges();
                res = true;
            }
            else throw new ArgumentNullException(nameof(select), "Не удалось найти указанный элемент");
            return res;
        }
        public TestObject GetTestObject(int orderId) => db.Orders.Include(x => x.TestObject).Where(x => x.Id == orderId).FirstOrDefault()?.TestObject;
    }
}
