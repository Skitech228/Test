using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class UnitRepository : IUnitRepository
    {
        private readonly ApplicationDbContext db;
        public UnitRepository(ApplicationDbContext db) => this.db = db;
        public Unit Add(Unit unit)
        {
            Unit add = null;
            //var select = db.Units.FirstOrDefault(x => x.Name == unit.Name);
            var select = db.Units.Include(x => x.Prefix)
                                 .Where(x => x.Id == unit.Id)
                                 .FirstOrDefault();
            if (select == null)
            {
                var newEl = new Unit
                {
                    Name = unit.Name,
                    Synonim = unit.Synonim
                };
                var prefix = unit.Prefix;
                if (prefix != null)
                    newEl.Prefix = db.Prefixes.Where(x => x.Id == prefix.Id).FirstOrDefault();
                db.Units.Add(newEl);
                db.SaveChanges();
                add = unit;
            }
            return add;
        }
        public bool DeleteAll()
        {
            bool result = false;
            if (db.Units.Count() != 0)
            {
                var select = db.Units.ToList();
                foreach (var item in select)
                    db.Units.Remove(item);
                db.SaveChanges();
                if (db.Units.Count() == 0)
                    result = true;
            }
            return result;
        }
        public bool Delete(int id)
        {

            bool result = false;

            //var sel = db.Units.FirstOrDefault(x => x.Name == unit.Name);
            var sel = db.Units.Where(x => x.Id == id).FirstOrDefault();

            if (sel != null)
            {
                var param = db.DeviceParameters.Include(x => x.Unit).Where(x => x.Unit.Id == sel.Id).FirstOrDefault();
                if (param != null)
                    param.Unit = null;
                db.Units.Remove(sel);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public Unit Edit(int id, Unit unit)
        {
            Unit edit = null;
            var select = db.Units.Include(x => x.Prefix)
                                 .Where(x => x.Id == id)
                                 .FirstOrDefault();
            if (select != null)
            {
                select.Name = unit.Name;
                if (!string.IsNullOrEmpty(unit.Synonim))
                    select.Synonim = unit.Synonim;
                if (!(unit.Prefix is null))
                    select.Prefix = db.Prefixes.Where(x => x.Id == unit.Prefix.Id).FirstOrDefault();
                db.SaveChanges();
                edit = select;
            }
            return edit;
        }
        public Unit Find(string Name)
        {
            Unit find = null;
            var select = db.Units.FirstOrDefault(x => x.Name == Name);
            if (select != null)
                find = select;
            return find;
        }
        public IEnumerable<Unit> GetList() => db.Units.Include(x => x.Prefix).ToList();
        public bool AddPrefix(int id, Prefix prefix)
        {
            bool result = false;
            var select = db.Units.FirstOrDefault(x => x.Id == id);
            var selPrefix = db.Prefixes.FirstOrDefault(x => x.Name == prefix.Name);
            if (select != null)
            {
                if (selPrefix != null)
                {
                    select.Prefix = selPrefix;
                    db.SaveChanges();
                    result = true;
                }
            }
            return result;
        }
        public bool DeletePrefix(int id)
        {
            bool result = false;
            var select = db.Units.Include(x => x.Prefix).FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Prefix = null;
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public Prefix GetPrefix(int id) => db.Units.Include(x => x.Prefix).FirstOrDefault(x => x.Id == id)?.Prefix;
    }
}
