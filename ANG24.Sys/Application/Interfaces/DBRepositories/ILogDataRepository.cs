using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface ILogDataRepository
    {
        LogData Add(LogData log);
        bool AddRange(IEnumerable<LogData> logs);
        // BUG: переделать, с API не прокатит
        IEnumerable<LogData> GetList(object filter);
    }
}
