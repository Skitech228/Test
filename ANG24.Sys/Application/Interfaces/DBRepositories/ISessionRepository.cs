using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{

    public interface ISessionRepository
    {
        Session Add(Session session);
        Session Find(int id);
        bool Delete(int id);
    }

}
