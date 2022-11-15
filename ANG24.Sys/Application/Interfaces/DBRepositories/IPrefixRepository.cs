using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IPrefixRepository
    {
        IEnumerable<Prefix> GetList();
    }
}
