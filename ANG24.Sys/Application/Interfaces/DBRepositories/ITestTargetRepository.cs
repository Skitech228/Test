using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface ITestTargetRepository
    {
        IEnumerable<TestTarget> GetList();
        TestTarget Add(string Name);
        TestTarget Edit(int id, string Name);
        TestTarget Find(int Id);
        TestTarget Find(string Name);
        bool Delete(int id);
        bool DeleteAll();
    }
}
