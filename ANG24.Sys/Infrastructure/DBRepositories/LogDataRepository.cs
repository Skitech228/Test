using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class LogDataRepository : ILogDataRepository
    {
        private readonly ApplicationDbContext db;

        public LogDataRepository(ApplicationDbContext db) => this.db = db;

        public LogData Add(LogData log)
        {
            db.LogDatas.Add(log);
            db.SaveChanges();
            return log;
        }

        public bool AddRange(IEnumerable<LogData> logs)
        {
            db.LogDatas.AddRange(logs);
            db.SaveChanges();
            return true;
        }

        public IEnumerable<LogData> GetList(object filter)
        {
            List<LogData> res = new();
            var fil = filter as LogFilter;
            if (fil.Session != null)
            {
                res = db.LogDatas.Include(x => x.Session).Include(x => x.Session.User).Where(x => x.Session.Id == fil.Session.Id).ToList();
            }
            else
            {
                if (fil.LeftDate != null && fil.RightDate != null)
                    res = db.LogDatas.Include(x => x.Session).Where(x => x.CreateDate >= fil.LeftDate).Where(x => x.CreateDate < fil.RightDate).ToList();
                else if (fil.LeftDate == null && fil.RightDate != null)
                    res = db.LogDatas.Include(x => x.Session).Where(x => x.CreateDate <= fil.RightDate).ToList();
                else if (fil.LeftDate != null && fil.RightDate == null)
                    res = db.LogDatas.Include(x => x.Session).Where(x => x.CreateDate <= fil.LeftDate).ToList();
            }
            //mock
            return res;
        }
    }
    public class LogFilter
    {
        public Session Session { get; set; }
        public DateTime? LeftDate { get; set; }
        public DateTime? RightDate { get; set; }
    }
}
