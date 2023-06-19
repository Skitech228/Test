#region Using derectives

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SysTest.Win.Database;
using SysTest.Win.Database.Entity;

#endregion

namespace SysTest.Win.EntityService
{
    public class PortService : IPortService
    {
        #region Implementation of IService<Port>

        private readonly ApplicationContext _context;

        public PortService(ApplicationContext context) => _context = context;

        public async Task<List<string>> GetAllPortsAsync() =>
                await _context.Ports.Select(x => x.HostName).ToListAsync();
        public async Task<List<Port>> GetAllTCPPortsAsync() =>
                await _context.Ports.Where(x => x.Protocol=="TCP").ToListAsync();
        public async Task<List<Port>> GetAllBTPortsAsync() =>
                await _context.Ports.Where(x => x.Protocol=="BT").ToListAsync();
        public async Task<List<Port>> GetAllConnectedPortsAsync() =>
                await _context.Ports.Where(x => x.IsConnected==true).ToListAsync();
        public async Task<bool> AddAsync(Port port)
        {
            await _context.Ports.AddAsync(port);

            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(Port port)
        {
            _context.Ports.Remove(port);

            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(Port port)
        {
            _context.Ports.Attach(port);
            _context.Entry(port).State = EntityState.Modified;

            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc />
        public async Task<Port> GetByIdAsync(int id) => await _context.Ports.FindAsync(id);

        /// <inheritdoc />
        public async Task<IEnumerable<Port>> GetAllAsync() => await _context.Ports.ToListAsync();

        #endregion
    }
}