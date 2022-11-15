using ANG24.Sys.Communication.Operators.AbstractOperators;
using ANG24.Sys.Application.Interfaces.Services;
using ANG24.Sys.Communication.Connections;
using ANG24.Sys.Communication.Interfaces;
using System.Runtime.CompilerServices;
using System.Text;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{
    public sealed class SA540ControllerOperator : ModBusControllerOperator<SA540Data>, ISA540ControllerOperator
    {
        private readonly INotificationService message;
        private ISerialPortController serialController;

        //public event Action<string, Action> OnNextFaseReceived;
        public event Action SendResult;
        public event Action Clear;
        public event Action<bool> OnStateChanged;
        public event Action<string> MeasStopped;
        public override event Action<SA540Data> OnDataReceived;

        public const int DEVICEID = 1;

        private SA540Data Data;
        private bool CheckVoltage = true;

        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        private CancellationToken token;
        private Task currentTask;
        private readonly AutoResetEvent TestEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent InstallVoltage = new AutoResetEvent(false);

        public string Mode { get; set; }

        public SA540ControllerOperator(INotificationService message) : base()
        {
            Name = Application.Types.Enum.LabController.SA540.ToString();
            this.message = message;
        }

        protected override void Port_BytesReceived(byte[] response)
        {
            var tempCommand = currentCommand;
            Data = new SA540Data();
            Data.ParseByteData(response);
            //if (tempCommand != null)
            //{
            //    if (byte.TryParse(tempCommand.CommandResponse, out int b) && b == Data.Command)
            //        Controller.ComandDone();
            //    else if (Data.Command == Convert.ToByte(0)) // Ответ при возникновении ошибки 
            //    {
            //        Controller.ComandDone();
            //        EmergencyStop();
            //        Debug.WriteLine("Ошибка от контроллера");
            //    }
            //}
            if (Data.Command == 0) // Ответ при возникновении ошибки 
            {
                EmergencyStop();
                Debug.WriteLine("Ошибка от контроллера");
            }
            CommandBroker(Data);
        }
        protected override void CommandBroker(SA540Data data)
        {
            if (data.ErrorCode != 0)
            {
                EmergencyStop();
                for (int i = 0; i < 3; i++)
                {
                    Stop_IEN();
                    Stop_Meas();
                }
            }
            switch (data.ErrorCode)
            {
                case 1:
                    message.SendNotificationOK("Перегрузка! Подано большое напряжение", null);
                    break;
                case 2:
                    message.SendNotificationOK("Перегрузка! Подан большой ток", null);
                    break;
                case 3:
                    message.SendNotificationOK("Нет синхронизации", null);
                    break;
                case 4:
                    message.SendNotificationOK("Подключен кабель КИ(КТ)! Отключите кабель КИ(КТ)", null);
                    break;
                case 5:
                    message.SendNotificationOK("Отключен кабель КИ(КТ)! Подключите кабель КИ(КТ)", null);
                    break;
                case 6:
                    message.SendNotificationOK("Проверьте схему измерительной цепи", null);
                    break;
                case 7:
                    message.SendNotificationOK("Превышен ток встронного источника", null);
                    break;
                default:
                    break;
            }
        }
        public void ConnectToMeas()
        {
            serialController.UnsubscirbePortNotifications();
            var response = ApplyCommand($"{DEVICEID} 255 5 0");
            if (response.Length != 0) Port_BytesReceived(response);
            else
            {
                EmergencyStop();
                message.SendNotificationOK("Нет связи с мостом СА540", null);
            }
            //SetCommand($"{DEVICEID} 255 5 0", null, "255") ;
        }
        public override void EmergencyStop()
        {
            tokenSource.Cancel();
            TestEvent.Set();
            InstallVoltage.Reset();
            OnStateChanged?.Invoke(false);
        }
        public void ClearMeas() => Clear?.Invoke();
        private void Stop_Meas()
        {
            StringBuilder str = new StringBuilder($"{DEVICEID} 162 5 0");
            Controller.WriteCommandPriority(new ControllerCommand()
            { Message = AddCS(str) });//0x01 0xA2 5 CS
        }
        public void StopSC_KT()
        {
            EmergencyStop();
            Stop_Meas();
            Stop_Meas();
            Stop_Meas();
            SendResult?.Invoke();
            OnStateChanged?.Invoke(false);
        }
        public void StopIEN()
        {
            EmergencyStop();
            Stop_IEN();
            SendResult?.Invoke();
            OnStateChanged?.Invoke(false);
        }
        public void InstallVoltage_IE(int TFases, int Source, int Triang, int StarN, int Star, int Fase, double Volt)
        {
            CheckVoltage = true;
            StartNewTask(() =>
            {
                //OnStateChanged?.Invoke(false);
                SetU_IE(TFases, Source, Triang, StarN, Star, Fase, Volt);
                while (CheckVoltage)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_IE((s) => Data.CurrentFase = Fase);
                    OnDataReceived?.Invoke(Data);
                }
            });
        }
        public void Meas_IE(int TFases, int Source, int Triang, int StarN, int Star, int Fase, double Volt, int N, double[] Pz_XX)
        {
            if (Source == 1) Internal_Test_IE(TFases, Source, Triang, StarN, Star, Fase, Volt, N, Pz_XX);
            else External_Test_IE(TFases, Fase, Volt, N, Pz_XX);
        }
        public void InstallVoltage_SC(int TFases)
        {
            CheckVoltage = true;
            StartNewTask(() =>
            {
                //OnStateChanged?.Invoke(false);
                SA540DataKZ.Instal_Calc();
                SetU_SC(TFases);
                while (CheckVoltage)
                {
                    if (TFases == 1)
                    {
                        for (int numberFase = 0; numberFase < 3; numberFase++)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest_SC(numberFase, (s) => Data.CurrentFase = numberFase);
                            OnDataReceived?.Invoke(Data);
                        }
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        SingleTest_SC(0, (s) => Data.CurrentFase = 0);
                        OnDataReceived?.Invoke(Data);
                    }
                }
            });
        }
        public void Meas_SC(int TFases, int Order, int N, double[] Zb_KZ)
        {
            if (TFases == 0) AutoTesting1_SC(N, Zb_KZ);
            else AutoTesting3_SC(Order, N, Zb_KZ);
        }
        public void InstallVoltage_KT(int TFases, int Source, double Volt)
        {
            CheckVoltage = true;
            StartNewTask(() =>
            {
                //OnStateChanged?.Invoke(false);
                SA540DataKT.Install_Calc();
                SetU_KT(TFases, Source, Volt);
                while (CheckVoltage)
                {
                    if (TFases == 1)
                    {
                        for (int numberFase = 0; numberFase < 3; numberFase++)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest_KT(numberFase, (s) => Data.CurrentFase = numberFase);
                            OnDataReceived?.Invoke(Data);
                        }
                    }
                    else
                    {
                        token.ThrowIfCancellationRequested();
                        SingleTest_KT(0, (s) => Data.CurrentFase = 0);
                        OnDataReceived?.Invoke(Data);
                    }
                }
            });
        }
        public void Meas_KT(int TFases, int Source, int Order, double Volt, int N, double Kz_KT)
        {
            if (Source == 1)
            {
                if (TFases == 0)
                    Internal_Test_KT(TFases, 1, Source, Order, Volt, N, Kz_KT);
                else
                    Internal_Test_KT(TFases, 3, Source, Order, Volt, N, Kz_KT);
            }
            else
            {
                if (TFases == 0)
                    AutoTesting1_KT(N, Kz_KT);
                else
                    AutoTesting3_KT(Order, N, Kz_KT);
            }
        }
        public void InstalVoltage_IEN(int TFases)
        {
            CheckVoltage = true;
            StartNewTask(() =>
            {
                //OnStateChanged?.Invoke(false);
                SA540DataXXN.InstallCalc();
                double uf = 0;
                CheckCable_KT();
                SetU_IEN();
                while (CheckVoltage)
                {
                    if (TFases == 1)
                    {
                        for (int numberFase = 0; numberFase < 3; numberFase++)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest_IEN((byte)(numberFase + 3), numberFase, (s) =>
                            {
                                Data.CurrentFase = numberFase;
                                uf = (float)Data.U;
                            });
                            token.ThrowIfCancellationRequested();
                            SingleTest_IEN(numberFase, numberFase, (s) =>
                            {
                                Data.CurrentFase = numberFase;
                                Data.Umf = Data.U;
                                Data.Uf = uf;
                            });
                            OnDataReceived?.Invoke(Data);
                        }
                    }

                    {
                        token.ThrowIfCancellationRequested();
                        SingleTest_IEN(3, 0, (s) =>
                        {
                            Data.CurrentFase = 0;
                            Data.Uf = Data.U;
                        });
                        OnDataReceived?.Invoke(Data);
                    }
                }
            });

        }
        public void Meas_IEN(int TFases, int Order, int N, double Volt)
        {
            CheckVoltage = false;
            EmergencyStop();
            if (TFases == 0) AutoTesting1_IEN(N, Volt);
            else AutoTesting3_IEN(Order, N, Volt);
        }

        #region IE
        private void SetU_IE(int TFases, int Source, int Triang, int StarN, int Star, int Fase, double Volt)
        {
            var str = new StringBuilder($"{DEVICEID} 1 15 0 {TFases} {Source} {Triang} {StarN} {Star} {Fase}");
            foreach (var item in BitConverter.GetBytes(Volt))
                str.Append(" " + item);
            var response = ApplyCommand(str.ToString());
            if (response.Length != 0) Port_BytesReceived(response);
            else EmergencyStop();
        }//0x01 0x01 Size TFases Source Triang StarN Star Fase Volt CS
        private void SingleTest_IE(Action<bool> action = null)
        {
            var response = ApplyCommand($"{DEVICEID} 160 5 0");
            if (response.Length != 0)
            {
                Port_BytesReceived(response);
                action?.Invoke(true);
            }
            else EmergencyStop();
        }
        private void NextTap_IE()
        {
            var response = ApplyCommand($"{DEVICEID} 161 5 0");
            if (response.Length != 0) Port_BytesReceived(response);
            else EmergencyStop();
        }//0x01 0xA1 5 CS
        private void Internal_Test_IE(int TFases, int Source, int Triang, int StarN, int Star, int Fase, double Volt, int n, double[] Pz_XX)
        {
            StartNewTask(() =>
            {
                SA540DataXX.InstallCalc();
                SetU_IE(TFases, Source, Triang, StarN, Star, Fase, Volt);

                for (var number = 0; number < n; number++)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_IE((s) => Calc_IE(number, n, Fase, Volt, Pz_XX));
                    SendResult.Invoke();
                }
                if (TFases == 1)
                {
                    token.ThrowIfCancellationRequested();
                    NextTap_IE();
                    message.SendNotificationOK("Закоротите следующий отвод трансформатора", null);
                }
            });
        }
        private void External_Test_IE(int TFases, int Fase, double Volt, int n, double[] Pz_XX)
        {
            CheckVoltage = false;
            EmergencyStop();
            Thread.Sleep(500);
            StartNewTask(() =>
            {
                for (var number = 0; number < n; number++)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_IE((s) => Calc_IE(number, n, Fase, Volt, Pz_XX));
                }
                message.SendNotificationOK("Снизьте напряжение источника до нуля", null);
                while (Data.U >= 5)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_IE((s) => Data.CurrentFase = Fase);
                    OnDataReceived?.Invoke(Data);
                }
                SendResult?.Invoke();
                if (TFases == 1) message.SendNotificationOK("Отключите внешний источник питания и закоротите следующий отвод трансформатора", null);
            });
        }
        private void Calc_IE(int n, int number, int fase, double Volt, double[] Pz_XX)
        {
            if (n == 0) SA540DataXX.U_aver_XX[fase] = SA540DataXX.I_aver_XX[fase] = SA540DataXX.f_aver_XX[fase] = SA540DataXX.P_aver_XX[fase] =
                    SA540DataXX.Pp_aver_XX[fase] = SA540DataXX.cos_aver_XX[fase] = SA540DataXX.Z_aver_XX[fase] = SA540DataXX.R_aver_XX[fase] =
                    SA540DataXX.X_aver_XX[fase] = SA540DataXX.TgSigma_aver_XX[fase] = SA540DataXX.L_aver_XX[fase] = SA540DataXX.C_aver_XX[fase] =
                    SA540DataXX.Fi_aver_XX[fase] = 0;


            double d_rad = (float)(Math.PI * Data.D / (60 * 180));

            SA540DataXX.U_aver_XX[fase] += Data.U / number;
            SA540DataXX.I_aver_XX[fase] += Data.I / number;
            SA540DataXX.f_aver_XX[fase] += Data.F / number;
            SA540DataXX.P_aver_XX[fase] += (float)Math.Abs(Data.U * Data.I * Math.Cos(d_rad)) / number;
            SA540DataXX.Pp_aver_XX[fase] += (float)Math.Abs(Volt * Volt * Data.I * Math.Cos(d_rad) / Data.U) / number;
            SA540DataXX.cos_aver_XX[fase] += (float)(Math.Cos(d_rad) / number);
            SA540DataXX.Z_aver_XX[fase] += Data.U / Data.I / number;
            SA540DataXX.R_aver_XX[fase] += (float)(Math.Abs(Data.U * Math.Cos(d_rad) / Data.I) / number);
            SA540DataXX.X_aver_XX[fase] += (float)(Math.Abs(Data.U * Math.Sin(d_rad) / Data.I) / number);
            SA540DataXX.TgSigma_aver_XX[fase] += (float)(Math.Tan(Math.PI / 2 + d_rad) / number);
            SA540DataXX.L_aver_XX[fase] += (float)(Data.U * Math.Sin(d_rad) / Data.I / (2 * Math.PI * Data.F) / number);
            SA540DataXX.C_aver_XX[fase] += (float)(1 / (2 * Math.PI * Data.F * (Data.U / Data.I) * Math.Sin(d_rad)) * 1000000 / number);
            SA540DataXX.Fi_aver_XX[fase] += (float)(d_rad * 180 / Math.PI / number);

            if (n == number - 1)
            {
                //Calc_MeasOk_XX[fase] = 1;

                if (Math.Sin(d_rad) > 0)
                {
                    //индуктивный характер
                    SA540DataXX.cos_lag_aver_XX[fase] = 1;
                    if (SA540DataXX.C_aver_XX[fase] > 0)
                        SA540DataXX.C_aver_XX[fase] = -SA540DataXX.C_aver_XX[fase];
                }
                else
                {
                    //емкостной характер
                    SA540DataXX.cos_lag_aver_XX[fase] = 0;
                    if (SA540DataXX.X_aver_XX[fase] > 0)
                        SA540DataXX.X_aver_XX[fase] = -SA540DataXX.X_aver_XX[fase];
                    if (SA540DataXX.C_aver_XX[fase] < 0)
                        SA540DataXX.C_aver_XX[fase] = -SA540DataXX.C_aver_XX[fase];
                }

                //Calc_GetPz(Pz_XX);

                if (Pz_XX[fase] != 0) SA540DataXX.dP_aver_XX[fase] = (SA540DataXX.Pp_aver_XX[fase] - Pz_XX[fase]) * 100 / Pz_XX[fase];

                if (SA540DataXX.Pp_aver_XX[0] != 0 && SA540DataXX.Pp_aver_XX[1] != 0 && SA540DataXX.Pp_aver_XX[2] != 0)
                {
                    SA540DataXX.P_ratio_XX[0] = SA540DataXX.Pp_aver_XX[2] / SA540DataXX.Pp_aver_XX[0];
                    SA540DataXX.P_ratio_XX[1] = SA540DataXX.Pp_aver_XX[2] / SA540DataXX.Pp_aver_XX[1];
                    SA540DataXX.P_ratio_XX[2] = SA540DataXX.Pp_aver_XX[0] / SA540DataXX.Pp_aver_XX[1];
                    if (Pz_XX[0] != 0 && Pz_XX[1] != 0 && Pz_XX[2] != 0)
                    {
                        SA540DataXX.dP_ratio_XX[0] = (SA540DataXX.P_ratio_XX[0] - Pz_XX[2] / Pz_XX[0]) * 100 / (Pz_XX[2] / Pz_XX[0]);
                        SA540DataXX.dP_ratio_XX[1] = (SA540DataXX.P_ratio_XX[1] - Pz_XX[2] / Pz_XX[1]) * 100 / (Pz_XX[2] / Pz_XX[1]);
                        SA540DataXX.dP_ratio_XX[2] = (SA540DataXX.P_ratio_XX[2] - Pz_XX[0] / Pz_XX[1]) * 100 / (Pz_XX[0] / Pz_XX[1]);
                    }
                    else SA540DataXX.dP_ratio_XX[0] = SA540DataXX.dP_ratio_XX[1] = SA540DataXX.dP_ratio_XX[2] = 0;
                }
                else
                {
                    SA540DataXX.P_ratio_XX[0] = SA540DataXX.P_ratio_XX[1] = SA540DataXX.P_ratio_XX[2] = 0;
                    SA540DataXX.dP_ratio_XX[0] = SA540DataXX.dP_ratio_XX[1] = SA540DataXX.dP_ratio_XX[2] = 0;
                }
            }
        }
        #endregion
        #region SC
        private void SetU_SC(int TFases)
        {
            var response = ApplyCommand($"{DEVICEID} 2 6 0 {TFases}");
            if (response.Length != 0) Port_BytesReceived(response);
            else EmergencyStop();
        }//0x01 0x02 6 TFases CS
        private void SingleTest_SC(int Fase, Action<bool> action = null)
        {
            var response = ApplyCommand($"{DEVICEID} 163 6 0 {Fase}");
            if (response.Length != 0)
            {
                Port_BytesReceived(response);
                action?.Invoke(true);
            }
            else EmergencyStop();
        }//0x01 0xA3 6 Fase CS
        private void AutoTesting1_SC(int n, double[] Zb_KZ)
        {
            CheckVoltage = false;
            EmergencyStop();
            Thread.Sleep(500);
            StartNewTask(() =>
            {
                for (int i = 0; i < n; i++)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_SC(0, (s) => Calc_SC(i, n, 0, Zb_KZ));
                }
                message.SendNotificationOK("Снизьте напряжение источника до нуля", () => { TestEvent.Set(); });
                while (Data.U >= 0.008)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_SC(0, (s) => Data.CurrentFase = 0);
                    OnDataReceived?.Invoke(Data);
                }
                SendResult?.Invoke();
                Stop_Meas();
                Stop_Meas();
                Stop_Meas();
            });
        }
        private void AutoTesting3_SC(int Order, int n, double[] Zb_KZ)
        {
            CheckVoltage = false;
            EmergencyStop();
            Thread.Sleep(500);
            StartNewTask(() =>
            {
                if (Order == 0) //Последовательно
                {
                    for (int numberFase = 0; numberFase < 3; numberFase++)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest_SC(numberFase, (s) => Calc_SC(i, n, numberFase, Zb_KZ));
                        }
                    }
                }
                else    //Параллельно
                {
                    for (int i = 0; i < n; i++)
                    {
                        for (int numberFase = 0; numberFase < 3; numberFase++)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest_SC(numberFase, (s) => Calc_SC(i, n, numberFase, Zb_KZ));
                        }
                    }
                }
                message.SendNotificationOK("Снизьте напряжение источника до нуля", null);
                var UFase = new double[3];
                double U = 0;
                do
                {
                    for (int numberFase = 0; numberFase < 3; numberFase++)
                    {
                        SingleTest_SC(numberFase, (s) =>
                        {
                            UFase[numberFase] = Data.U;
                            Data.CurrentFase = numberFase;
                        });
                        OnDataReceived?.Invoke(Data);
                    }
                    U = UFase.Max();
                }
                while (U >= 0.008);
                SendResult?.Invoke();
                Stop_Meas();
                Stop_Meas();
                Stop_Meas();
            });
        }
        private void Calc_SC(int n, int number, int fase, double[] Zb_KZ)
        {
            double d_rad;
            double U = Data.U;
            double I = Data.I;
            double D = Data.D;
            Calc_CorrectNach(ref U, ref I, ref D);//Учет емкости кабеля

            if (n == 0) SA540DataKZ.U_aver_KZ[fase] = SA540DataKZ.I_aver_KZ[fase] = SA540DataKZ.f_aver_KZ[fase] = SA540DataKZ.Z_aver_KZ[fase] =
                   SA540DataKZ.Zp_aver_KZ[fase] = SA540DataKZ.cos_aver_KZ[fase] = SA540DataKZ.R_aver_KZ[fase] = SA540DataKZ.X_aver_KZ[fase] =
                   SA540DataKZ.P_aver_KZ[fase] = SA540DataKZ.TgSigma_aver_KZ[fase] = SA540DataKZ.L_aver_KZ[fase] = SA540DataKZ.C_aver_KZ[fase] =
                   SA540DataKZ.Fi_aver_KZ[fase] = 0;

            d_rad = (float)(Math.PI * D / (60 * 180));

            SA540DataKZ.U_aver_KZ[fase] += U / number;
            SA540DataKZ.I_aver_KZ[fase] += I / number;
            SA540DataKZ.f_aver_KZ[fase] += Data.F / number;
            SA540DataKZ.Z_aver_KZ[fase] += U / I / number;
            SA540DataKZ.Zp_aver_KZ[fase] += 50 / Data.F * (U / I) / number;
            SA540DataKZ.R_aver_KZ[fase] += (float)(Math.Abs(U / I * Math.Cos(d_rad)) / number);//последовательная схема
            SA540DataKZ.X_aver_KZ[fase] += (float)(Math.Abs(U / I * Math.Sin(d_rad)) / number);//последовательная схема
            SA540DataKZ.cos_aver_KZ[fase] += (float)(Math.Cos(d_rad) / number);
            SA540DataKZ.P_aver_KZ[fase] += (float)(Data.U * Data.I * Math.Cos(d_rad) / number);
            SA540DataKZ.TgSigma_aver_KZ[fase] += (float)(Math.Tan(Math.PI / 2 + d_rad) / number);
            SA540DataKZ.L_aver_KZ[fase] += (float)(Data.U / Data.I * Math.Sin(d_rad) / (2 * Math.PI * Data.F) / number);
            SA540DataKZ.C_aver_KZ[fase] += (float)(1 / (2 * Math.PI * Data.F * (Data.U / Data.I) * Math.Sin(d_rad)) / number);
            SA540DataKZ.Fi_aver_KZ[fase] += (float)(d_rad * 180 / Math.PI / number);
            if (n == number - 1)
            {
                if (Math.Sin(d_rad) > 0)
                {
                    //индуктивный характер
                    SA540DataKZ.cos_lag_aver_KZ[fase] = 1;
                    if (SA540DataKZ.C_aver_KZ[fase] > 0)
                        SA540DataKZ.C_aver_KZ[fase] = -SA540DataKZ.C_aver_KZ[fase];
                }
                else
                {
                    //емкостной характер
                    SA540DataKZ.cos_lag_aver_KZ[fase] = 0;
                    if (SA540DataKZ.X_aver_KZ[fase] > 0)
                        SA540DataKZ.X_aver_KZ[fase] = -SA540DataKZ.X_aver_KZ[fase];
                    if (SA540DataKZ.C_aver_KZ[fase] < 0)
                        SA540DataKZ.C_aver_KZ[fase] = -SA540DataKZ.C_aver_KZ[fase];
                }

                if (Zb_KZ[fase] != 0) SA540DataKZ.dZ_aver_KZ[fase] = (SA540DataKZ.Zp_aver_KZ[fase] - Zb_KZ[fase]) * 100 / Zb_KZ[fase];
            }
        }
        private void Calc_CorrectNach(ref double U, ref double I, ref double D) //Учет емкости кабеля
        {
            double dc, I_re, I_im, In_re, In_im;

            if (SA540Data.Zc == 0) return;
            //Коррекция тока, который возникает за счет емкости кабеля
            dc = (float)(SA540Data.dZc * Math.PI / (180 * 60));
            In_re = (float)(U * Math.Cos(dc) / SA540Data.Zc);
            In_im = (float)(U * Math.Sin(dc) / SA540Data.Zc);

            I_re = (float)(I * Math.Cos(D * Math.PI / (180 * 60)));
            I_im = (float)(I * Math.Sin(D * Math.PI / (180 * 60)));
            I_re -= In_re;
            I_im -= In_im;

            I = (float)Math.Sqrt(I_re * I_re + I_im * I_im);

            D = (float)(Math.Atan2(I_im, I_re) * 180 * 60 / Math.PI);
            if (D > 180 * 60) D -= 360 * 60;
            else if (D <= -180 * 60) D += 360 * 60;
        }
        #endregion
        #region KT
        private void SetU_KT(int TFases, int Source, double Volt)
        {

            StringBuilder str = new StringBuilder($"{DEVICEID} 3 12 0 {TFases} 0 {Source}");
            foreach (var item in BitConverter.GetBytes(Volt))
                str.Append(" " + item);
            var response = ApplyCommand(str.ToString(), 1500);
            if (response.Length != 0) Port_BytesReceived(response);
            else EmergencyStop();
        }
        public void SingleTest_KT(int Fase, Action<bool> action = null)
        {
            var response = ApplyCommand($"{DEVICEID} 164 7 0 {Fase} 1");
            if (response.Length != 0)
            {
                Port_BytesReceived(response);
                action?.Invoke(true);
            }
            else EmergencyStop();
        }
        private void Internal_Test_KT(int TFases, int FaseCount, int Source, int order, double Volt, int n, double Kz_KT)
        {
            serialController.UnsubscirbePortNotifications();
            StartNewTask(() =>
            {
                SA540DataKT.Install_Calc();
                SetU_KT(TFases, Source, Volt);
                if (order == 0) //Последовательно
                {
                    for (int numberFase = 0; numberFase < FaseCount; numberFase++)
                    {
                        for (int number = 0; number < n; number++)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest_KT(numberFase, (s) => Calc_KT(Data.Uh, Data.Ul, Data.D, Data.F, Data.I, number, n, numberFase, Kz_KT));
                            SendResult?.Invoke();
                        }
                    }
                }
                else     //Параллельно
                {
                    for (int number = 0; number < n; number++)
                    {
                        for (int numberFase = 0; numberFase < FaseCount; numberFase++)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest_KT(numberFase, (s) => Calc_KT(Data.Uh, Data.Ul, Data.D, Data.F, Data.I, number, n, numberFase, Kz_KT));
                            SendResult?.Invoke();
                        }
                    }
                }
                Stop_Meas();
                Stop_Meas();
                Stop_Meas();
            });
        }
        private void AutoTesting1_KT(int n, double Kz_KT)
        {
            CheckVoltage = false;
            EmergencyStop();
            Thread.Sleep(500);
            StartNewTask(() =>
            {
                for (int number = 0; number < n; number++)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_KT(0, (s) => Calc_KT(Data.Uh, Data.Ul, Data.D, Data.F, Data.I, number, n, 0, Kz_KT));
                }
                message.SendNotificationOK("Снизьте напряжение источника до нуля", null);
                while (Data.Uh >= 1)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_KT(0, (s) => Data.CurrentFase = 0);
                    OnDataReceived?.Invoke(Data);
                }
                Stop_Meas();
                Stop_Meas();
                Stop_Meas();
                SendResult?.Invoke();
            });
        }
        private void AutoTesting3_KT(int Order, int n, double Kz_KT)
        {
            CheckVoltage = false;
            EmergencyStop();
            Thread.Sleep(500);
            StartNewTask(() =>
            {
                if (Order == 0) //Последовательно
                {
                    for (int numberFase = 0; numberFase < 3; numberFase++)
                    {
                        for (int number = 0; number < n; number++)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest_KT(numberFase, (s) => Calc_KT(Data.Uh, Data.Ul, Data.D, Data.F, Data.I, number, n, numberFase, Kz_KT));
                        }
                    }
                }
                else    //Параллельно
                {
                    for (int number = 0; number < n; number++)
                    {
                        for (int numberFase = 0; numberFase < 3; numberFase++)
                        {
                            token.ThrowIfCancellationRequested();
                            SingleTest_KT(numberFase, (s) => Calc_KT(Data.Uh, Data.Ul, Data.D, Data.F, Data.I, number, n, numberFase, Kz_KT));
                        }
                    }
                }
                message.SendNotificationOK("Снизьте напряжение источника до нуля", null);
                double[] UFase = new double[3];
                double U = 0;
                do
                {
                    for (int numberFase = 0; numberFase < 3; numberFase++)
                    {
                        token.ThrowIfCancellationRequested();
                        SingleTest_KT(numberFase, (s) =>
                        {
                            UFase[numberFase] = Data.Uh;
                            Data.CurrentFase = numberFase;
                        });
                        OnDataReceived?.Invoke(Data);
                    }
                    U = UFase.Max();
                }
                while (U >= 1);
                SendResult?.Invoke();
                Stop_Meas();
                Stop_Meas();
                Stop_Meas();
            });
        }
        private void CheckCable_KT()
        {
            var response = ApplyCommand($"{DEVICEID} 35 6 0 1");
            if (response.Length != 0)
            {
                Port_BytesReceived(response);
                if (Data.Err == 1)
                {
                    message.SendNotificationOK("Отключите кабель для КТ", null);
                    EmergencyStop();
                }
            }
            else EmergencyStop();
        }//0x01 0x23 6 0x01 CS  
        private void Calc_KT(double Uh, double Ul, double d, double f, double I, int n, int number, int fase, double Kz_KT)
        {
            double d_rad;

            if (n == 0) SA540DataKT.Uh_aver_KT[fase] = SA540DataKT.Ul_aver_KT[fase] = SA540DataKT.f_aver_KT[fase] = SA540DataKT.K_aver_KT[fase] =
                   SA540DataKT.d_aver_KT[fase] = SA540DataKT.G_aver_KT[fase] = SA540DataKT.I_aver_KT[fase] = 0;

            d_rad = (float)(Math.PI * d / (60 * 180));

            SA540DataKT.Uh_aver_KT[fase] += Uh / number;
            SA540DataKT.Ul_aver_KT[fase] += Ul / number;
            SA540DataKT.f_aver_KT[fase] += f / number;
            SA540DataKT.K_aver_KT[fase] += Uh / Ul / number;
            SA540DataKT.I_aver_KT[fase] += I / number;

            //Правильное усреднение угла
            d /= 60; //в градусах
            if (n == 0) SA540DataKT.d_first[fase] = d;
            if (Math.Abs(SA540DataKT.d_first[fase]) > 160 && d < 0) d += 360;
            SA540DataKT.d_aver_KT[fase] += -d / number;

            if (n == number - 1)
            {
                if (SA540DataKT.d_aver_KT[fase] >= 180) SA540DataKT.d_aver_KT[fase] -= 360;
                if (SA540DataKT.d_aver_KT[fase] <= -180) SA540DataKT.d_aver_KT[fase] += 360;

                if (SA540DataKT.d_aver_KT[fase] <= -15) SA540DataKT.G_aver_KT[fase] = (float)((SA540DataKT.d_aver_KT[fase] + 360) / 30 + 0.5);
                else SA540DataKT.G_aver_KT[fase] = (float)(SA540DataKT.d_aver_KT[fase] / 30 + 0.5);

                SA540DataKT.G_aver_KT[fase] = Math.Abs(SA540DataKT.G_aver_KT[fase]);

                if (Kz_KT != 0) SA540DataKT.dK_aver_KT[fase] = (SA540DataKT.K_aver_KT[fase] - Kz_KT) * 100 / Kz_KT;
            }
        }
        #endregion
        #region IEN
        private void SetU_IEN()
        {
            var response = ApplyCommand($"{DEVICEID} 33 12 0 1 0 0 0 0 0 0");
            if (response.Length != 0) Port_BytesReceived(response);
            else EmergencyStop();
        }
        private void Stop_IEN()
        {
            StringBuilder str = new StringBuilder($"{DEVICEID} 36 5 0");
            Controller.WriteCommandPriority(new ControllerCommand()
            { Message = AddCS(str) });
        }//0x01 0x24 5 CS
        private void SingleTest_IEN(int ParU, int ParI, Action<bool> action = null)
        {
            var response = ApplyCommand($"{DEVICEID} 34 7 0 {ParU} {ParI}");
            if (response.Length != 0)
            {
                Port_BytesReceived(response);
                action(true);
            }
            else EmergencyStop();
        }//0x01 0x22 7 ParU ParI CS
        private void AutoTesting1_IEN(int n, double Volt)
        {
            CheckVoltage = false;
            EmergencyStop();
            Thread.Sleep(500);
            StartNewTask(() =>
            {
                for (int number = 0; number < n; number++)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_IEN(3, 0, (s) => Calc_IEN(0, Data.U, Data.I, Data.D, Data.F, number, n, 0, false, Volt));//фазное напряжение
                }
                message.SendNotificationOK("Снизьте напряжение источника до нуля", null);
                while (Data.U >= 5)
                {
                    token.ThrowIfCancellationRequested();
                    SingleTest_IEN(3, 0, (s) =>
                     {
                         Data.CurrentFase = 0;
                         Data.Uf = Data.U;
                     });
                    OnDataReceived?.Invoke(Data);
                }
                SendResult?.Invoke();
                Stop_IEN();
            });
        }
        private void AutoTesting3_IEN(int Order, int n, double Volt)
        {
            CheckVoltage = false;
            EmergencyStop();
            Thread.Sleep(500);
            double uf = 0;
            StartNewTask(() =>
            {
                if (Order == 0) //Последовательно
                {
                    for (int numberFase = 0; numberFase < 3; numberFase++)
                    {
                        for (int number = 0; number < n; number++)
                        {
                            token.ThrowIfCancellationRequested();
                            //межфазное напряжение
                            double Umf = 0;
                            SingleTest_IEN(numberFase, numberFase, (s) => Umf = Data.U);
                            token.ThrowIfCancellationRequested();
                            //фазное напряжение
                            SingleTest_IEN((byte)(numberFase + 3), numberFase, (s) => Calc_IEN(Umf, Data.U, Data.I, Data.D, Data.F, number, n, numberFase, true, Volt));
                        }
                    }
                }
                else     //Параллельно
                {
                    for (int number = 0; number < n; number++)
                    {
                        for (int numberFase = 0; numberFase < 3; numberFase++)
                        {
                            token.ThrowIfCancellationRequested();
                            double Umf = 0;
                            SingleTest_IEN(numberFase, numberFase, (s) => Umf = Data.U);
                            token.ThrowIfCancellationRequested();
                            SingleTest_IEN((byte)(numberFase + 3), numberFase, (s) => Calc_IEN(Umf, Data.U, Data.I, Data.D, Data.F, number, n, numberFase, true, Volt));
                        }
                    }
                }
                message.SendNotificationOK("Снизьте напряжение источника до нуля", null);
                double[] UFase = new double[3];
                double U = 0;
                do
                {
                    for (int numberFase = 0; numberFase < 3; numberFase++)
                    {
                        token.ThrowIfCancellationRequested();
                        SingleTest_IEN((byte)(numberFase + 3), numberFase, (s) =>
                        {
                            Data.CurrentFase = numberFase;
                            uf = Data.U;
                        });
                        token.ThrowIfCancellationRequested();
                        SingleTest_IEN(numberFase, numberFase, (s) =>
                        {
                            Data.CurrentFase = numberFase;
                            Data.Umf = Data.U;
                            Data.Uf = uf;
                            UFase[numberFase] = uf;
                        });
                        OnDataReceived?.Invoke(Data);
                        U = UFase.Max();
                    }
                }
                while (U >= 5);
                SendResult?.Invoke();
                token.ThrowIfCancellationRequested();
                Stop_IEN();
            });
        }
        private void Calc_IEN(double Umf, double Uf, double I, double d, double f, int n, int number, int fase, bool threeFases, double Volt)
        {
            double Up, d_rad;

            if (!threeFases) Up = Volt;
            else Up = (float)(Volt / 1.73205);

            Calc_CorrectNach(ref Uf, ref I, ref d);//Учет емкости кабеля

            if (n == 0) SA540DataXXN.Umf_aver_XXN[fase] = SA540DataXXN.Uf_aver_XXN[fase] = SA540DataXXN.I_aver_XXN[fase] = SA540DataXXN.f_aver_XXN[fase] =
                SA540DataXXN.Pp_aver_XXN[fase] = SA540DataXXN.Qp_aver_XXN[fase] = SA540DataXXN.Sp_aver_XXN[fase] = SA540DataXXN.Fi_aver_XXN[fase] =
                SA540DataXXN.cos_aver_XXN[fase] = SA540DataXXN.P_aver_XXN[fase] = SA540DataXXN.Q_aver_XXN[fase] = SA540DataXXN.S_aver_XXN[fase] =
                SA540DataXXN.R_aver_XXN[fase] = SA540DataXXN.X_aver_XXN[fase] = SA540DataXXN.L_aver_XXN[fase] = 0;

            d_rad = (float)(Math.PI * d / (60 * 180));

            SA540DataXXN.Umf_aver_XXN[fase] += Umf / number;
            SA540DataXXN.Uf_aver_XXN[fase] += Uf / number;
            SA540DataXXN.I_aver_XXN[fase] += I / number;
            SA540DataXXN.f_aver_XXN[fase] += f / number;
            SA540DataXXN.Pp_aver_XXN[fase] += (float)(Up * Up * I * Math.Cos(d_rad) / Uf / number);
            SA540DataXXN.Qp_aver_XXN[fase] += (float)(Up * Up * I * Math.Sin(d_rad) / Uf / number);
            SA540DataXXN.Sp_aver_XXN[fase] += Up * Up * I / Uf / number;
            SA540DataXXN.cos_aver_XXN[fase] += (float)(Math.Cos(d_rad) / number);
            SA540DataXXN.P_aver_XXN[fase] += (float)(Uf * I * Math.Cos(d_rad) / number);
            SA540DataXXN.Q_aver_XXN[fase] += (float)(Uf * I * Math.Sin(d_rad) / number);
            SA540DataXXN.S_aver_XXN[fase] += (float)(Uf * I / number);
            SA540DataXXN.R_aver_XXN[fase] += (float)(Uf * Math.Cos(d_rad) / I / number);
            SA540DataXXN.X_aver_XXN[fase] += (float)(Uf * Math.Sin(d_rad) / I / number);
            SA540DataXXN.L_aver_XXN[fase] += (float)(Uf * Math.Sin(d_rad) / I / (2 * Math.PI * f) / number);
            //Правильное усреднение угла
            d /= 60; //в градусах
            if (n == 0) SA540DataXXN.d_first[fase] = d;
            if (Math.Abs(SA540DataXXN.d_first[fase]) > 160 && d < 0) d += 360;
            SA540DataXXN.Fi_aver_XXN[fase] += d / number;

            if (n == number - 1)
            {
                if (Math.Sin(d_rad) > 0)
                {
                    //индуктивный характер
                    SA540DataXXN.cos_lag_aver_XXN[fase] = 1;
                }
                else
                {
                    //емкостной характер
                    SA540DataKZ.cos_lag_aver_KZ[fase] = 0;
                    if (SA540DataXXN.X_aver_XXN[fase] > 0)
                        SA540DataXXN.X_aver_XXN[fase] = -SA540DataXXN.X_aver_XXN[fase];
                }
                if (threeFases)
                {
                    SA540DataXXN.U0_XXN = (float)((SA540DataXXN.Umf_aver_XXN[0] + SA540DataXXN.Umf_aver_XXN[1] + SA540DataXXN.Umf_aver_XXN[2]) / 3.0);
                    SA540DataXXN.I0_XXN = (float)((SA540DataXXN.I_aver_XXN[0] + SA540DataXXN.I_aver_XXN[1] + SA540DataXXN.I_aver_XXN[2]) / 3.0);
                    SA540DataXXN.F0_XXN = (float)((SA540DataXXN.f_aver_XXN[0] + SA540DataXXN.f_aver_XXN[1] + SA540DataXXN.f_aver_XXN[2]) / 3.0);
                    SA540DataXXN.P0_XXN = SA540DataXXN.Pp_aver_XXN[0] + SA540DataXXN.Pp_aver_XXN[1] + SA540DataXXN.Pp_aver_XXN[2];
                    SA540DataXXN.Q0_XXN = SA540DataXXN.Qp_aver_XXN[0] + SA540DataXXN.Qp_aver_XXN[1] + SA540DataXXN.Qp_aver_XXN[2];
                    SA540DataXXN.S0_XXN = SA540DataXXN.Sp_aver_XXN[0] + SA540DataXXN.Sp_aver_XXN[1] + SA540DataXXN.Sp_aver_XXN[2];
                }
                else
                {
                    SA540DataXXN.U0_XXN = SA540DataXXN.Uf_aver_XXN[0];
                    SA540DataXXN.I0_XXN = SA540DataXXN.I_aver_XXN[0];
                    SA540DataXXN.F0_XXN = SA540DataXXN.f_aver_XXN[0];
                    SA540DataXXN.P0_XXN = SA540DataXXN.Pp_aver_XXN[0];
                    SA540DataXXN.Q0_XXN = SA540DataXXN.Qp_aver_XXN[0];
                    SA540DataXXN.S0_XXN = SA540DataXXN.Sp_aver_XXN[0];
                }
            }
        }
        #endregion
        #region Help methods
        /// <summary>
        /// Создает асинхронную задачу для отправки данных в порт
        /// </summary>
        /// <param name="action"></param>
        private void StartNewTask(Action action, [CallerMemberName] string NameMethod = "")
        {
            if (currentTask?.IsCompleted ?? true || currentTask?.Status == TaskStatus.RanToCompletion || currentTask?.Status == TaskStatus.Canceled)
            {
                if (tokenSource != null) tokenSource.Dispose();
                tokenSource = new CancellationTokenSource();
                token = tokenSource.Token;
                OnStateChanged?.Invoke(true);
                currentTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        action.Invoke();
                        OnStateChanged?.Invoke(false);
                        MeasStopped?.Invoke(Mode);
                    }
                    catch
                    {
                        //
                        OnStateChanged?.Invoke(false);
                    }
                }, token);
            }
        }
        private byte[] ApplyCommand(string msg, int delay = 100) => serialController.ApplyCommand(AddCS(new StringBuilder(msg)).Split(' ').Select(x => Convert.ToByte(x)).ToArray(), delay);
        private string AddCS(StringBuilder str)
        {
            int cs = 0;
            foreach (var item in str.ToString().Split(' '))
                cs ^= byte.Parse(item);
            str.Append(" " + cs);
            return str.ToString();
        }
        public override void SetPort(System.IO.Ports.SerialPort port)
        {
            base.SetPort(port);
            Controller.CommandAwaitInterval = 6000;
            Controller.WriteTryCount = 5;
            serialController = Controller as SerialPortController;
        }
        #endregion
    }
}
