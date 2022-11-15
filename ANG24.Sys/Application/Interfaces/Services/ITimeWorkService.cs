using ANG24.Sys.Application.Types.ServiceTypes;

namespace ANG24.Sys.Application.Interfaces.Services;
public interface ITimeWorkService : IExecuteableCreator
{
    IEnumerable<Module> SynchronizationTime();
    int TimeUpdateSynchronization();
}
