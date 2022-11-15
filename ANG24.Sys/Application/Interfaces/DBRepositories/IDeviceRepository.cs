using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IDeviceRepository
    {
        IEnumerable<Device> GetList();
        Device Find(int id);
        Device Find(string Name);
        Device Find(string Name, int Zav_N);
        Device Find(string Name, bool isInner);
        Device Add(Device device);
        Device Edit(int id, Device device);
        bool Delete(int id);
        bool DeleteAll();
        Device FindBySynonym(string Symonym);

        IEnumerable<Module> GetModulesToDevice(int id);
        bool AddModuleToDevice(int id, Module module);
        bool RemoveModuleToDevice(int id, int moduleId);

        DeviceGroup GetDeviceGroupToDevice(int id);
        bool AddDeviceGroup(int id, DeviceGroup deviceGroup);
        bool RemoveDeviceGroup(int id);

        IEnumerable<DeviceParameter> GetDeviceParameters(int id);
        bool AddDeviceParameter(int id, DeviceParameter deviceParameter);
        bool RemoveDeviceParameter(int id, int deviceParameterId);
    }
}
