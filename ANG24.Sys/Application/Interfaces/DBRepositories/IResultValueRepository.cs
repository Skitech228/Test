using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IResultValueRepository
    {
        ResultValue Add(ResultValue resultValue);
        ResultValue Update(int id, ResultValue resultValue);
        ResultValue Find(int id);
        bool Delete(int id);
        bool AddRange(IEnumerable<ResultValue> resultValues);
        bool UpdateRange(IEnumerable<ResultValue> resultValues);
        IEnumerable<ResultValue> FindAllToOrder(int OrderId);
        bool RemoveAllToOrder(int OrderId);

    }
}
