using ANG24.Sys.Communication.Helpers;
using ANG24.Sys.Communication.Types;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{
    internal sealed class OSCOperator // Hantek 6000b
    {
        #region private prop
        private readonly object locker = new();
        private readonly ManualResetEvent mre = new(false);
        private readonly Queue<Action> queue = new();
        private ReflectMode _reflectMode;
        private readonly BackgroundWorker worker;
        private readonly System.Timers.Timer frequencyTimer;
        private readonly AutoResetEvent frequencyBreaker;

        private double oldStandardSR = 0.0;
        private int _voltDiv;
        private int _triggerLevel;
        private int _pulseLen;
        private double _K;
        private double _L;
        #endregion
        #region public prop
        public ReflectMode ReflectMode
        {
            get => _reflectMode;
            set
            {
                if (Enum.IsDefined(value))
                {
                    _reflectMode = value;
                    switch (value)
                    {
                        case ReflectMode.Impulse:
                            lock (locker)
                                ReflectometerCommands.SetREFDll();
                            break;
                        case ReflectMode.Continuous:
                            lock (locker)
                                ReflectometerCommands.SetIDMDLL(fdds, Wave, Period);
                            break;
                    }
                    TriggerLevel = 192;
                    PulseLen = PulseLen;
                }
            }
        }
        public double Lenght { get; set; }
        public int VoltDiv { get => _voltDiv; set { if (_voltDiv != value) SetVoltDiv(_voltDiv = value); } }
        public int TriggerLevel
        {
            get => _triggerLevel;
            set
            {
                if (_triggerLevel != value)
                {
                    if (value > 255) value = 255;
                    if (value < 0) value = 0;
                    SetTriggerLevel(_triggerLevel = value);
                }
            }
        }
        public int PulseLen
        {
            get => _pulseLen;
            set
            {
                var v = 0;
                switch (ReflectMode)
                {
                    case ReflectMode.Impulse:
                        _pulseLen = value;
                        v = value / 5;
                        break;
                    case ReflectMode.Continuous:
                        _pulseLen = value;
                        Task.Factory.StartNew(async () =>
                        {
                            SetFreqIDM(fdds);
                            await Task.Delay(150);
                            v = (int)m;
                            ChangePulseLen(v);
                        });
                        return;
                    default:
                        _pulseLen = value;
                        v = value / 5;
                        break;
                }
                ChangePulseLen(v);
            }
        }
        public double K
        {
            get => _K;
            set
            {
                _K = value;
                if (oldStandardSR != StandardSR)
                {
                    SetSampleRate(IndexSR);
                    oldStandardSR = StandardSR;
                }
                if (ReflectMode == ReflectMode.Continuous)
                    PulseLen = PulseLen;
            }
        }
        public double L
        {
            get => _L;
            set
            {
                _L = value;
                SetSampleRate(IndexSR);
                if (ReflectMode == ReflectMode.Continuous)
                    PulseLen = PulseLen;
            }
        }
        private double FactSR
        {
            get
            {
                if (K == 0 && L == 0)
                    return (int)SampleRate.Hz_1G;

                return 65536 / (L / (3 * Math.Pow(10, 8) / (2 * K)));
            }
        }
        private int IndexSR
        {
            get
            {
                int result = 0;
                try
                {
                    var arr = Enum.GetValues(typeof(SampleRate));
                    Array.Reverse(arr);
                    if (FactSR >= (int)arr.GetValue(0))
                        return 0;

                    for (int i = 0; i < arr.Length - 1; i++)
                        if ((int)arr.GetValue(i) > FactSR && (int)arr.GetValue(i + 1) < FactSR)
                        {
                            result = i + 1;
                            break;
                        }
                }
                catch (Exception) { }
                return result;
            }
        }
        public double StandardSR
        {
            get
            {
                var arr = Enum.GetValues(typeof(SampleRate));
                Array.Reverse(arr);
                return (int)arr.GetValue(IndexSR);
            }
        }
        public int LenMass
        {
            get
            {
                var len = (int)(L * 2 * K * StandardSR / (3 * Math.Pow(10, 8)));
                if (len == 0 || len > 65535)
                    len = 65535;
                return len;
            }
        }
        public event Action<RawOSCData> OnDataFromOscReceived;
        public int Period
        {
            get
            {
                lock (locker)
                    return ReflectometerCommands.GetPeriodDll();
            }
        }
        public int Wave
        {
            get
            {
                lock (locker)
                    return ReflectometerCommands.GetWaveDll();
            }
        }
        public bool ConnectState
        {
            get
            {
                lock (locker)
                    return ReflectometerCommands.GetDeviceVersion() != 0;
            }
        }
        #endregion
        public OSCOperator()
        {
            worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            frequencyTimer = new System.Timers.Timer(100) { AutoReset = true };
            frequencyBreaker = new AutoResetEvent(true);
            frequencyTimer.Elapsed += (p, e) =>
            { // каждые CommandsFrequency mc даёт возможность отправки данных в контроллер
                frequencyBreaker.Set();
            };

            worker.DoWork += Worker_DoWork;
            ReflectometerCommands.InitDLL(6, 9, 190, 40); // ?, int VolDiv = 9, int TriggerLevel = 190, int PulseLen = 40
            worker.RunWorkerAsync();
        }
        ~OSCOperator()
        {
            worker.CancelAsync();
            worker.DoWork -= Worker_DoWork;
            worker.Dispose();
        }
        private async void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            frequencyTimer.Start();
            while (!worker.CancellationPending)
            {
                mre.WaitOne();
                try
                {
                    await Task.Factory.StartNew(() =>
                    {
                        frequencyBreaker.WaitOne();
                        lock (locker)
                        {
                            while (queue.Count != 0)
                                queue.Dequeue()?.Invoke();
                            GetData();
                        }
                    });

                }
                catch (Exception ex)
                {
                    Debug.Write($"OSC error: {ex}");
                }
            }
            void GetData()
            {
                var result = new RawOSCData();
                var gs = 0;
                try
                {
                    IntPtr ptr1 = Marshal.AllocCoTaskMem(Marshal.SizeOf(result));                 //создаем пустой указатель с размером как у объекта ReflectData
                    Marshal.StructureToPtr(result, ptr1, true);                                   //связываемся с указателем[?]
                    switch (ReflectMode)
                    {
                        case ReflectMode.Impulse:
                            gs = ReflectometerCommands.ReadOSCDLL(ptr1);
                            break;
                        case ReflectMode.Continuous:
                            gs = ReflectometerCommands.ReadOSC3DLL(ptr1);
                            break;
                    }
                    result = (RawOSCData)Marshal.PtrToStructure(ptr1, typeof(RawOSCData));//возвращаем в указатель значения считаных данных
                    Marshal.FreeCoTaskMem(ptr1);                                                  // и освообождаем память с указателя
                }
                catch (DllNotFoundException)
                {
                    Debug.WriteLine("Dll-файл сборки не найден(Рефлектометр)");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error: {0} ({1})", ex.Message, ex.Source);
                }
                result.GS = gs;
                // не отсылать пустоту 
                if (result.IntArr[0] != 0)
                {
                    if (ReflectMode == ReflectMode.Impulse
                        && ((gs & 0x02) != 0x02 || (gs & 0x03) != 0x03))
                        return;
                    OnDataFromOscReceived?.Invoke(result);
                }
            }
        }
        public void Start() => mre.Set();
        public void Stop() => mre.Reset();
        public void ChangePulseLen(int PulseLen) => queue.Enqueue(() => ReflectometerCommands.ChangePulseLenDLL(PulseLen));
        public void SetFreqIDM(double freq) => queue.Enqueue(() => ReflectometerCommands.SetFreqIDMDll(freq));
        public void SetSampleRate(int SRIndex) => queue.Enqueue(() => ReflectometerCommands.SetSampleRateDLL(SRIndex + 6));
        public void SetTriggerLevel(int TriggerLevel) => queue.Enqueue(() => ReflectometerCommands.SetTriggerLevelDLL(TriggerLevel));
        public void SetVoltDiv(int voidDivIndex) => queue.Enqueue(() => ReflectometerCommands.SetVoltDivDLL(voidDivIndex));
        #region DDSCalculator
        //входные данные 
        private double f => StandardSR; // частота OSC 
        private double l => Lenght > 1000 ? Lenght : 1000;//длина обзора //так как при L < 1000 портится математика работы ИДМ принято решение записать L не меньше 1000 
        private double n => Wave;// pWave значение     
        private double z => Period;// pPeriod значение 
        private double dzi => PulseLen;// DZI значение        
        private double RS => n / z;
        private double t => 1 / f;
        private double TauDSO => t * p;
        private double p
        {
            get
            {
                var freqD = f;
                var len = (int)(l * 2 * K * freqD / (3 * Math.Pow(10, 8)));
                if (len == 0 || len > 65535)
                    len = 65535;
                return len;
            }
        }// количество видимых выборок

        private TauMetricValue TauMetricDSO => new(TauDSO);
        private TauMetricValue TauDDS
        {
            get
            {
                var tau = TauMetricDSO;
                tau.Value += 1;
                return tau;
            }
        }
        public double fdds => 1 / TauDDS.TranslateTo(TauUnitMetric.s);
        private double t1 => TauDDS.TranslateTo(TauUnitMetric.s) / RS;
        private double m => dzi / Math.Pow(10, 9) / t1 < 1 ? 1 : dzi / Math.Pow(10, 9) / t1;

        private enum TauUnitMetric : int
        {
            ns = 0,
            us = 3,
            ms = 6,
            s = 9
        }
        private struct TauMetricValue
        {
            public TauUnitMetric Unit;
            public double Value;
            public TauMetricValue(double val)
            {
                var counter = 0;
                var res = val * Math.Pow(10, 9);
                while (res > 1000 && counter < 9)
                {
                    res /= 1000;
                    counter += 3;
                }
                Unit = (TauUnitMetric)counter;
                Value = res;
            }
            public double TranslateTo(TauUnitMetric unit) => Value * Math.Pow(10, Unit - unit); // Unit - unit = delta
        }

        #endregion
    }
}
