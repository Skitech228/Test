using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class OrderTypeRepository : IOrderTypeRepository
    {
        private readonly ApplicationDbContext db;


        public OrderTypeRepository(ApplicationDbContext db) => this.db = db;

        public OrderType Add(string Name)
        {
            OrderType add = null;
            var select = db.OrderTypes.FirstOrDefault(x => x.Name == Name);
            if (select == null)
            {
                var newElement = new OrderType() { Name = Name };
                db.OrderTypes.Add(newElement);
                db.SaveChanges();
                add = newElement;
            }
            return add;
        }
        public bool Delete(int Id)
        {
            bool result = false;
            var select = db.OrderTypes.FirstOrDefault(x => x.Id == Id);
            if (select != null)
            {
                var orders = db.Orders.Where(x => x.OrderType.Id == select.Id).ToList();
                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        order.OrderType = null;
                        db.SaveChanges();
                    }
                }
                db.OrderTypes.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public OrderType Edit(int id, string Name)
        {
            OrderType edit = null;
            var select = db.OrderTypes.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Name = Name;
                db.SaveChanges();
                edit = select;
            }
            return edit;
        }
        public OrderType Find(string Name) => db.OrderTypes.FirstOrDefault(x => x.Name == Name);
        public IEnumerable<OrderType> GetList() => db.OrderTypes.ToList();
        public bool RemoveAll()
        {
            bool result = false;
            if (db.OrderTypes.Count() != 0)
            {
                var select = db.OrderTypes.ToList();
                foreach (var item in select)
                    db.OrderTypes.Remove(item);
                db.SaveChanges();
                if (db.OrderTypes.Count() == 0)
                    result = true;
            }
            return result;
        }
    }
}
