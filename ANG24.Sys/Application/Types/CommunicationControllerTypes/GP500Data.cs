using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using System.Diagnostics;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class GP500Data : ModbusData
    {
        public string ScreanName { get; set; }
        public byte bMode { get; set; }
        public byte bState { get; set; }
        public byte bCursor { get; set; }
        public short U1 { get; set; }
        public short I1 { get; set; }

        public GP500Data() { }
        public override void ParseByteData(byte[] data)
        {
            ///парсинг данных
            try
            {
                Debug.WriteLine("in " + DateTime.Now.ToString("hh:mm:ss") + " " + BitConverter.ToString(data));
                ScreanName = GetScreanName(data[1]);
                CS = data[^1];
                switch (ScreanName)
                {
                    case "Основной":

                        break;
                    case "Информационный":

                        break;
                    case "Настроечный":

                        break;
                    case "Дополнительно 1":

                        break;
                    case "Дополнительно 2":

                        break;
                    case "Дополнительно 3":

                        break;
                    case "Дополнительно 4":
                        break;

                    case "Дополнительно 5":

                        break;
                    case "Дополнительно 6":

                        break;



                    // Аварийные экраны
                    case "Допуск U":

                        break;
                    case "Допуск Ep":

                        break;
                    case "Симметр. Ep":

                        break;
                    case "Огр. I":

                        break;
                    case "Отказ вентил.":

                        break;
                    case "Огр. T":

                        break;
                    case "Сеть не в допуске":

                        break;
                }
            }
            catch { }

            string GetScreanName(byte data)
            {
                return data switch
                {
                    0x00 => "Основной",
                    0x01 => "Информационный",
                    0x02 => "Настроечный",
                    0x03 => "Дополнительно 1",
                    0x04 => "Дополнительно 2",
                    0x05 => "Дополнительно 3",
                    0x06 => "Дополнительно 4",
                    0x07 => "Дополнительно 5",
                    0x08 => "Дополнительно 6",

                    0x10 => "Допуск U",
                    0x20 => "Допуск Ep",
                    0x30 => "Симметр. Ep",
                    0x40 => "Огр. I",
                    0x50 => "Отказ вентил.",
                    0x60 => "Огр. T",
                    0x70 => "Сеть не в допуске",
                    _ => throw new NotImplementedException()
                };
            }
        }
        #region
        public static double Zc { get; set; }
        public static double dZc { get; set; }

        /// <summary>
        /// Напряжение
        /// </summary>
        public double U { get; set; }
        /// <summary>
        /// Ток
        /// </summary>
        public double I { get; set; }
        /// <summary>
        /// угол между током и напряжением;
        /// </summary>
        public double D { get; set; }
        /// <summary>
        /// частота
        /// </summary>
        public double F { get; set; }

        /// <summary>
        /// высокое напряжение
        /// </summary>
        public double Uh { get; set; }
        /// <summary>
        /// низкое напряжение
        /// </summary>
        public double Ul { get; set; }

        /// <summary>
        /// проверка подключения кабеля для КТ
        /// 0 - кабель отключен;
        /// 1 - кабель подключен;
        /// </summary>
        public int Err { get; set; }

        public double Umf { get; set; }
        public double Uf { get; set; }

        // 1 Перегрузка! Подано большое напряжение.
        // 2 Перегрузка! Подан большой ток.
        // 3 Нет синхронизации!
        // 4 Подключен кабель КИ(КТ)! Отключите кабель КИ(КТ).
        // 5 Отключен кабель КИ(КТ)! Подключите кабель КИ(КТ).
        // 6 Проверте схему измерительной цепи.
        // 7 Превышен ток встроенного источника.

        /// <summary>
        /// Отвод трансформатора, который измеряется
        /// </summary>
        public int CurrentFase { get; set; }
        #endregion
    }
}
