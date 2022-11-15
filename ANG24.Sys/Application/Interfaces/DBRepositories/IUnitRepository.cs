using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IUnitRepository
    {
        IEnumerable<Unit> GetList();
        Unit Find(string Name);
        Unit Add(Unit unit);
        Unit Edit(int id, Unit unit);
        bool Delete(int id);
        bool DeleteAll();
        Prefix GetPrefix(int unitId);
        bool AddPrefix(int unitId, Prefix prefix);
        bool DeletePrefix(int unitId);
    }
}
