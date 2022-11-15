using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class ParameterAdditionRepository : IParameterAdditionRepository
    {
        private readonly ApplicationDbContext db;
        public ParameterAdditionRepository(ApplicationDbContext db) => this.db = db;
        public ParameterAddition Add(ParameterAddition parameterAddition)
        {
            ParameterAddition add;
            var newElement = new ParameterAddition() { Name = parameterAddition.Name, ValueType = parameterAddition.ValueType };
            db.ParameterAdditions.Add(add = newElement);
            db.SaveChanges();
            return add;
        }
        public bool Delete(int Id)
        {
            bool result = false;
            //Удаление результатов
            var ResultValues = db.ResultValues.Where(x => x.ParameterAddition.Id == Id).ToList();
            foreach (var item in ResultValues)
                db.ResultValues.Remove(item);
            //Удаление доп параметров
            var select = db.ParameterAdditions.FirstOrDefault(x => x.Id == Id);
            if (select != null)
            {
                db.ParameterAdditions.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public bool DeleteAll()
        {
            bool result = false;
            if (db.ParameterAdditions.Count() != 0)
            {
                var select = db.ParameterAdditions.ToList();
                foreach (var item in select)
                    db.ParameterAdditions.Remove(item);
                db.SaveChanges();
                if (db.ParameterAdditions.Count() == 0)
                    result = true;
            }
            return result;
        }
        public ParameterAddition Edit(int id, ParameterAddition parameterAddition)
        {
            ParameterAddition edit = null;

            var select = db.ParameterAdditions.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Name = parameterAddition.Name;
                select.ValueType = parameterAddition.ValueType;
                db.SaveChanges();
                edit = select;
            }
            return edit;
        }
        public ParameterAddition Find(string Name) => db.ParameterAdditions.FirstOrDefault(x => x.Name == Name);
        public ParameterAddition Find(int Id) => db.ParameterAdditions.FirstOrDefault(x => x.Id == Id);
        public IEnumerable<ParameterAddition> GetList() => db.ParameterAdditions.ToList();
    }
}
