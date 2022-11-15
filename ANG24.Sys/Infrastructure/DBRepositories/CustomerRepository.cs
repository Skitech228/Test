using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext db;
        public CustomerRepository(ApplicationDbContext db) => this.db = db;
        public Customer Add(string Name)
        {

            Customer add = null;
            var select = db.Customers.FirstOrDefault(x => x.Name == Name);
            if (select == null)
            {
                var newElement = new Customer() { Name = Name };
                db.Customers.Add(newElement);
                db.SaveChanges();
                add = newElement;
            }
            return add;
        }
        public bool DeleteAll()
        {

            bool result = false;
            if (db.Customers.Count() != 0)
            {
                var select = db.Customers.ToList();
                foreach (var item in select)
                {
                    db.Customers.Remove(item);
                }
                db.SaveChanges();
                if (db.Customers.Count() == 0)
                    result = true;
            }
            return result;
        }
        public bool Delete(int id)
        {

            bool result = false;

            var select = db.Customers.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                db.Customers.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public Customer Edit(int id, string Name)
        {

            Customer edit = null;
            var select = db.Customers.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Name = Name;
                db.SaveChanges();
                edit = select;
            }
            return edit;
        }
        public Customer Find(string Name)
        {

            Customer find = null;
            var select = db.Customers.FirstOrDefault(x => x.Name == Name);
            if (select != null)
                find = select;

            return find;
        }
        public Customer Find(int Id)
        {

            var select = db.Customers.FirstOrDefault(x => x.Id == Id);
            if (select != null)
                return select;
            else
                return null;

        }
        public IEnumerable<Customer> GetList()
        {

            var check = db.Customers.ToList();
            if (check != null && check.Count > 0)
                return check;
            else
                return null;
        }
    }
}
