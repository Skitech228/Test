using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly IDeviceParameterRepository deviceParameterRepository;
        private readonly ApplicationDbContext db;


        public DeviceRepository(IDeviceParameterRepository deviceParameterRepository, ApplicationDbContext db)
        {
            this.deviceParameterRepository = deviceParameterRepository;
            this.db = db;
        }
        public IEnumerable<Device> GetList()
        {
            return db.Devices.Include(x => x.DeviceGroup)
                                  .Include(x => x.DeviceParameters)
                                  .Include(x => x.Modules)
                                  .ToList();
        }
        public Device Add(Device device)
        {

            Device add = null;

            var selectSynonym = db.Devices.FirstOrDefault(x => x.Synonym == device.Synonym);
            if (selectSynonym == null)
            {
                var select = db.Devices.FirstOrDefault(x => x.Name == device.Name);
                if (select == null)
                {
                    var newElement = new Device
                    {
                        Name = device.Name,
                        Synonym = device.Synonym,
                        Zav_N = device.Zav_N,
                        VerificationDate = device.VerificationDate,
                        NextVerificationDate = device.NextVerificationDate,
                        IsInner = device.IsInner,
                        DeviceGroup = db.DeviceGroups.Where(x => x.Id == device.DeviceGroup.Id).FirstOrDefault()
                    };
                    foreach (var item in device.DeviceParameters)
                    {
                        var DeviceParameter = deviceParameterRepository.Add(item);
                        newElement.DeviceParameters.Add(db.DeviceParameters.Where(x => x.Id == DeviceParameter.Id).FirstOrDefault());
                    }
                    db.Devices.Add(newElement);
                    db.SaveChanges();
                    add = newElement;
                }
            }
            return add;
        }
        public bool AddDeviceGroup(int deviceId, DeviceGroup deviceGroup)
        {

            bool result = false;
            var select = db.Devices.FirstOrDefault(x => x.Id == deviceId);
            var selDeviceGroup = db.DeviceGroups.FirstOrDefault(x => x.Name == deviceGroup.Name);
            if (select != null)
            {
                if (selDeviceGroup != null)
                {
                    select.DeviceGroup = selDeviceGroup;
                    db.SaveChanges();
                    result = true;
                }
            }
            return result;
        }
        public bool AddDeviceParameter(int deviceId, DeviceParameter deviceParameter)
        {

            bool result = false;
            var select = db.Devices.FirstOrDefault(x => x.Id == deviceId);
            if (select != null)
            {
                var parameters = db.DeviceParameters.FirstOrDefault(x => x.Parameter == deviceParameter.Parameter);
                if (parameters != null)
                    if (!select.DeviceParameters.ToList().Contains(parameters))
                    {
                        select.DeviceParameters.Add(parameters);
                        db.SaveChanges();
                        result = true;
                    }
            }
            return result;
        }
        public bool AddModuleToDevice(int deviceId, Module module)
        {

            bool result = false;
            var select = db.Devices.FirstOrDefault(x => x.Id == deviceId);
            if (select != null)
            {
                var modules = db.Modules.FirstOrDefault(x => x.Name == module.Name);
                if (modules != null)
                    if (!select.Modules.ToList().Contains(modules))
                        select.Modules.Add(modules);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public bool DeleteAll()
        {
            bool result = false;
            if (db.Devices.Count() != 0)
            {
                var select = db.Devices.ToList();
                foreach (var item in select)
                    db.Devices.Remove(item);
                db.SaveChanges();
                if (db.Devices.Count() == 0)
                    result = true;
            }
            return result;
        }
        public bool Delete(int id)
        {
            bool result = false;

            var deviceParameters = db.DeviceParameters.Where(x => x.Device.Id == id).ToList();
            foreach (var item in deviceParameters)
                deviceParameterRepository.Delete(item.Id);
            // Получаем данные после удаления
            var select = db.Devices.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                db.Devices.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public Device Edit(int deviceId, Device device)
        {
            Device edit = null;
            var select = db.Devices.Include(x => x.DeviceGroup)
                                  .Include(x => x.DeviceParameters)
                                  .Where(x => x.Id == deviceId)
                                  .FirstOrDefault();
            if (select != null)
            {
                foreach (var item in select.DeviceParameters)
                {
                    if (device.DeviceParameters.All(x => x.Id != item.Id))
                        deviceParameterRepository.Delete(item.Id);
                }
                select.Name = device.Name;
                select.Synonym = device.Synonym;
                select.Zav_N = device.Zav_N;
                select.VerificationDate = device.VerificationDate;
                select.NextVerificationDate = device.NextVerificationDate;
                select.DeviceGroup = db.DeviceGroups.Where(x => x.Id == device.DeviceGroup.Id).FirstOrDefault();
                if (device.DeviceParameters.Count != 0)
                {
                    foreach (var item in device.DeviceParameters)
                    {
                        var param = db.DeviceParameters.Include(x => x.Unit).Include(x => x.ParameterAdditions).Where(x => x.Id == item.Id).FirstOrDefault() ?? null;
                        if (param == null)
                        {
                            var newDeviceParameter = deviceParameterRepository.Add(item);
                            select.DeviceParameters.Add(db.DeviceParameters.Where(x => x.Id == newDeviceParameter.Id).FirstOrDefault());
                        }
                        else
                        {
                            if (param.ParameterAdditions.Count == 0 && item.ParameterAdditions.Count != 0)
                            {
                                var result = db.ResultValues.Where(x => x.DeviceParameter.Id == param.Id).FirstOrDefault();
                                if (result != null)
                                {
                                    db.ResultValues.Remove(result);
                                    db.SaveChanges();
                                }
                            }

                            deviceParameterRepository.Edit(param.Id, item);
                        }
                    }
                    select.IsInner = device.IsInner;
                    db.SaveChanges();
                    edit = select;
                }
            }
            return edit;
        }
        public Device Find(int id)
        {
            return db.Devices.Include(x => x.DeviceGroup)
                             .Include(x => x.DeviceParameters)
                             .FirstOrDefault(x => x.Id == id);
        }
        public Device Find(string Name, int Zav_N) => db.Devices.FirstOrDefault(x => x.Name == Name && x.Zav_N == Zav_N);
        public Device Find(string Name) => db.Devices.FirstOrDefault(x => x.Name == Name);
        public Device Find(string Name, bool isInner) => db.Devices.FirstOrDefault(x => x.Name == Name && x.IsInner == isInner);
        public Device FindBySynonym(string Symonym) => db.Devices.FirstOrDefault(x => x.Synonym == Symonym);
        public DeviceGroup GetDeviceGroupToDevice(int deviceId) => db.Devices.Include(x => x.DeviceGroup).FirstOrDefault(x => x.Id == deviceId)?.DeviceGroup;
        public IEnumerable<DeviceParameter> GetDeviceParameters(int deviceId)
        {
            List<DeviceParameter> parameters = null;
            var select = db.Devices.Include(x => x.DeviceParameters).FirstOrDefault(x => x.Id == deviceId);
            if (select != null)
            {
                var local = select.DeviceParameters.ToList();
                if (local != null && local.Count != 0)
                    parameters = local;
            }
            return parameters;
        }
        public IEnumerable<Module> GetModulesToDevice(int deviceId)
        {
            List<Module> modules = null;
            var select = db.Devices.Include(x => x.Modules).FirstOrDefault(x => x.Id == deviceId);
            if (select != null)
            {
                var local = select.Modules.ToList();
                if (local != null && local.Count != 0)
                    modules = local;
            }
            return modules;
        }
        public bool RemoveDeviceGroup(int deviceId)
        {

            bool result = false;

            var select = db.Devices.Include(x => x.DeviceGroup).FirstOrDefault(x => x.Id == deviceId);
            if (select != null)
            {
                select.DeviceGroup = null;
                db.SaveChanges();
                result = true;
            }
            return result;
        }
        public bool RemoveDeviceParameter(int deviceId, int deviceParameterId)
        {

            bool result = false;
            var select = db.Devices.Include(x => x.DeviceParameters).FirstOrDefault(x => x.Id == deviceId);
            var deviceParameter = select.DeviceParameters.FirstOrDefault(x => x.Id == deviceParameterId);
            if (select != null)
            {
                var parameters = db.DeviceParameters.FirstOrDefault(x => x.Parameter == deviceParameter.Parameter);
                if (parameters != null)
                {
                    var localResult = select.DeviceParameters.Remove(parameters);
                    if (localResult)
                    {
                        db.SaveChanges();
                        result = true;
                    }
                }
            }
            return result;
        }
        public bool RemoveModuleToDevice(int deviceId, int moduleId)
        {

            bool result = false;
            var select = db.Devices.Include(nameof(Device.Modules)).FirstOrDefault(x => x.Id == deviceId);
            var module = select.Modules.FirstOrDefault(x => x.Id == moduleId);
            if (select != null)
            {
                var modules = db.Modules.FirstOrDefault(x => x.Name == module.Name);
                if (modules != null)
                {
                    var localResult = select.Modules.Remove(modules);
                    if (localResult)
                    {
                        db.SaveChanges();
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
