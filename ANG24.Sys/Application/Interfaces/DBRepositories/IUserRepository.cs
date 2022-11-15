using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{

    public interface IUserRepository
    {
        IEnumerable<User> List();
        User Add(User user);
        User Edit(string id, User newUser);
        User Find(string id);
        bool Delete(string id);
    }
}
