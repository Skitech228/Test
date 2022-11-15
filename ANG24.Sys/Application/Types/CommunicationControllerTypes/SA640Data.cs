using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class SA640Data : ModbusData
    {
        // Параметры fU, fI и fR имеют большую степень фильтрации чем gU, gI и gR,
        // поэтому вначале измерения, когда идет заряд индуктивности и параметры не стабилизировалась,
        // на экране отображается gU, gI и gR, а затем, когда сигнал стал более стабилен - fU, fI и fR.

        // Для отображения параметров в графическом виде используется gU, gI и gR.
        public SA640Data() { }
        public override void ParseByteData(byte[] data) //Little indian (DCBA)
        {
            base.ParseByteData(data);
            ///парсинг данных
            try
            {
                Command = data[1];
                if (Command != 0x00) Message = $"Command {Command} done";
                CS = data[data.Length - 1];
                switch (Command)
                {
                    case 0x1F: //0xFF
                        Message = Encoding.GetEncoding("windows-1251").GetString(data, 4, data.Length - (data.Length - data[2]) - 6);
                        break;
                    case 0x38:
                        if (data.Length >= 28)
                        {
                            FU = (float)Math.Round(ToFloat(new byte[] { data[4], data[5], data[6], data[7] }), 4);
                            FI = (float)Math.Round(ToFloat(new byte[] { data[8], data[9], data[10], data[11] }), 6);
                            FR = (float)Math.Round(ToFloat(new byte[] { data[12], data[13], data[14], data[15] }), 4);
                            GU = (float)Math.Round(ToFloat(new byte[] { data[16], data[17], data[18], data[19] }), 4);
                            GI = (float)Math.Round(ToFloat(new byte[] { data[20], data[21], data[22], data[23] }), 6);
                            GR = (float)Math.Round(ToFloat(new byte[] { data[24], data[25], data[26], data[27] }), 4);
                            State = data[data.Length - 2];
                        }
                        break;
                    case 0x00:
                        ErrorCode = data[data.Length - 2];
                        break;
                }
            }
            catch { }
        }

        #region
        /// <summary>
        /// напряжение (дополнительная фильтрация);
        /// </summary>
        public double FU { get; set; }
        /// <summary>
        /// ток (дополнительная фильтрация);
        /// </summary>
        public double FI { get; set; }
        /// <summary>
        /// сопротивление (дополнительная фильтрация);
        /// </summary>
        public double FR { get; set; }
        /// <summary>
        /// Напряжение
        /// </summary>
        public double GU { get; set; }
        /// <summary>
        /// Ток
        /// </summary>
        public double GI { get; set; }
        /// <summary>
        /// сопротивление
        /// </summary>
        public double GR { get; set; }
        /// <summary>
        /// состояние измерительного блока
        /// 0 – Standby – режим ожидания;
        /// 1 – Stabilization – стабилизация тока;
        /// 2 – Discharge – разряд;
        /// 3 – EmergencyStop – экстренная остановка;
        /// </summary>
        public int State { get; set; }

        // Errors
        // 1 Перегрузка! Подано большое напряжение.
        // 2 Перегрузка! Подан большой ток.
        // 3 ---
        // 4 Выполняется разряд индуктивности!
        // 5 Превышен ток источника!
        // 6 Перегрев! Превышена температура прибора.
        #endregion
    }
    public class SA640Transformer
    {
        public SA640Transformer(byte phaseCount)
        {
            PhaseCount = phaseCount;
            Coils = new List<Coil>();
        }
        public string Name { get; set; }
        public byte PhaseCount { get; private set; }
        public Coil this[string? value] => Coils.FirstOrDefault(c => c.Name == value);
        public List<Coil> Coils { get; private set; }

        public class Phase : INotifyPropertyChanged
        {
            public Phase(string name, Coil coil, byte position)
            {
                Name = name;
                Coil = coil;
                Position = position;
            }
            private int _index;
            public int Index
            {
                get => _index;
                set { _index = value; NotifyPropertyChanged(); }
            }
            public string Name { get; }
            private double _u;
            public double U
            {
                get => _u;
                set { _u = value; NotifyPropertyChanged(); }
            }
            private double _r;
            public double R
            {
                get => _r;
                set
                {
                    _r = value;
                    NotifyPropertyChanged();
                }
            }
            private double _i;
            public double I
            {
                get => _i;
                set { _i = value; NotifyPropertyChanged(); }
            }
            public Coil Coil { get; private set; }
            public byte Position { get; }

            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public override string ToString() => $"{Index:00}: {R}";
        }
        public class Coil
        {
            public Position this[int value] => Positions.FirstOrDefault(c => c.I == value);
            public Coil(byte phaseCount, byte positionsCount, byte switchType)
            {
                if (switchType == 0)
                {
                    Positions = new Position[1];
                    Positions[0] = new Position(phaseCount, this, 0, Сonnection?.ToLower().EndsWith("n") ?? false);
                }
                else
                {
                    Positions = new Position[positionsCount == 0 ? 1 : positionsCount];
                    for (int i = 0; i < Positions.Length; i++)
                        Positions[i] = new Position(phaseCount, this, (byte)i, Сonnection?.ToLower().EndsWith("n") ?? false);
                }
            }

            public string Name { get; set; }
            public string Сonnection { get; set; }
            /// <summary>
            /// 0 - нет переключателя
            /// 1 - ПБВ
            /// 2 - РПН
            /// </summary>
            public byte SwitchType { get; set; }
            public byte NPositions { get; set; }
            public double NominalI { get; set; }
            public Position[] Positions;
            public override string ToString() => Сonnection != "" ? $"{Name} - {Сonnection}" : Name;
            public class Position : INotifyPropertyChanged
            {
                private double dR;
                public Position(byte phaseCount, Coil coil, byte i, bool isN)
                {
                    I = i;
                    if (phaseCount == 3)
                    {
                        if (isN)
                            Phases = new Phase[3]
                            {
                                new Phase("AN", coil, I),
                                new Phase("BN", coil, I),
                                new Phase("CN", coil, I)
                            };
                        else
                            Phases = new Phase[3]
                            {
                                new Phase("AB", coil, I),
                                new Phase("BC", coil, I),
                                new Phase("CA", coil, I)
                            };
                    }
                    else Phases = new Phase[1] { new Phase("AN", coil, I) };
                    if (phaseCount == 3)
                    {
                        Phases[0].PropertyChanged += (sender, e) => { CalculateDR(); };
                        Phases[2].PropertyChanged += (sender, e) => { CalculateDR(); };
                    }
                    DR = 0;
                }
                private void CalculateDR()
                {
                    var sum = new double[] { Phases[0].R, Phases[1].R, Phases[2].R }.OrderBy(x => x).ToArray();
                    DR = Math.Abs(sum[0] - sum[2]) / (sum.Aggregate((x, y) => x + y) / 3 / 100);
                    if (double.IsNaN(DR)) DR = 0;
                }
                public Phase[] Phases { get; private set; }
                public byte I { get; }
                public double DR { get => dR; private set { dR = value; NotifyPropertyChanged(); } }
                public event PropertyChangedEventHandler PropertyChanged;
                private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                // доп данные
            }
        }
        public void AddCoil(string name, string connection, byte switchType, byte nPositions, double nominalI)
        {
            Coils.Add(new Coil(PhaseCount, nPositions, switchType)
            {
                Name = name,
                Сonnection = PhaseCount == 1 ? "" : connection,
                SwitchType = switchType,
                NPositions = switchType == 0 ? (byte)1 : nPositions,
                NominalI = nominalI
            });
        }
        public void DeleteCoil(Coil coil) => Coils.Remove(coil);
    }
}
