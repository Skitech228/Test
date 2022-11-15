using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IModuleRepository
    {
        IEnumerable<Module> GetList();
        Module Find(int id);
        Module Find(string module);
        //Реализовать
        //IEnumerable<Device> GetDevicesToModule(int id);
    }
}
