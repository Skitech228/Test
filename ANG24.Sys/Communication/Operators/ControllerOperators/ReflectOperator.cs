using ANG24.Sys.Communication.Helpers;
using ANG24.Sys.Communication.Operators.AbstractOperators;
using ANG24.Sys.Communication.Operators.ControllerOperators;
using ANG24.Sys.Communication.Types;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums.Reflect;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{
    public sealed class ReflectOperator : StringSerialControllerOperator<ReflectData>, IReflectOperator
    {
        public override event Action<ReflectData> OnDataReceived;
        public event Action<ReflectOSCData> OnDataFromOscReceived;
        #region fields
        private OSCOperator osc;
        private readonly ReflectDataPrepare reflectDataPrepare;
        private CancellationTokenSource tokenSource = new();
        private readonly AutoResetEvent channelWaiter = new(false);
        private readonly ILabConfigurationService config;
        private Task ChannelSwitcher;
        private Channel _currentChannel;
        private Mode _currentMode;
        private Amplitude _currentAmplitude;
        private Resistance _currentResistance;
        private int _IDMPulse;
        private int _IDMDelay;
        private bool isIDMObserve;
        #endregion
        #region Props        
        public bool IsIDMObserve { get; set; }
        public int IDMPulse
        {
            get => _IDMPulse;
            set
            {
                if (_IDMPulse != value)
                {
                    WriteCommand("P", value, x =>
                    {
                        if (x)
                        {
                            //PulseChanged?.Invoke(value);
                            _IDMPulse = value;
                        }
                    });

                }
            }
        }
        public int IDMDelay
        {
            get => _IDMDelay;
            set
            {
                if (_IDMDelay != value)
                {
                    WriteCommand("D", value, x =>
                    {
                        if (x)
                        {
                            //DelayChanged?.Invoke(value);
                            _IDMDelay = value;
                        }
                    });
                }
            }
        }
        public List<Channel> Channels { get; set; } = new List<Channel>();
        public Channel CurrentChannel
        {
            get => _currentChannel;
            private set
            {
                if (Enum.IsDefined(value))
                    WriteCommand("C",
                        (int)value,
                        success =>
                        {
                            reflectDataPrepare.CurrentChannel = value;
                            _currentChannel = value;
                            channelWaiter.Set();
                        });
            }
        }
        public Mode CurrentMode
        {
            get => _currentMode;
            set
            {
                if (Enum.IsDefined(value))
                    WriteCommand("M", (int)value, success =>
                    {
                        if (success)
                        {
                            _currentMode = value;
                            if (value != Mode.Impulse)
                                ReflectMode = ReflectMode.Continuous;
                            else
                                ReflectMode = ReflectMode.Impulse;
                            switch (value)
                            {
                                case Mode.Impulse:
                                    if (!isIDMObserve)
                                        AddChannel(1);
                                    VoltDiv = 9;
                                    break;
                                case Mode.IDM:
                                    AddChannel(4);
                                    CurrentResistance = Resistance.Ohm_600;
                                    VoltDiv = 4;
                                    break;
                                case Mode.Decay:
                                    AddChannel(5);
                                    VoltDiv = 4;
                                    break;
                                case Mode.ICE:
                                    AddChannel(6);
                                    VoltDiv = 4;
                                    break;
                            }
                        }
                    });
            }
        }
        public Amplitude CurrentAmplitude
        {
            get => _currentAmplitude;
            set
            {
                if (Enum.IsDefined(value))
                    WriteCommand("V", (int)value, success =>
                    {
                        if (success)
                            _currentAmplitude = value;
                    });
            }

        }
        public Resistance CurrentResistance
        {
            get => _currentResistance;
            set
            {
                if (Enum.IsDefined(value) && _currentResistance != value)
                {
                    WriteCommand("R", (int)value, success =>
                    {
                        if (success)
                            _currentResistance = value;
                    });
                }
            }
        }
        public ReflectMode ReflectMode { get => osc.ReflectMode; set => osc.ReflectMode = value; }
        #region OSC
        public int VoltDiv { get => osc.VoltDiv; set => osc.VoltDiv = value; }
        public int TriggerLevel { get => osc.TriggerLevel; set => osc.TriggerLevel = value; }
        public int PulseLen { get => osc.PulseLen; set => osc.PulseLen = value; }
        public double K
        {
            get => osc.K; set
            {
                osc.K = value;
                reflectDataPrepare.Length = (int)(L * 10 * (K / 1.5));
            }
        }
        public double L
        {
            get => osc.L;
            set
            {
                osc.L = value;
                reflectDataPrepare.Length = (int)value;
                reflectDataPrepare.LenMas = osc.LenMass;
            }
        }
        public int GetPeriod() => osc?.Period ?? 0;
        public int GetWave() => osc?.Wave ?? 0;
        public void Start() => osc?.Start();
        public void Stop() => osc?.Stop();
        #endregion
        #endregion
        public ReflectOperator(ILabConfigurationService config)
        {
            Name = Application.Types.Enum.LabController.Reflect.ToString();
            reflectDataPrepare = new ReflectDataPrepare
            {
                Channels = Channels // ссылка
            };
            this.config = config;
        }
        public override bool Connect()
        {
            if (!base.Connect()) return false;
            // установка параметров работы с контроллером рефлектометра после подключения
            CommandAwaitInterval = 500;
            CommandsFrequency = 100;

            // запуск осциллографа в случае удачного коннекта к контроллеру АРМ
            osc = new OSCOperator();
            // Установка стартовыхпараметров осцилографа 
            ReflectMode = ReflectMode.Impulse;
            CurrentMode = Mode.Impulse;
            CurrentAmplitude = Amplitude.V30;
            CurrentResistance = Resistance.Ohm_50;

            K = 1.87;
            L = 500;
            VoltDiv = 9;
            PulseLen = 100;
            AddChannel(6);
            TurnChannelSwitcher(true);
            osc.OnDataFromOscReceived += OSCDataReciever;
            return true;
        }
        public override void EmergencyStop()
        {
            base.EmergencyStop();
            osc.Stop();
        }
        public void AddChannel(int channel)
        {
            if (!Enum.IsDefined(typeof(Channel), channel)) return;
            var realChannel = (Channel)channel;
            //if (true)
            //{
            //проверка на наличие в списке
            if (Channels.Contains(realChannel)) return;

            if (channel == 4 || channel == 5 || channel == 6) // если одиночный канал → удалить все остальные
                Channels.Clear();
            else     // в противном случае удалить только 4\5\6 канал
                Channels.RemoveAll(x => (int)x == 4 || (int)x == 5 || (int)x == 6);

            Channels.Add(realChannel);
            //}
            //else
            //    if (!Channels.Contains(realChannel))
            //    Channels.Add(realChannel);
            channelWaiter.Set();
        }
        public void RemoveChannel(int channel)
        {
            var ch = Channels.FirstOrDefault(x => x.Equals(channel));
            if (Channels.Contains((Channel)channel))
                Channels.Remove((Channel)channel);

            if (Channels.Count == 0)
                Channels.Add((Channel)1);
            //TurnChannelSwitcher(false); // поменять местами если всё таки потребуется отсутствие каналов              
        }
        public void CompareChannels(bool isCompare, int c1, int c2)
        {
            if (!Enum.IsDefined(typeof(Channel), c1) || !Enum.IsDefined(typeof(Channel), c1)) return;
            reflectDataPrepare.ComparedChannel1 = (Channel)c1;
            reflectDataPrepare.ComparedChannel2 = (Channel)c2;
            reflectDataPrepare.IsCompare = isCompare;
        }
        public void TurnSmoother() => reflectDataPrepare.TurnSmoother();

        public void ShiftReflectGraphOffset(int offset) => reflectDataPrepare.XOffset = offset;
        #region Helpers
        // Специцический формат отправки данных к контроллеру рефлектометра
        private void OSCDataReciever(RawOSCData data)
        {
            // ассинхронно, что бы не мешать работе осцилографа 
            Task.Factory.StartNew(() =>
            {
                var reflectData = reflectDataPrepare.GetChannelsData(data.IntArr, out bool isDataUpdated);
                if (isDataUpdated)
                    OnDataFromOscReceived?.Invoke(new ReflectOSCData
                    {
                        ReflectData = reflectData,
                        Gs = data.GS, // хз зачем клиенту GS, лучше убарть при не необходимости
                        Lehght = osc.LenMass, // тоже хз зачем, т.к. это число - Х последнего элемента массива любого из каналов
                        Channel = CurrentChannel // та же проблема, все каналы доступны в ReflectData
                    });
            });
        }
        private void TurnChannelSwitcher(bool mode)
        {
            if (ChannelSwitcher?.Status == TaskStatus.Running)
            {
                tokenSource.Cancel();
                channelWaiter.Set();
                Thread.Sleep(300); // на всякий случай
            }
            if (mode) // если true, то запустить свитчер, в противном случае только остановить предыдущий
            {
                tokenSource = new CancellationTokenSource();
                var token = tokenSource.Token;
                ChannelSwitcher = Task.Factory.StartNew(() =>
                {
                    while (!token.IsCancellationRequested)
                        try
                        {
                            if (Channels.Count > 1)
                                foreach (var channel in Channels)
                                {
                                    if (token.IsCancellationRequested) break;
                                    if (channel != CurrentChannel)
                                    {
                                        CurrentChannel = channel;
                                        channelWaiter.WaitOne();
                                    }
                                }
                            else
                            {
                                if (Channels.Count != 0 && CurrentChannel != Channels[0])
                                    CurrentChannel = Channels[0];
                                channelWaiter.WaitOne();
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine($"Channel Switcher exception: {e} {e.Message}");
                        }
                },
                token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Current);
            }
        }
        protected override void CommandBroker(ReflectData data)
        {
            if ((int)CurrentMode == 2)
            {
                if (data.Message.Contains("ArcSend")) reflectDataPrepare.Mode2Stage = 1;
                if (data.Message.Contains("BaseSend")) reflectDataPrepare.Mode2Stage = 2;
            }

            OnDataReceived?.Invoke(data);
        }
        private void WriteCommand(string TypeCommand, int value, Action<bool> action) => base.SetCommand($"{TypeCommand}{value:D3}#", action);
        #endregion
    }
}
