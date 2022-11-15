using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class DeviceParameterRepository : IDeviceParameterRepository
    {

        private readonly IParameterAdditionRepository parameterAdditionRepository;
        private readonly ApplicationDbContext db;

        public DeviceParameterRepository(IParameterAdditionRepository parameterAdditionRepository, ApplicationDbContext db)
        {
            this.parameterAdditionRepository = parameterAdditionRepository;
            this.db = db;
        }

        public DeviceParameter Add(DeviceParameter deviceParameter)
        {

            DeviceParameter add = null;
            var select = db.DeviceParameters.FirstOrDefault(x => x.Id == deviceParameter.Id);
            if (select == null)
            {
                var newElement = new DeviceParameter
                {
                    Parameter = deviceParameter.Parameter,
                    Synonym = deviceParameter.Synonym,
                    Unit = db.Units.Include(x => x.Prefix).Where(x => x.Id == deviceParameter.Unit.Id).FirstOrDefault()
                };
                foreach (var item in deviceParameter.ParameterAdditions)
                {
                    newElement.ParameterAdditions.Add(db.ParameterAdditions.Where(x => x.Id == item.Id).FirstOrDefault() ?? item);
                }
                db.DeviceParameters.Add(newElement);
                db.SaveChanges();
                add = newElement;
            }
            return add;
        }

        public IEnumerable<DeviceParameter> GetList()
        {

            var check = db.DeviceParameters.Include(x => x.Unit).Include(x => x.Unit.Prefix).Include(x => x.ParameterAdditions).ToList();
            if (check != null && check.Count > 0)
                return check;
            return null;

        }

        public bool DeleteAll()
        {
            bool result = false;
            if (db.DeviceParameters.Count() != 0)
            {
                var select = db.DeviceParameters.ToList();
                foreach (var item in select)
                {
                    db.DeviceParameters.Remove(item);
                }
                db.SaveChanges();
                if (db.DeviceParameters.Count() == 0)
                    result = true;
            }
            return result;
        }

        public bool Delete(int id)
        {
            bool result = false;
            var ResultValues = db.ResultValues.Where(x => x.DeviceParameter.Id == id).ToList();

            //Удаляем результаты без доп параметра
            foreach (var item in ResultValues)
            {
                db.ResultValues.Remove(item);
                db.SaveChanges();
            }

            //Удаление дополнительных параметров
            var additions = db.ParameterAdditions.Where(x => x.DeviceParameter.Id == id).ToList();
            foreach (var item in additions)
            {
                parameterAdditionRepository.Delete(item.Id);
            }

            //Удаление параметра
            var select = db.DeviceParameters.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                db.DeviceParameters.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        public DeviceParameter Edit(int id, DeviceParameter deviceParameter)
        {
            DeviceParameter edit = null;
            var old = db.DeviceParameters.Include(x => x.Unit)
                                                .Include(x => x.Unit.Prefix)
                                                .Include(x => x.ParameterAdditions)
                                                .Where(x => x.Id == id)
                                                .FirstOrDefault();
            if (old != null)
            {
                foreach (var item in old.ParameterAdditions)
                {
                    if (deviceParameter.ParameterAdditions.All(x => x.Id != item.Id))
                        parameterAdditionRepository.Delete(item.Id);
                }
                old.Parameter = deviceParameter.Parameter;
                old.Synonym = deviceParameter.Synonym;
                old.Unit = db.Units.Where(x => x.Id == deviceParameter.Unit.Id).FirstOrDefault();
                if (deviceParameter.ParameterAdditions.Count != 0)
                {
                    foreach (var item in deviceParameter.ParameterAdditions)
                    {
                        var addItem = db.ParameterAdditions.Include(x => x.ResultValues).Where(x => x.Id == item.Id).FirstOrDefault();
                        if (addItem != null)
                        {
                            //Отлавливать неизмененные элементы, поскольку на все элементы выполняется редактирование
                            parameterAdditionRepository.Edit(addItem.Id, item);
                        }
                        else old.ParameterAdditions.Add(item);
                    }
                }
                db.SaveChanges();
                edit = old;
            }
            return edit;
        }

        public DeviceParameter Find(string ParameterName)
        {

            DeviceParameter find = null;
            var select = db.DeviceParameters.FirstOrDefault(x => x.Parameter == ParameterName);
            if (select != null)
                find = select;
            return find;
        }

        public bool AddParameterAddition(int id, ParameterAddition parameterAddition)
        {
            bool result = false;
            var select = db.DeviceParameters.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                var Additions = db.ParameterAdditions.FirstOrDefault(x => x.Name == parameterAddition.Name && x.DeviceParameter == null);
                if (Additions != null)
                {

                    select.ParameterAdditions.Add(Additions);
                    db.SaveChanges();
                    result = true;
                }
            }
            return result;
        }
        public bool AddUnit(int id, Unit unit)
        {

            bool result = false;
            var select = db.DeviceParameters.Where(x => x.Id == id).FirstOrDefault();
            var selUnit = db.Units.Where(x => x.Id == unit.Id).FirstOrDefault();
            if (select != null)
                if (selUnit != null)
                {
                    select.Unit = selUnit;
                    db.SaveChanges();
                    result = true;
                }
            return result;
        }
        public bool DeleteParameterAddition(int id, ParameterAddition parameterAddition)
        {

            bool result = false;
            var find = db.DeviceParameters.FirstOrDefault(x => x.Id == id);
            ;
            var select = db.DeviceParameters.Include(x => x.ParameterAdditions).FirstOrDefault(x => x.Parameter == find.Parameter);
            if (select != null)
            {
                var Additions = db.ParameterAdditions.FirstOrDefault(x => x.Name == parameterAddition.Name);
                if (Additions != null)
                {
                    var localResult = select.ParameterAdditions.Remove(Additions);
                    if (localResult)
                    {
                        db.SaveChanges();
                        result = true;
                    }
                }
            }
            return result;
        }
        public bool DeleteUnit(int id)
        {
            bool result = false;
            var select = db.DeviceParameters.Include(x => x.Unit).FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Unit = null;
                db.SaveChanges();
                result = true;
            }
            return result;
        }

        public Unit GetUnit(int id)
        {
            Unit unit = null;
            var select = db.DeviceParameters.Include(x => x.Unit).FirstOrDefault(x => x.Id == id);
            if (select != null)
                if (select.Unit != null)
                    unit = select.Unit;
            return unit;
        }
        public IEnumerable<ParameterAddition> GetParameterAdditionList(int id)
        {
            List<ParameterAddition> modules = null;
            var find = db.DeviceParameters.FirstOrDefault(x => x.Id == id);
            var select = db.DeviceParameters.Include(x => x.ParameterAdditions).FirstOrDefault(x => x.Parameter == find.Parameter);
            if (select != null)
            {
                var local = select.ParameterAdditions.ToList();
                if (local != null && local.Count != 0)
                    modules = local;
            }
            return modules;
        }
    }
}
