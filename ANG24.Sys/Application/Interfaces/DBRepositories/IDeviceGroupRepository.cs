using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IDeviceGroupRepository
    {
        IEnumerable<DeviceGroup> GetList();
        DeviceGroup Add(string Name);
        bool Delete(int id);
        DeviceGroup Edit(int id, string newName);
        DeviceGroup Find(int id);
        DeviceGroup Find(string Name);
    }
}
