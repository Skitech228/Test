using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IModuleStageRepository
    {
        ModuleStage Add(ModuleStage moduleStage);
        ModuleStage Find(int id);
        bool Delete(int id);
    }
}
