using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class PrefixRepository : IPrefixRepository
    {
        private readonly ApplicationDbContext db;
        public PrefixRepository(ApplicationDbContext db) => this.db = db;
        public IEnumerable<Prefix> GetList() => db.Prefixes;
    }
}
