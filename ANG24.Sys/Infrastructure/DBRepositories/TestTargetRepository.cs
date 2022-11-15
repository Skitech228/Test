using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class TestTargetRepository : ITestTargetRepository
    {
        private readonly ApplicationDbContext db;
        public TestTargetRepository(ApplicationDbContext db) => this.db = db;
        public TestTarget Add(string Name)
        {
            TestTarget add = null;
            var select = db.TestTargets.FirstOrDefault(x => x.Name == Name);
            if (select == null)
            {
                var newElement = new TestTarget() { Name = Name };
                db.TestTargets.Add(newElement);
                db.SaveChanges();
                add = newElement;
            }
            return add;
        }
        public bool DeleteAll()
        {
            bool result = false;
            if (db.TestTargets.Count() != 0)
            {
                var select = db.TestTargets.ToList();
                foreach (var item in select)
                    db.TestTargets.Remove(item);
                db.SaveChanges();
                if (db.TestTargets.Count() == 0)
                    result = true;
            }
            return result;
        }
        public bool Delete(int id)
        {
            bool result = false;
            var select = db.TestTargets.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                var orders = db.Orders.Where(x => x.TestTarget.Id == select.Id).ToList();
                if (orders != null)
                    foreach (var order in orders)
                    {
                        order.TestTarget = null;
                        db.SaveChanges();
                    }
                db.TestTargets.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public TestTarget Edit(int id, string Name)
        {
            TestTarget edit = null;
            var select = db.TestTargets.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Name = Name;
                db.SaveChanges();
                edit = select;
            }
            return edit;
        }
        public TestTarget Find(string Name) => db.TestTargets.FirstOrDefault(x => x.Name == Name);
        public TestTarget Find(int Id) => db.TestTargets.FirstOrDefault(x => x.Id == Id);
        public IEnumerable<TestTarget> GetList() => db.TestTargets.ToList();
    }
}
