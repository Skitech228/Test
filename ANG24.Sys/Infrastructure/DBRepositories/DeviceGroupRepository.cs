using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class DeviceGroupRepository : IDeviceGroupRepository
    {
        private readonly ApplicationDbContext db;

        public DeviceGroupRepository(ApplicationDbContext db) => this.db = db;

        public DeviceGroup Add(string Name)
        {

            DeviceGroup add = null;
            var select = db.DeviceGroups.FirstOrDefault(x => x.Name == Name);
            if (select == null)
            {
                var newElement = new DeviceGroup
                {
                    Name = Name
                };
                db.DeviceGroups.Add(newElement);
                db.SaveChanges();
                add = newElement;
            }
            return add;
        }
        public DeviceGroup Edit(int id, string newName)
        {

            DeviceGroup edit = null;
            var select = db.DeviceGroups.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                select.Name = newName;
                db.SaveChanges();
                edit = select;
            }
            return edit;
        }
        public DeviceGroup Find(int id)
        {

            DeviceGroup find = null;
            var select = db.DeviceGroups.FirstOrDefault(x => x.Id == id);
            if (select != null)
                find = select;
            return find;
        }
        public DeviceGroup Find(string name)
        {

            DeviceGroup find = null;
            var select = db.DeviceGroups.FirstOrDefault(x => x.Name == name);
            if (select != null)
                find = select;
            return find;
        }
        public IEnumerable<DeviceGroup> GetList() => db.DeviceGroups.ToList();
        public bool Delete(int id)
        {

            bool result = false;
            var select = db.DeviceGroups.FirstOrDefault(x => x.Id == id);
            if (select != null)
            {
                var devices = db.Devices.Include(x => x.DeviceGroup).Where(x => x.DeviceGroup.Id == select.Id).ToList();
                if (devices != null && devices.Count != 0)
                    foreach (var item in devices)
                        item.DeviceGroup = null;
                db.DeviceGroups.Remove(select);
                db.SaveChanges();
                result = true;
            }
            return result;
        }
    }
}
