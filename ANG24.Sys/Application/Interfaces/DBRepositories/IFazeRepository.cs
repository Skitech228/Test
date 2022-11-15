using ANG24.Sys.Domain.DBModels;

namespace ANG24.Sys.Application.Interfaces.DBRepositories
{
    public interface IFazeRepository
    {
        IEnumerable<Faze> GetList();
        Faze Find(string name);
        Faze Add(Faze faze);
        Faze Edit(int id, Faze faze);
        bool Delete(int id);

        bool SaveImage(int id, byte[] image);
        byte[] GetImage(int id);
        /*
         * дополнить следующими возможностями:
         * - Загрузка изображения
         * - Сохранение изображения
         */
    }
}
