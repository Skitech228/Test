using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class TestObjectRepository : ITestObjectRepository
    {
        private readonly IFazeRepository fazeRepository;
        private readonly ApplicationDbContext db;
        public TestObjectRepository(IFazeRepository fazeRepository, ApplicationDbContext db)
        {
            this.fazeRepository = fazeRepository;
            this.db = db;
        }
        public TestObject Add(TestObject testObject)
        {
            TestObject add = null;
            var select = db.TestObjects.FirstOrDefault(x => x.Id == testObject.Id);
            if (select == null)
            {
                var newElement = new TestObject
                {
                    Mark = testObject.Mark,
                    Work_U = testObject.Work_U,
                    DiametrSize = testObject.DiametrSize,
                    Lehght = testObject.Lehght
                };
                foreach (var item in testObject.Fazes)
                {
                    var faze = fazeRepository.Add(item);
                    newElement.Fazes.Add(db.Fazes.Where(x => x.Id == faze.Id).FirstOrDefault());
                }
                db.TestObjects.Add(newElement);
                db.SaveChanges();
                add = newElement;
            }
            return add;
        }
        public bool DeleteAll()
        {
            bool result = false;
            if (db.TestObjects.Count() != 0)
            {
                var select = db.TestObjects.ToList();
                foreach (var item in select)
                    db.TestObjects.Remove(item);
                db.SaveChanges();
                if (db.TestObjects.Count() == 0)
                    result = true;
            }
            return result;
        }
        public bool Delete(int id)
        {
            bool result = false;
            var testObject = db.TestObjects.FirstOrDefault(x => x.Id == id);
            if (testObject.Fazes != null && testObject.Fazes.Count != 0)
            {
                //Удалаение фазы
                var fazes = testObject.Fazes.ToList();
                foreach (var item in fazes)
                    fazeRepository.Delete(item.Id);
            }
            //Удаление Испытательного объекта
            var select = db.TestObjects.Include(x => x.Fazes).Where(x => x.Id == id).FirstOrDefault();
            if (select != null)
            {
                //Удаление связи из отчета
                var orders = db.Orders.Where(x => x.TestObject.Id == testObject.Id).ToList();
                if (orders != null)
                    foreach (var order in orders)
                    {
                        order.TestObject = null;
                        db.SaveChanges();
                    }
                db.TestObjects.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public TestObject Edit(int id, TestObject testObject)
        {
            TestObject edit = null;
            var select = db.TestObjects.Include(x => x.Fazes).Where(x => x.Id == id).FirstOrDefault();
            if (select != null)
            {
                foreach (var item in select.Fazes)
                    if (testObject.Fazes.All(x => x.Id != item.Id))
                        fazeRepository.Delete(item.Id);


                select.Mark = testObject.Mark;
                select.Work_U = testObject.Work_U;
                select.DiametrSize = testObject.DiametrSize;
                select.Lehght = testObject.Lehght;
                if (testObject.Fazes.Count != 0)
                {
                    foreach (var item in testObject.Fazes)
                    {
                        var faze = db.Fazes.Include(x => x.FazeMeteringResults).Where(x => x.Id == item.Id).FirstOrDefault();
                        if (faze == null)
                        {
                            var newFaze = fazeRepository.Add(item);
                            select.Fazes.Add(db.Fazes.Where(x => x.Id == newFaze.Id).FirstOrDefault());
                        }
                        else
                            fazeRepository.Edit(faze.Id, item);
                    }
                }
                db.SaveChanges();
                edit = select;
            }
            return edit;
        }
        public TestObject Find(string Mark) => db.TestObjects.FirstOrDefault(x => x.Mark == Mark);
        public IEnumerable<TestObject> GetList() => db.TestObjects.Include(x => x.Fazes).ToList();
    }
}
