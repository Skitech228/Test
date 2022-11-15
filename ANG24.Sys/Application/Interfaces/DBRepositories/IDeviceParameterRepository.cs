using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IDeviceParameterRepository
    {
        IEnumerable<DeviceParameter> GetList();
        //IQueryable<DeviceParameter> GetDeviceParameters();
        DeviceParameter Find(string ParameterName);
        DeviceParameter Add(DeviceParameter deviceParameter);
        DeviceParameter Edit(int id, DeviceParameter deviceParameter);
        bool Delete(int id);
        bool DeleteAll();

        IEnumerable<ParameterAddition> GetParameterAdditionList(int id);
        Unit GetUnit(int id);

        bool AddUnit(int id, Unit unit);
        bool AddParameterAddition(int id, ParameterAddition parameterAddition);


        bool DeleteUnit(int id);
        bool DeleteParameterAddition(int id, ParameterAddition parameterAddition);
    }
}
