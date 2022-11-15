using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class FazeMeteringResultRepository : IFazeMeteringResultRepository
    {
        private readonly ApplicationDbContext db;
        public FazeMeteringResultRepository(ApplicationDbContext db) => this.db = db;

        public FazeMeteringResult Add(FazeMeteringResult fazeMeteringResult)
        {
            FazeMeteringResult res = null;
            var newEl = new FazeMeteringResult
            {
                Order = db.Orders.Where(x => x.Id == fazeMeteringResult.Order.Id).FirstOrDefault(),
                Faze = db.Fazes.Where(x => x.Id == fazeMeteringResult.Faze.Id).FirstOrDefault(),
                TestResult = fazeMeteringResult.TestResult
            };
            res = newEl;
            db.FazeMeteringResults.Add(newEl);
            db.SaveChanges();
            return res;
        }

        public bool Delete(int id)
        {
            bool res = false;
            try
            {
                var select = db.FazeMeteringResults.Where(x => x.Id == id).FirstOrDefault();
                db.Entry(select).State = EntityState.Deleted;
                db.SaveChanges();
                res = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}<{1}> -- {2}", nameof(FazeMeteringResultRepository), nameof(Delete), ex.Message);
            }
            return res;
        }

        public FazeMeteringResult Find(int Id)
        {

            FazeMeteringResult res = null;
            res = db.FazeMeteringResults.Include(x => x.Faze)
                                        .Include(x => x.Order)
                                        .Where(x => x.Id == Id)
                                        .FirstOrDefault();
            return res;
        }

        //public FazeMeteringResult Find(FazeMeteringResult fazeMeteringResult)
        //{

        //    FazeMeteringResult res = null;
        //    res = db.FazeMeteringResults.Include(x => x.Faze)
        //                                .Include(x => x.Order)
        //                                .Where(x => x.Faze.Id == fazeMeteringResult.Faze.Id
        //                                       && x.Order.Id == fazeMeteringResult.Order.Id)
        //                                .FirstOrDefault();
        //    return res;
        //}

        public FazeMeteringResult Update(int id, FazeMeteringResult fazeMeteringResult)
        {

            FazeMeteringResult res = null;
            var select = Find(id);
            if (select != null)
            {
                try
                {
                    select.Order = db.Orders.Where(x => x.Id == fazeMeteringResult.Order.Id).FirstOrDefault();
                    select.Faze = db.Fazes.Where(x => x.Id == fazeMeteringResult.Faze.Id).FirstOrDefault();
                    select.TestResult = fazeMeteringResult.TestResult;
                    db.Entry(select).State = EntityState.Modified;
                    db.SaveChanges();
                    res = select;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}<{1}> -- {2}", nameof(FazeMeteringResultRepository), nameof(Update), ex.Message);
                }
            }
            return res;
        }
    }
}
