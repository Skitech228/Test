using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class EnergyObjectRepository : IEnergyObjectRepository
    {
        private readonly ApplicationDbContext db;
        public EnergyObjectRepository(ApplicationDbContext db) => this.db = db;
        public EnergyObject Add(string Name)
        {

            EnergyObject add = null;
            var select = db.EnergyObjects.FirstOrDefault(x => x.Name == Name);
            if (select == null)
            {
                var newElement = new EnergyObject() { Name = Name };
                db.EnergyObjects.Add(newElement);
                db.SaveChanges();
                add = newElement;
            }
            return add;
        }

        public bool DeleteAll()
        {
            bool result = false;
            if (db.EnergyObjects.Count() != 0)
            {
                var select = db.EnergyObjects.ToList();
                foreach (var item in select)
                {
                    db.EnergyObjects.Remove(item);
                }
                db.SaveChanges();
                if (db.EnergyObjects.Count() == 0)
                    result = true;
            }
            return result;
        }
        public bool Delete(int id)
        {
            bool result = false;
            var select = db.EnergyObjects.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                var orders = db.Orders.Where(x => x.EnergyObject.Id == select.Id).ToList();
                if (orders != null)
                    foreach (var order in orders)
                    {
                        order.EnergyObject = null;
                        db.SaveChanges();
                    }
                db.EnergyObjects.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public EnergyObject Edit(int id, string Name)
        {

            EnergyObject edit = null;
            var select = db.EnergyObjects.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Name = Name;
                db.SaveChanges();
                edit = select;
            }
            return edit;
        }
        public EnergyObject Find(string Name)
        {

            EnergyObject find = null;
            var select = db.EnergyObjects.FirstOrDefault(x => x.Name == Name);
            if (select != null)
                find = select;
            return find;
        }
        public EnergyObject Find(int Id)
        {

            EnergyObject find = null;
            var select = db.EnergyObjects.FirstOrDefault(x => x.Id == Id);
            if (select != null)
                find = select;
            return find;
        }
        public IEnumerable<EnergyObject> GetList() => db.EnergyObjects.ToList();
    }
}
