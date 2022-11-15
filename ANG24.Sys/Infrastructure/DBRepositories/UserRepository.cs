using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Infrastructure.DBRepositories
{

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext db;
        public UserRepository(ApplicationDbContext db) => this.db = db;
        public User Add(User user)
        {
            User add = null;
            var select = db.Users.FirstOrDefault(x => x.Name == user.Name && x.SecondName == user.SecondName && x.ThirdName == user.ThirdName);
            if (select == null)
            {
                var newEl = new User
                {
                    Name = user.Name,
                    SecondName = user.SecondName,
                    ThirdName = user.ThirdName,
                    Position = user.Position,
                    CreateDate = DateTime.Now
                };
                db.Users.Add(newEl);
                db.SaveChanges();
                add = newEl;
            }
            return add;
        }
        public User Edit(string id, User newUser)
        {
            User edit = null;
            var select = db.Users.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Name = newUser.Name;
                select.SecondName = newUser.SecondName;
                select.ThirdName = newUser.ThirdName;
                select.Position = newUser.Position;
                db.Entry(select).State = EntityState.Modified;
                db.SaveChanges();
                edit = select;
            }
            return select;
        }
        public User Find(string id) => db.Users
                           .Include(x => x.Sessions)
                           .Include(x => x.ModuleStages).FirstOrDefault(x => x.Id == id);
        public IEnumerable<User> List() => db.Users.Include(x => x.ModuleStages).Include(x => x.Sessions).ToList();
        public bool Delete(string id)
        {
            var select = db.Users
                           .Include(x => x.Sessions)
                           .Include(x => x.ModuleStages)
                           .Where(x => x.Id == id)
                           .FirstOrDefault();
            if (select != null)
            {
                db.Users.Remove(select);
                db.Entry(select).State = EntityState.Deleted;
                db.SaveChanges();
                return true;
            }
            return false;
        }
    }

}
