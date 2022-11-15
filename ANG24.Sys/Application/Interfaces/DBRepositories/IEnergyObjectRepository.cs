using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IEnergyObjectRepository
    {
        IEnumerable<EnergyObject> GetList();
        EnergyObject Add(string Name);
        EnergyObject Edit(int id, string Name);
        bool Delete(int id);
        EnergyObject Find(string Name);
        EnergyObject Find(int Id);
        //служебная операция
        bool DeleteAll();
    }
}
