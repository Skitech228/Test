using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IFazeMeteringResultRepository
    {
        FazeMeteringResult Add(FazeMeteringResult fazeMeteringResult);
        FazeMeteringResult Update(int id, FazeMeteringResult fazeMeteringResult);
        FazeMeteringResult Find(int Id);
        //FazeMeteringResult Find(FazeMeteringResult fazeMeteringResult);
        bool Delete(int id);

    }
}
