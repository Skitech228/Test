using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IParameterAdditionRepository
    {
        IEnumerable<ParameterAddition> GetList();
        ParameterAddition Add(ParameterAddition parameterAddition);
        ParameterAddition Edit(int id, ParameterAddition parameterAddition);
        bool Delete(int Id);
        ParameterAddition Find(int Id);
        ParameterAddition Find(string Name);
        //служебная операция
        bool DeleteAll();
    }
}
