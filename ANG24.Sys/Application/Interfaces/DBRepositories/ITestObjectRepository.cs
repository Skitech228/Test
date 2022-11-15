using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface ITestObjectRepository
    {
        IEnumerable<TestObject> GetList();
        TestObject Add(TestObject testObject);
        TestObject Edit(int id, TestObject testObject);
        bool Delete(int id);
        TestObject Find(string Mark);
        //служебная операция
        bool DeleteAll();
    }
}
