using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class FazeRepository : IFazeRepository
    {
        private readonly IFazeMeteringResultRepository fazeMeteringResultRepository;
        private readonly ApplicationDbContext db;

        public FazeRepository(IFazeMeteringResultRepository fazeMeteringResultRepository, ApplicationDbContext db)
        {
            this.fazeMeteringResultRepository = fazeMeteringResultRepository;
            this.db = db;
        }
        public Faze Add(Faze faze)
        {

            Faze add = null;
            var select = db.Fazes.Where(x => x.Id == faze.Id).FirstOrDefault();
            if (select == null)
            {
                Faze fazeItem = new Faze
                {
                    Name = faze.Name,
                    ConnectionImage = faze.ConnectionImage
                };
                db.Fazes.Add(add = fazeItem);
                db.SaveChanges();
            }
            else throw new ArgumentException("Указанный элемент уже существует", nameof(select));
            return add;
        }

        public bool Delete(int id)
        {

            bool res = false;
            var resultValues = db.ResultValues.Where(x => x.Faze.Id == id).ToList();
            if (resultValues?.Count != 0)
            {
                foreach (var result in resultValues)
                {
                    db.Entry(result).State = EntityState.Deleted;
                    db.SaveChanges();
                }
            }
            var faseMetering = db.FazeMeteringResults.Include(x => x.Order).Where(x => x.Faze.Id == id).ToList();
            if (faseMetering?.Count != 0)
            {
                foreach (var metering in faseMetering)
                {
                    fazeMeteringResultRepository.Delete(metering.Id);
                }
            }
            var select = db.Fazes.Where(x => x.Id == id).FirstOrDefault();
            if (select != null)
            {
                db.Fazes.Remove(select);
                db.SaveChanges();
                res = true;
            }
            else throw new ArgumentNullException(nameof(select), "Не удалось найти указанный элемент");
            return res;
        }

        public Faze Edit(int id, Faze faze)
        {
            Faze edit = null;
            var select = db.Fazes.Where(x => x.Id == id).FirstOrDefault();
            if (select != null)
            {
                select.Name = faze.Name;
                select.ConnectionImage = faze.ConnectionImage;
                db.SaveChanges();
                edit = select;
            }
            else throw new ArgumentNullException(nameof(select), "Не удалось найти указанный элемент");
            return edit;
        }

        public IEnumerable<Faze> GetList()
        {

            List<Faze> list = new List<Faze>();
            var select = db.Fazes.Include(x => x.TestObject).ToList();
            if (select.Count != 0)
                foreach (var item in select) list.Add(item);
            return list;
        }

        public byte[] GetImage(int id)
        {

            byte[] image = null;
            var select = db.Fazes.Where(x => x.Id == id).FirstOrDefault();
            if (select != null)
            {
                image = select.ConnectionImage;
            }
            else throw new ArgumentNullException(nameof(select), "Не удалось найти указанный элемент");
            return image;
        }

        public bool SaveImage(int id, byte[] image)
        {

            bool res = false;
            var select = db.Fazes.Where(x => x.Id == id).FirstOrDefault();
            if (select != null)
            {
                select.ConnectionImage = image;
                db.SaveChanges();
            }
            else throw new ArgumentNullException(nameof(select), "Не удалось найти указанный элемент");
            return res;
        }
        public Faze Find(string name) => db.Fazes.FirstOrDefault(x => x.Name == name);
    }
}
