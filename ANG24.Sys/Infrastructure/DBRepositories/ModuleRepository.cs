using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly ApplicationDbContext db;
        public ModuleRepository(ApplicationDbContext db) => this.db = db;
        //public IEnumerable<Device> GetDevicesToModule(string module) => db.Devices.ToList();
        public IEnumerable<Module> GetList() => db.Modules.ToList();
        public Module Find(int id) => db.Modules.FirstOrDefault(x => x.Id == id);
        public Module Find(string module) => db.Modules.FirstOrDefault(x => x.Name == module);
    }
}
