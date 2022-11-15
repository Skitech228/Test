using ANG24.Sys.Domain.DBModels;
using ANG24.Sys.Persistence;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ANG24.Sys.Infrastructure.DBRepositories
{
    public class ResultValueRepository : IResultValueRepository
    {
        private readonly ApplicationDbContext db;
        public ResultValueRepository(ApplicationDbContext db) => this.db = db;

        public ResultValue Add(ResultValue resultValue)
        {
            ResultValue res = null;
            var select = db.ResultValues.Include(x => x.Order)
                                        .Include(x => x.Device)
                                        .Include(x => x.DeviceParameter)
                                        .Include(x => x.ParameterAddition)
                                        .Include(x => x.Faze)
                                        .Where(x => x.Order.Id == resultValue.Order.Id
                                             && x.DeviceParameter.Id == resultValue.DeviceParameter.Id
                                             && x.ParameterAddition.Id == resultValue.ParameterAddition.Id
                                             && x.Faze.Id == resultValue.Faze.Id)
                                        .FirstOrDefault();
            if (select == null)
            {
                try
                {
                    db.ResultValues.Add(res = resultValue);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    res = null;
                    Debug.WriteLine("{0}<{1}> -- {2}", nameof(ResultValueRepository), nameof(Add), ex.Message);
                }
            }
            return res;
        }
        private ResultValue DirectAdd(ResultValue resultValue)
        {
            ResultValue res = null;
            resultValue.DeviceParameter = resultValue.DeviceParameter == null ? null : db.DeviceParameters.Where(x => x.Id == resultValue.DeviceParameter.Id).FirstOrDefault();
            resultValue.Device = resultValue.Device == null ? null : db.Devices.Where(x => x.Id == resultValue.Device.Id).FirstOrDefault();
            resultValue.Faze = resultValue.Faze == null ? null : db.Fazes.Where(x => x.Id == resultValue.Faze.Id).FirstOrDefault();
            resultValue.Order = resultValue.Order == null ? null : db.Orders.Where(x => x.Id == resultValue.Order.Id).FirstOrDefault();
            resultValue.ParameterAddition = resultValue.ParameterAddition == null ? null : db.ParameterAdditions.Where(x => x.Id == resultValue.ParameterAddition.Id).FirstOrDefault();
            db.ResultValues.Add(res = resultValue);
            db.SaveChanges();
            return res;
        }
        private bool DirectAddRange(IEnumerable<ResultValue> resultValues)
        {
            bool res = false;
            try
            {
                db.ResultValues.AddRange(resultValues);
                db.SaveChanges();
                res = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return res;
        }
        public bool AddRange(IEnumerable<ResultValue> resultValues)
        {
            bool res = false;
            foreach (var resultValue in resultValues)
            {
                var select = db.ResultValues.Include(x => x.Order)
                                       .Include(x => x.Device)
                                       .Include(x => x.DeviceParameter)
                                       .Include(x => x.ParameterAddition)
                                       .Include(x => x.Faze)
                                       .Where(x => x.Order.Id == resultValue.Order.Id
                                             && x.DeviceParameter.Id == resultValue.DeviceParameter.Id
                                             && x.ParameterAddition.Id == resultValue.ParameterAddition.Id
                                             && x.Faze.Id == resultValue.Faze.Id)
                                       .FirstOrDefault();
                if (select == null)
                {
                    try
                    {
                        db.ResultValues.Add(resultValue);
                    }
                    catch (Exception ex)
                    {
                        res = false;
                        Debug.WriteLine("{0}<{1}> -- {2}", nameof(ResultValueRepository), nameof(Add), ex.Message);
                        return res;
                    }
                }
            }
            try
            {
                db.SaveChanges();
                res = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}<{1}> -- {2}", nameof(ResultValueRepository), nameof(AddRange), ex.Message);
            }
            return res;
        }
        private void UpdateValue(int id, string newValue)
        {
            var resultValue = db.ResultValues.FirstOrDefault(x => x.Id == id);
            resultValue.DeviceParameter = resultValue.DeviceParameter == null ? null : db.DeviceParameters.Where(x => x.Id == resultValue.DeviceParameter.Id).FirstOrDefault();
            resultValue.Device = resultValue.Device == null ? null : db.Devices.Where(x => x.Id == resultValue.Device.Id).FirstOrDefault();
            resultValue.Faze = resultValue.Faze == null ? null : db.Fazes.Where(x => x.Id == resultValue.Faze.Id).FirstOrDefault();
            resultValue.Order = resultValue.Order == null ? null : db.Orders.Where(x => x.Id == resultValue.Order.Id).FirstOrDefault();
            resultValue.ParameterAddition = resultValue.ParameterAddition == null ? null : db.ParameterAdditions.Where(x => x.Id == resultValue.ParameterAddition.Id).FirstOrDefault();
            resultValue.Value = newValue;
            db.Entry(resultValue).State = EntityState.Modified;
            db.SaveChanges();
        }
        public bool UpdateRange(IEnumerable<ResultValue> resultValues)
        {
            bool res = false;
            try
            {
                foreach (var item in resultValues)
                {
                    var select = Find(item.Id);
                    if (select == null) DirectAdd(item);
                    else Update(select.Id, item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}<{1}> -- {2}", nameof(ResultValueRepository), nameof(UpdateRange), ex.Message);
            }
            return res;
        }
        public ResultValue Find(int id)
        {
            ResultValue res = null;

            var select = db.ResultValues.Include(x => x.Order)
                                        .Include(x => x.Device)
                                        .Include(x => x.DeviceParameter)
                                        .Include(x => x.ParameterAddition)
                                        .Include(x => x.Faze)
                                        .Where(x => x.Id == id)
                                        .FirstOrDefault();
            if (select != null) res = select;
            return res;
        }
        public IEnumerable<ResultValue> FindAllToOrder(int orderId)
        {
            return db.ResultValues.Include(x => x.Order)
                                 .Include(x => x.Device)
                                 .Include(x => x.DeviceParameter)
                                 .Include(x => x.ParameterAddition)
                                 .Include(x => x.Faze)
                                 .Where(x => x.Order.Id == orderId)
                                 .ToList();
        }
        public bool Delete(int id)
        {
            bool res = false;

            var select = db.ResultValues.Where(x => x.Id == id).FirstOrDefault();
            if (select != null)
            {
                try
                {
                    db.ResultValues.Remove(select);
                    db.SaveChanges();
                    res = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}<{1}> -- {2}", nameof(ResultValueRepository), nameof(Delete), ex.Message);
                }
            }
            return res;
        }
        public bool RemoveAllToOrder(int OrderId)
        {
            bool res = false;

            var select = FindAllToOrder(OrderId);
            db.ResultValues.RemoveRange(select);
            try
            {
                db.SaveChanges();
                res = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}<{1}> -- {2}", nameof(ResultValueRepository), nameof(RemoveAllToOrder), ex.Message);
            }
            return res;
        }
        public ResultValue Update(int id, ResultValue resultValue)
        {
            ResultValue res = null;

            var select = db.ResultValues.Include(x => x.Order)
                                 .Include(x => x.Device)
                                 .Include(x => x.DeviceParameter)
                                 .Include(x => x.ParameterAddition)
                                 .Include(x => x.Faze)
                                 .Where(x => x.Id == id)
                                 .FirstOrDefault();
            if (select != null)
            {
                select.Order = db.Orders.Include(x => x.TestObject)
                                        .Include(x => x.Devices)
                                        .Where(x => x.Id == resultValue.Order.Id)
                                        .FirstOrDefault();
                select.Device = db.Devices.Include(x => x.DeviceParameters)
                                          .Where(x => x.Id == resultValue.Device.Id)
                                          .FirstOrDefault();
                select.DeviceParameter = db.DeviceParameters.Include(x => x.ParameterAdditions)
                                                            .Where(x => x.Id == resultValue.DeviceParameter.Id)
                                                            .FirstOrDefault();
                select.ParameterAddition = db.ParameterAdditions.Where(x => x.Id == resultValue.ParameterAddition.Id).FirstOrDefault();
                select.Faze = db.Fazes.Where(x => x.Id == resultValue.Faze.Id).FirstOrDefault();
                try
                {
                    db.SaveChanges();
                    res = select;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("{0}<{1}> -- {2}", nameof(ResultValueRepository), nameof(Update), ex.Message);
                }
            }
            return res;
        }
        public bool UpdateRange(List<ResultValue> resultValues)
        {
            bool res = false;

            try
            {
                foreach (var item in resultValues)
                {
                    var sel = resultValues.FirstOrDefault(x => x.Id == item.Id);
                    if (sel != null)
                        Update(item.Id, sel);
                }
                res = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}<{1}> -- {2}", nameof(ResultValueRepository), nameof(UpdateRange), ex.Message);
            }
            return res;
        }
        public ResultValue FindById(int id) => db.ResultValues.FirstOrDefault(x => x.DeviceParameter.Id == id);
    }
}
