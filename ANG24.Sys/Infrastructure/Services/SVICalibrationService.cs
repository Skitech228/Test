using ANG24.Sys.Infrastructure.Helpers;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;
using ANG24.Sys.Application.Types.ServiceTypes;
using System.Text;
using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;

namespace ANG24.Sys.Infrastructure.Services;
public sealed class SVICalibrationService : Executer, ISVICalibrationService
{
    #region Fields
    private readonly ISVIOperator oper;
    private LabModule currentModule;
    private LabModule CurrentModule
    {
        get => currentModule;
        set
        {
            if (currentModule != value && value != LabModule.NoMode)
            {
                currentModule = value;
                Points = value switch
                {
                    LabModule.HVMAC => PointsAC,
                    LabModule.HVMDC => PointsDC,
                    _ => null
                };
            }
        }
    }
    public event Action<IEnumerable<Point>> OnPointsChanged;
    private readonly AutoResetEvent GotResponse = new(false);

    public int SelectedPointIndex = -1;
    private bool pointArrive;
    private bool SettingNewPoints;
    public string CurrentDate { get; set; }
    public string Rev { get; set; }
    public int SerialNumber { get; set; }
    public int ZeroPoint { get; set; }

    private int VoltagePoint { get; set; }
    public (int AC, int DC) RealPointsCount { get; set; }
    private readonly List<Point> PointsAC = new();
    private readonly List<Point> PointsDC = new();
    private List<Point> Points = new(); // current
    #endregion
    public SVICalibrationService(ISVIOperator oper, Autofac.ILifetimeScope container) : base(container)
    {
        oper.OnDataReceived += OnDataReceived;
        this.oper = oper;
        Init();
    }

    #region Query
    public async void GetRealPoints()
    {
        // пересчет всех точек калибровки
        // есть пара багов, но работает +/- стабильно
        // лучше не трогать без крайней нужды
        pointArrive = false;
        if (SettingNewPoints || Points is null) return;
        SettingNewPoints = true;
        Points.Clear();
        await Task.Factory.StartNew(() =>
        {
            var count = CurrentModule == LabModule.HVMAC ? RealPointsCount.AC : RealPointsCount.DC;
            for (int i = 0; i < count; i++)
            {
                var PointInd = i;
                Task.Factory.StartNew(() =>
                {
                    var tryCount = 5;
                    pointArrive = false;
                    while (!pointArrive)
                    {
                        if (CurrentModule == LabModule.HVMDC) oper.GetKoefDCCalib(SerialNumber, PointInd);
                        else oper.GetKoefACCalib(SerialNumber, PointInd);
                        Thread.Sleep(350);
                        tryCount--;
                        if (tryCount < 1) break;
                    }
                }).Wait();
            }
            SettingNewPoints = false;
        });
    }
    public void EditPoint(int index, int voltage)
    {
        if (index == -1 || Points is null) return;
        if (Points.Count == 0 || index == 0 && Points[0].Y == 0) return; // {1} проверка на наличие элементов
        var x = index;
        var point = new Point { X = x, Y = voltage };
        Points[x] = point;
        if (CurrentModule == LabModule.HVMDC) oper.EditPointDCCalib(SerialNumber, x, voltage);
        else oper.EditPointACCalib(SerialNumber, x, voltage);
    }
    public void SetSerial(int serial) => oper.SetSerialNumCalib(serial);
    public (int DC, int AC) GetPointsCount()
    {
        oper.GetPointCountCalib(SerialNumber);
        GotResponse.WaitOne(1000);
        return RealPointsCount;
    }
    public IEnumerable<Point> GetPoints() => Points;
    public void AddPoint(int x, int voltage)
    {
        if (Points is null) return;
        if (VoltagePoint > 250000 || VoltagePoint < 0) return;
        VoltagePoint = voltage;
        if (CurrentModule == LabModule.HVMDC) oper.AddPointDCCalib(SerialNumber, VoltagePoint);
        else oper.AddPointACCalib(SerialNumber, VoltagePoint);
    }
    public void DeletePoint(int index)
    {
        if (index == -1 || Points is null) return;
        SelectedPointIndex = index;
        if (CurrentModule == LabModule.HVMDC) oper.RemovePointDCCalib(SerialNumber, index);
        else oper.RemovePointACCalib(SerialNumber, index);
    }
    public void DeleteAllPoints()
    {
        oper.DeleteAllPointsCalib(SerialNumber);
        oper.SetDateCalib(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
    }
    public void SetZeroPoint(int pos) => oper.SetLevelZeroPointCalib(Convert.ToInt32(pos));
    public string GetCalibDate()
    {
        oper.GetDateCalib();
        GotResponse.WaitOne(1000);
        return CurrentDate;
    }
    public int GetSerial()
    {
        oper.GetSerialNumCalib();
        GotResponse.WaitOne(1000);
        return SerialNumber;
    }
    public int GetZeroPoint()
    {
        oper.GetLevelZeroPointCalib();
        GotResponse.WaitOne(1000);
        return ZeroPoint;
    }
    #endregion
    #region private
    private void Init()
    {
        Task.Factory.StartNew(async () =>
        {
            int tryCount = 5;
            do// Получения серийного номера
            {
                oper.GetSerialNumCalib();
                await Task.Delay(300);
                tryCount--;
                if (tryCount < 1) break;
            } while (SerialNumber == 0);
            oper.GetPointCountCalib(SerialNumber); // Попытка получения колличества точек
            await Task.Delay(100);
            tryCount = 5;
            var currentDate = DateTime.Now;
            do// Установка даты калибровки
            {
                oper.SetDateCalib(currentDate.Year, currentDate.Month, currentDate.Day);
                await Task.Delay(300);
                tryCount--;
                if (tryCount < 1) break;
            } while (CurrentDate == null);

            tryCount = 5;

            do// Установка ревизии калибратора
            {
                oper.GetFirmwareRev();
                await Task.Delay(200);
                tryCount--;
                if (tryCount < 1) break;
            } while (Rev == null);

            oper.GetLevelZeroPointCalib();
        });
    }
    private async void OnDataReceived(MEAData data)
    {
        if (data.Module != LabModule.NoMode)
            CurrentModule = data.Module;
        if (data.Message != "")
        {
            switch (data.Message.Split(':', '=')[0])
            {
                case "Serial": //Пришел серийник
                    if (CurrentModule == LabModule.Main) CurrentModule = data.Module;
                    SerialNumber = int.Parse(data.Message.Replace("Serial=", ""));
                    GotResponse.Set();
                    break;

                case "Calib": //Пришла дата 
                    StringBuilder d = new(data.Message.Replace("Calib=", ""));
                    d.Insert(6, ".");
                    d.Insert(4, ".");
                    CurrentDate = d.ToString();
                    GotResponse.Set();
                    break;

                case "REV": //Пришла ревизия
                    Rev = data.Message.Split('=')[1];
                    break;

                case "ZeroPoint": //Пришла нулевая точка
                    ZeroPoint = int.Parse(data.Message.Replace("ZeroPoint=", "")) - 2048;
                    GotResponse.Set();
                    break;

                case "VOLPTRS": //Пришло кол-во точек {1} DC  {2} AC
                    var ss = data.Message.Split(',');
                    if (CurrentModule == LabModule.HVMDC && int.Parse(ss[1]) < Points.Count  //Для модуля HVMDC удалять данные только если удалили точку DC
                     || CurrentModule == LabModule.HVMAC && int.Parse(ss[2].Replace(";", "")) < Points.Count)
                    {
                        Points?.RemoveAt(SelectedPointIndex);
                        OnPointsChanged?.Invoke(Points);
                    }
                    RealPointsCount = (int.Parse(ss[2].Replace(";", "")), int.Parse(ss[1]));
                    GotResponse.Set();
                    break;

                case "VOLCPACDC": //Оповещение об очистке списка точек
                    Points.Clear();
                    var sss = data.Message.Split(',');
                    RealPointsCount = (int.Parse(sss[2].Replace(";", "")), int.Parse(sss[1]));
                    OnPointsChanged?.Invoke(Points);
                    break;

                case "VOLCPDC": //Пришла добавленная DC точка
                    if (CurrentModule == LabModule.HVMDC) await Add();
                    break;

                case "VOLCPAC": //Пришла добавленная AC точка
                    if (CurrentModule == LabModule.HVMAC) await Add();
                    break;

                case "DCPTR": //Пришла обновленная DC точка со значением напряжения
                    if (CurrentModule == LabModule.HVMDC) UpdateWithKoef(data.Message);
                    break;

                case "ACPTR": //Пришла обновленная AC точка со значением напряжения
                    if (CurrentModule == LabModule.HVMAC) UpdateWithKoef(data.Message);
                    break;
            }
        }
    }
    private async Task ConvertPoints(int tryCount = 3)
    {
        // *работает хреново из за проблем со связью
        // если кто-то захочет разобраться почему и как это исправить... жалаю ему удачи
        if (Points is null) return;
        var points = Points.OrderBy(x => x.Y).ToArray();
        Points.Clear();
        foreach (var point in points)
            Points.Add(point);

        bool allUpdated = true;
        do
        {
            if (Points.Count == 0) break;

            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].X > 16) continue; // BUG: если напряжение больше 16в, точка будет пропущена

                if (CurrentModule == LabModule.HVMDC)
                    oper.GetKoefDCCalib(SerialNumber, i);
                else
                    oper.GetKoefACCalib(SerialNumber, i);

                await Task.Delay(200);
            }
            await Task.Delay(500);
            for (int i = 0; i < Points.Count; i++)
                if (Points[i].X < 10)
                {
                    allUpdated = false;
                    break;
                }
        } while (!allUpdated && --tryCount != 0);
        OnPointsChanged?.Invoke(Points);
    }
    private async Task Add()
    {
        if (Points is null) return;
        var x = Points.Count;
        Points.Add(new Point { X = x, Y = VoltagePoint });
        VoltagePoint = 0;
        if (Rev == "3")
            await ConvertPoints();
        else
            OnPointsChanged?.Invoke(Points);
        await Task.Delay(300);
    }
    private void UpdateWithKoef(string model)
    {
        //запрос данных точки (rev 3)
        if (Points is null) return;
        pointArrive = true;
        var s = model.Split(','); // {0} Serial, {1} x {2} voltage {3} koef
        var point = new Point { X = int.Parse(s[3].Replace(";", "")), Y = int.Parse(s[2]) };
        if (!SettingNewPoints)
        {
            if (Points.Count == 0) return;
            var pos = int.Parse(s[1]);
            if (Points[pos].Y == point.Y)
                Points[pos] = point;
            else
            {
                Points.Remove(Points.FirstOrDefault(p => p.Y == point.Y));
                Points.Insert(pos, point);
            }
        }
        else
        {
            if (Points.Contains(point)) return;
            Points.Add(point);
        }
        OnPointsChanged?.Invoke(Points);
    }
    #endregion
}