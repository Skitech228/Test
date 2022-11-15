using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;
using ANG24.Sys.Application.Types.Enum;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{
    public sealed class GP500ControllerOperator : IGP500ControllerOperator
    {
        private BackgroundWorker backgroundWorker;
        private readonly BlockingCollection<Task> queue = new();
        private ControllerState controllerState = ControllerState.NotConnection;
        private KP500ControllerValues kp500ControllerValuesSave = new();
        private int errorCount = 0;
        public string Name => LabController.GP500.ToString();
        public event Action<KP500ControllerValues> OnKP500DataReceived;
        public event Action<GP500Data> OnDataReceived;
        public event Action<IControllerOperatorData> OnData;

        public GP500ControllerOperator()
        {
            // что бы предупреждения убрать
            OnDataReceived?.Invoke(null);
            OnDataReceived += (data) => OnData?.Invoke(data);
        }
        public void Start()
        {
            try
            {
                if (backgroundWorker == null)
                {
                    backgroundWorker = new BackgroundWorker
                    {
                        WorkerReportsProgress = true,
                        WorkerSupportsCancellation = true
                    };
                    backgroundWorker.DoWork += WorkProcess;
                    backgroundWorker.RunWorkerAsync();
                }
                else
                {
                    backgroundWorker.RunWorkerAsync();
                }

            }
            catch (Exception)
            {
            }
        }
        public void Stop()
        {
            // Изменение статуса контроллера на остановку
            controllerState = ControllerState.Stoping;
            // Остановка выполения очереди
            if (backgroundWorker != null && backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
            }
            // Очистка очереди
            while (queue.Count != 0)
            {
                queue.Take();
            }
            //Сброс напряжения
            Task task = new Task(() =>
            {
                KP500ControllerData.Instance().ButtonReset();
            });
            task.Start();
            task.Wait();
            // Закрытие порта
            KP500ControllerData.Instance().Close();
            // Изменение статуса
            controllerState = ControllerState.NotConnection;
        }
        public ControllerState GetState()
        {
            return controllerState;
        }
        private void WorkProcess(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            while (!worker.CancellationPending)
            {
                if (controllerState == ControllerState.NotConnection || controllerState == ControllerState.ConnectionError)
                {
                    controllerState = ControllerState.Initialization;
                    if (KP500ControllerData.Instance().InitAuto())
                    {
                        controllerState = ControllerState.Connected;
                    }
                    else
                    {
                        controllerState = ControllerState.ConnectionError;
                        KP500ControllerValues kp500ControllerValues = new KP500ControllerValues
                        {
                            Ktr = 0.25,
                            State = 4,
                            StateMode = 0,
                            SelectScreen = 0,
                            Voltage = 0,
                            PercentVoltage = 0,
                            Current = 0,
                            PercentCurrent = 0,
                            FrequencyState = -1,
                            MatchingState = -1,
                            ModeImpState = -1,
                            ResistanceState = -1,
                            Freq1 = -1,
                            Freq2 = -1,
                            Freq3 = -1
                        };
                        OnKP500DataReceived?.Invoke(kp500ControllerValues);
                        //Thread.Sleep(2000);
                    }
                }

                if (controllerState == ControllerState.LossConnection)
                {
                    var buffer = KP500ControllerData.Instance().GetCurrentInfo();
                    if (buffer.state == true && buffer.value[1] == 1)
                    {
                        controllerState = ControllerState.Connected;
                    }
                    else
                    {
                        KP500ControllerValues kp500ControllerValues = new KP500ControllerValues
                        {
                            Ktr = 0.25,
                            State = 3,
                            StateMode = 0,
                            SelectScreen = 0,
                            Voltage = 0,
                            PercentVoltage = 0,
                            Current = 0,
                            PercentCurrent = 0,
                            FrequencyState = -1,
                            MatchingState = -1,
                            ModeImpState = -1,
                            ResistanceState = -1,
                            Freq1 = -1,
                            Freq2 = -1,
                            Freq3 = -1
                        };
                        OnKP500DataReceived?.Invoke(kp500ControllerValues);
                    }
                    //Thread.Sleep(2000);
                }

                if (controllerState == ControllerState.Connected)
                {
                    Task taskCurrentInfo = new Task(() => CurrentInfo());
                    queue.Add(taskCurrentInfo);
                    while (queue.Count > 0)
                    {
                        var selectTask = queue.FirstOrDefault();
                        if (selectTask != null)
                        {
                            Thread.Sleep(40);
                            selectTask.Start();
                            selectTask.Wait();
                            queue.TryTake(out selectTask);
                        }
                    }
                }
                //Thread.Sleep(250);
            }
            e.Cancel = true;
            return;
        }
        private void CurrentInfo()
        {
            var check1 = false;
            var check2 = false;

            KP500ControllerValues kp500ControllerValues = new KP500ControllerValues();

            var buffer = KP500ControllerData.Instance().GetCurrentInfo();
            if (buffer.state == true && buffer.value[1] == 1)
            {
                check1 = true;
                kp500ControllerValues = new KP500ControllerValues
                {
                    Ktr = 0.25,
                    State = 1,
                    SelectScreen = buffer.value[2]
                };
                #region Состояние
                switch (buffer.value[2])
                {
                    case 11:
                        kp500ControllerValues.State = 1;
                        break;
                    case 10:
                    case 26:
                    case 27:
                    case 28:
                    case 42:
                    case 43:
                    case 44:
                    case 58:
                    case 59:
                    case 60:
                    case 74:
                    case 75:
                    case 76:
                    case 90:
                    case 91:
                    case 92:
                        kp500ControllerValues.State = 5;
                        break;
                        //default:
                        //    kp500ControllerValues.StateMode = 0;
                        //    break;
                }
                #endregion

                #region Режим работы импульсов 
                var ModeImp = buffer.value[11];
                switch (ModeImp)
                {
                    case 0:
                        kp500ControllerValues.ModeImp = "Непрерывно";
                        kp500ControllerValues.ModeImpState = 0;
                        break;
                    case 1:
                        kp500ControllerValues.ModeImp = "Импульсно";
                        kp500ControllerValues.ModeImpState = 1;
                        break;
                    case 2:
                        kp500ControllerValues.ModeImp = "МЧ2";
                        kp500ControllerValues.ModeImpState = 2;
                        break;
                    case 3:
                        kp500ControllerValues.ModeImp = "МЧ3";
                        kp500ControllerValues.ModeImpState = 3;
                        break;
                }
                #endregion

                #region Частота
                //var frequency = buffer.value[3] & 3;
                var frequency = buffer.value[3] & 7;
                switch (frequency)
                {
                    case 0:
                        kp500ControllerValues.Frequency = "480 Hz";
                        kp500ControllerValues.FrequencyState = 0;
                        break;
                    case 1:
                        kp500ControllerValues.Frequency = "1069 Hz";
                        kp500ControllerValues.FrequencyState = 1;
                        break;
                    case 2:
                        kp500ControllerValues.Frequency = "9796 Hz";
                        kp500ControllerValues.FrequencyState = 2;
                        break;

                    case 3:
                        kp500ControllerValues.Frequency = "Change freq1";
                        kp500ControllerValues.FrequencyState = 3;
                        break;
                    case 4:
                        kp500ControllerValues.Frequency = "Change freq2";
                        kp500ControllerValues.FrequencyState = 4;
                        break;
                    case 5:
                        kp500ControllerValues.Frequency = "Change freq3";
                        kp500ControllerValues.FrequencyState = 5;
                        break;
                }
                #endregion

                #region Согласование
                var matching = buffer.value[3] & 8;
                switch (matching)
                {
                    case 0:
                        kp500ControllerValues.Matching = "Фикс.";
                        kp500ControllerValues.MatchingState = 0;
                        break;
                    case 8:
                        kp500ControllerValues.Matching = "Авто";
                        kp500ControllerValues.MatchingState = 1;
                        break;
                }
                #endregion

                #region Сопротивление
                var resistance = buffer.value[3] & 240;
                switch (resistance)
                {
                    case 0:
                        kp500ControllerValues.Resistance = "0.5 Ohm";
                        kp500ControllerValues.ResistanceValue = 0.5;
                        kp500ControllerValues.Ktr = 0.25;
                        kp500ControllerValues.ResistanceState = 0;
                        break;
                    case 16:
                        kp500ControllerValues.Resistance = "1 Ohm";
                        kp500ControllerValues.ResistanceValue = 1;
                        kp500ControllerValues.Ktr = 0.354;
                        kp500ControllerValues.ResistanceState = 1;
                        break;
                    case 32:
                        kp500ControllerValues.Resistance = "2 Ohm";
                        kp500ControllerValues.ResistanceValue = 2;
                        kp500ControllerValues.Ktr = 0.5;
                        kp500ControllerValues.ResistanceState = 2;
                        break;
                    case 48:
                        kp500ControllerValues.Resistance = "4 Ohm";
                        kp500ControllerValues.ResistanceValue = 4;
                        kp500ControllerValues.Ktr = 0.707;
                        kp500ControllerValues.ResistanceState = 3;
                        break;
                    case 64:
                        kp500ControllerValues.Resistance = "8 Ohm";
                        kp500ControllerValues.ResistanceValue = 8;
                        kp500ControllerValues.Ktr = 1.0;
                        kp500ControllerValues.ResistanceState = 4;
                        break;
                    case 80:
                        kp500ControllerValues.Resistance = "16 Ohm";
                        kp500ControllerValues.ResistanceValue = 16;
                        kp500ControllerValues.Ktr = 1.41;
                        kp500ControllerValues.ResistanceState = 5;
                        break;
                    case 96:
                        kp500ControllerValues.Resistance = "32 Ohm";
                        kp500ControllerValues.ResistanceValue = 32;
                        kp500ControllerValues.Ktr = 2.0;
                        kp500ControllerValues.ResistanceState = 6;
                        break;
                    case 112:
                        kp500ControllerValues.Resistance = "64 Ohm";
                        kp500ControllerValues.ResistanceValue = 64;
                        kp500ControllerValues.Ktr = 2.83;
                        kp500ControllerValues.ResistanceState = 7;
                        break;
                    case 128:
                        kp500ControllerValues.Resistance = "128 Ohm";
                        kp500ControllerValues.ResistanceValue = 128;
                        kp500ControllerValues.Ktr = 4;
                        kp500ControllerValues.ResistanceState = 8;
                        break;
                    case 144:
                        kp500ControllerValues.Resistance = "256 Ohm";
                        kp500ControllerValues.ResistanceValue = 256;
                        kp500ControllerValues.Ktr = 5.66;
                        kp500ControllerValues.ResistanceState = 9;
                        break;
                }
                #endregion

                #region Перегрузка
                var overload = buffer.value[4] & 28;
                switch (overload)
                {
                    case 4:
                        kp500ControllerValues.Overload = "Перегрузка по U";
                        kp500ControllerValues.OverloadState = 1;
                        break;
                    case 8:
                        kp500ControllerValues.Overload = "Перегрузка по I";
                        kp500ControllerValues.OverloadState = 2;
                        break;
                    case 12:
                        kp500ControllerValues.Overload = "Перегрузка по P";
                        kp500ControllerValues.OverloadState = 3;
                        break;
                    case 16:
                        kp500ControllerValues.Overload = "Перегрузка по T";
                        kp500ControllerValues.OverloadState = 4;
                        break;
                    case 20:
                        kp500ControllerValues.Overload = "Перегрузка по T";
                        kp500ControllerValues.OverloadState = 4;
                        break;
                    default:
                        kp500ControllerValues.Overload = "Перегрузки нет";
                        kp500ControllerValues.OverloadState = 0;
                        break;
                }
                #endregion

                #region Буффер 
                kp500ControllerValues.Buffer =
                    buffer.value[0].ToString() + ";" +
                    buffer.value[1].ToString() + ";" +
                    buffer.value[2].ToString() + ";" +
                    buffer.value[3].ToString() + ";" +
                    buffer.value[4].ToString() + ";" +
                    buffer.value[5].ToString() + ";" +
                    buffer.value[6].ToString() + ";" +
                    buffer.value[7].ToString() + ";" +
                    buffer.value[8].ToString() + ";" +
                    buffer.value[9].ToString() + ";" +
                    buffer.value[10].ToString() + ";" +
                    buffer.value[11].ToString() + ";" +
                    buffer.value[12].ToString() + ";" +
                    buffer.value[13].ToString() + ";" +
                    buffer.value[14].ToString() + ";" +
                    buffer.value[15].ToString() + ";" +
                    buffer.value[16].ToString();
                #endregion

                // Напряжение
                kp500ControllerValues.Voltage = Math.Round((buffer.value[5] * 256 + buffer.value[6]) * kp500ControllerValues.Ktr / 10, 2);
                // Ток
                kp500ControllerValues.Current = Math.Round((buffer.value[7] * 256 + buffer.value[8]) / (100 * kp500ControllerValues.Ktr), 2);
                // Напряжение в процентах
                kp500ControllerValues.PercentVoltage = Math.Round(kp500ControllerValues.Voltage * 100 / Math.Sqrt(kp500ControllerValues.ResistanceValue * 512), 1);
                // Ток в процентах
                kp500ControllerValues.PercentCurrent = Math.Round(kp500ControllerValues.Current * 100 / Math.Sqrt(512 / kp500ControllerValues.ResistanceValue), 2);
            }

            var buffer1 = KP500ControllerData.Instance().GetInfo05();
            if (buffer1.state == true)
            {
                var seniorByte = buffer1.value[2];
                var t = seniorByte << 8;
                var juniorByte = buffer1.value[3];
                var FreqCurrent = t + juniorByte;
                kp500ControllerValues.FreqCurrent = FreqCurrent;

                var seniorByte1 = buffer1.value[4];
                var t1 = seniorByte1 << 8;
                var juniorByte1 = buffer1.value[5];
                var Freq1 = t1 + juniorByte1;
                kp500ControllerValues.Freq1 = Freq1;

                var seniorByte2 = buffer1.value[6];
                var t2 = seniorByte2 << 8;
                var juniorByte2 = buffer1.value[7];
                var Freq2 = t2 + juniorByte2;
                kp500ControllerValues.Freq2 = Freq2;

                var seniorByte3 = buffer1.value[8];
                var t3 = seniorByte3 << 8;
                var juniorByte3 = buffer1.value[9];
                var Freq3 = t3 + juniorByte3;
                kp500ControllerValues.Freq3 = Freq3;
                check2 = true;
            }

            if (check1 == true && check2 == true)
            {
                if (controllerState != ControllerState.Connected) controllerState = ControllerState.Connected;
                errorCount = 0;
                OnKP500DataReceived?.Invoke(kp500ControllerValues);
                kp500ControllerValuesSave = kp500ControllerValues;
            }
            else
            {
                if (errorCount > 5)
                {
                    if (controllerState != ControllerState.LossConnection) controllerState = ControllerState.LossConnection;
                    kp500ControllerValues = new KP500ControllerValues
                    {
                        Ktr = 0.25,
                        State = 3,
                        StateMode = 0,
                        SelectScreen = 0,
                        Voltage = 0,
                        PercentVoltage = 0,
                        Current = 0,
                        PercentCurrent = 0,
                        FrequencyState = -1,
                        MatchingState = -1,
                        ModeImpState = -1,
                        ResistanceState = -1,
                        Freq1 = -1,
                        Freq2 = -1,
                        Freq3 = -1
                    };
                    OnKP500DataReceived?.Invoke(kp500ControllerValues);
                }
                errorCount++;
            }
        }
        public void SetChangeMode()
        {
            KP500ControllerValues kp500ControllerValues = kp500ControllerValuesSave;
            kp500ControllerValues.State = 5;
            OnKP500DataReceived?.Invoke(kp500ControllerValues);
        }
        public void DownButtonPlus()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().DownButtonPlus();
                });
                queue.Add(task);
            }
        }
        public void UpButtonPlus()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().UpButtonPlus();
                });
                queue.Add(task);
            }
        }
        public void DownButtonMinus()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().DownButtonMinus();
                });
                queue.Add(task);
            }
        }
        public void UpButtonMinus()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().UpButtonMinus();
                });
                queue.Add(task);
            }
        }
        public void ButtonReset()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().ButtonReset();
                });
                queue.Add(task);
            }
        }
        public void ButtonMode()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().ButtonMode();
                });
                queue.Add(task);
            }
        }
        public void ButtonMatching()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().ButtonMatching();
                });
                queue.Add(task);
            }
        }
        public void SetManualFrequency()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().SetManualFrequency();
                });
                queue.Add(task);
            }
        }
        public void SaveFrequency()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().SaveFrequency();
                });
                queue.Add(task);
            }
        }
        public void SetFrequency(KP500FrequencyEnum frequency)
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    SetChangeMode();
                    KP500ControllerData.Instance().SetFrequency(frequency);
                });
                queue.Add(task);
            }
        }
        public void SetResistance(KP500Resistance resist)
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    SetChangeMode();
                    KP500ControllerData.Instance().SetResistance(resist);
                });
                queue.Add(task);
            }
        }
        public void SetModeImp(KP500ModeImp modeImp)
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    SetChangeMode();
                    KP500ControllerData.Instance().SetModeImp(modeImp);
                });
                queue.Add(task);
            }
        }
        public void SetModeMCH2(KP500SetMCH2 setMCH2)
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    SetChangeMode();
                    KP500ControllerData.Instance().SetModeMCH2(setMCH2);
                });
                queue.Add(task);
            }
        }
        public void OpenSetFrequency()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    SetChangeMode();
                    if (!KP500ControllerData.Instance().OpenSetFrequency())
                    {
                        KP500ControllerValues kp500ControllerValues = kp500ControllerValuesSave;
                        kp500ControllerValues.State = 6;
                        kp500ControllerValues.MessageError = "Ошибка переключения в режим изменения частоты. ГП-500К перезагружен.";
                        OnKP500DataReceived?.Invoke(kp500ControllerValues);

                        ButtonResetAll();
                    }
                });
                queue.Add(task);
            }
        }
        public void SetMainMenu()
        {
            if (controllerState == ControllerState.Connected)
            {
                Task task = new Task(() =>
                {
                    KP500ControllerData.Instance().SetMainMenu();
                });
                queue.Add(task);
            }
        }
        public void ButtonResetAll()
        {
            KP500ControllerData.Instance().ButtonResetAll();
        }


        public bool Connect()
        {
            throw new NotImplementedException();
        }
        public bool Connect(int AttemptCount)
        {
            throw new NotImplementedException();
        }
        public void Disconnect()
        {
            throw new NotImplementedException();
        }
        public void StartQueue()
        {
            throw new NotImplementedException();
        }
        public void StopQueue()
        {
            throw new NotImplementedException();
        }
        public void EmergencyStop()
        {
            throw new NotImplementedException();
        }

        public bool Connected => throw new NotImplementedException();
        public IMethodExecuter CurrentProcess => throw new NotImplementedException();
    }
}

