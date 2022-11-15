using ANG24.Sys.Application.Types.CommunicationControllerTypes.AbstractModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public sealed class SA540Data : ModbusData
    {
        public SA540Data() { }
        public override void ParseByteData(byte[] data)
        {
            base.ParseByteData(data);
            ///парсинг данных
            try
            {
                Debug.WriteLine("in " + DateTime.Now.ToString("hh:mm:ss") + " " + BitConverter.ToString(data));
                Command = data[1];
                if (data[1] != 0x00) Message = $"Command done";
                CS = data[data.Length - 1];
                switch (Command)
                {
                    case 0xFF: //0xFF
                        Zc = ToFloat(new byte[] { data[4], data[5], data[6], data[7] });
                        dZc = ToFloat(new byte[] { data[8], data[9], data[10], data[11] });
                        Message = Encoding.GetEncoding("windows-1251").GetString(data, 12, data.Length - (data.Length - data[2]) - 14);
                        break;
                    case 0xA0:
                        if (data.Length >= 21)
                        {
                            U = ToFloat(new byte[] { data[4], data[5], data[6], data[7] });
                            I = ToFloat(new byte[] { data[8], data[9], data[10], data[11] });
                            D = ToFloat(new byte[] { data[12], data[13], data[14], data[15] });
                            F = ToFloat(new byte[] { data[16], data[17], data[18], data[19] });
                        }
                        break;
                    case 0xA3:
                        if (data.Length >= 21)
                        {
                            U = ToFloat(new byte[] { data[4], data[5], data[6], data[7] });
                            I = ToFloat(new byte[] { data[8], data[9], data[10], data[11] });
                            D = ToFloat(new byte[] { data[12], data[13], data[14], data[15] });
                            F = ToFloat(new byte[] { data[16], data[17], data[18], data[19] });
                        }
                        break;
                    case 0xA4:
                        if (data.Length >= 24)
                        {
                            Uh = ToFloat(new byte[] { data[4], data[5], data[6], data[7] });
                            Ul = ToFloat(new byte[] { data[8], data[9], data[10], data[11] });
                            D = ToFloat(new byte[] { data[12], data[13], data[14], data[15] });
                            F = ToFloat(new byte[] { data[16], data[17], data[18], data[19] });
                            I = ToFloat(new byte[] { data[20], data[21], data[22], data[23] });
                        }
                        break;
                    case 0x23:
                        Err = data[4];
                        break;
                    case 0x22:
                        if (data.Length >= 21)
                        {
                            U = ToFloat(new byte[] { data[4], data[5], data[6], data[7] });
                            I = ToFloat(new byte[] { data[8], data[9], data[10], data[11] });
                            D = ToFloat(new byte[] { data[12], data[13], data[14], data[15] });
                            F = ToFloat(new byte[] { data[16], data[17], data[18], data[19] });
                        }
                        break;
                    case 0x00:
                        ErrorCode = data[data.Length - 2];
                        Message = $"Error {ErrorCode}";
                        break;
                    case 0x01:

                        break;
                        //case 0xA1:
                        //    break;
                        //case 0x02:
                        //    break;
                        //case 0xA2:
                        //    break;
                        //case 0x03:
                        //    break;               
                        //case 0x21:
                        //    break;
                        //case 0x24:
                        //    break;
                }
            }
            catch { }

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

    public static class SA540DataKT
    {
        static SA540DataKT()
        {
            Uh_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            Ul_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            f_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            K_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            d_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            G_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            dK_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            dK_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            d_first = new ObservableCollection<double>() { 0, 0, 0 };
            I_aver_KT = new ObservableCollection<double> { 0, 0, 0 };
        }
        public static void Install_Calc()
        {
            Uh_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            Ul_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            f_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            K_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            d_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            G_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            dK_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            dK_aver_KT = new ObservableCollection<double>() { 0, 0, 0 };
            d_first = new ObservableCollection<double>() { 0, 0, 0 };
            I_aver_KT = new ObservableCollection<double> { 0, 0, 0 };
        }
        /// <summary>
        /// Усредненное значение напряжения на обмотке ВН
        /// </summary>
        public static ObservableCollection<double> Uh_aver_KT { get; set; }
        /// <summary>
        /// Усредненное значение на обмотке НН
        /// </summary>
        public static ObservableCollection<double> Ul_aver_KT { get; set; }
        /// <summary>
        /// Усредненное значение частоты
        /// </summary>
        public static ObservableCollection<double> f_aver_KT { get; set; }
        /// <summary>
        /// Усредненное значение коэффициента трансформации
        /// </summary>
        public static ObservableCollection<double> K_aver_KT { get; set; }
        /// <summary>
        /// Группа соединения обмоток
        /// </summary>
        public static ObservableCollection<double> G_aver_KT { get; set; }
        /// <summary>
        /// Относительная разность измеренных и базовых значений коэффициента трансформации
        /// </summary>
        public static ObservableCollection<double> dK_aver_KT { get; set; }
        /// <summary>
        /// Служебное значение
        /// </summary>
        public static ObservableCollection<double> d_first { get; set; }
        /// <summary>
        /// Усредненное значение угла
        /// </summary>
        public static ObservableCollection<double> d_aver_KT { get; set; }
        public static ObservableCollection<double> I_aver_KT { get; set; }
    }
    public static class SA540DataKZ
    {
        static SA540DataKZ()
        {
            U_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            I_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            f_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            Z_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            Zp_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            cos_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            R_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            X_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            dZ_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            P_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            L_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            C_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            Fi_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            TgSigma_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            cos_lag_aver_KZ = new ObservableCollection<byte>() { 0, 0, 0 };
        }
        public static void Instal_Calc()
        {
            U_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            I_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            f_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            Z_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            Zp_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            cos_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            R_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            X_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            dZ_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            P_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            L_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            C_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            Fi_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            TgSigma_aver_KZ = new ObservableCollection<double>() { 0, 0, 0 };
            cos_lag_aver_KZ = new ObservableCollection<byte>() { 0, 0, 0 };
        }
        /// <summary>
        /// Усредненное значение напряжения
        /// </summary>
        public static ObservableCollection<double> U_aver_KZ { get; set; }
        /// <summary>
        /// Усредненное значение тока
        /// </summary>
        public static ObservableCollection<double> I_aver_KZ { get; set; }
        /// <summary>
        /// Усредненное значение частоты
        /// </summary>
        public static ObservableCollection<double> f_aver_KZ { get; set; }
        /// <summary>
        /// Усредненное значение полного сопротивления
        /// </summary>
        public static ObservableCollection<double> Z_aver_KZ { get; set; }
        /// <summary>
        /// Усредненное значение приведенного по частоте полного сопротивления
        /// </summary>
        public static ObservableCollection<double> Zp_aver_KZ { get; set; }
        /// <summary>
        /// Усредненное значение коэффициента мощности
        /// </summary>
        public static ObservableCollection<double> cos_aver_KZ { get; set; }
        /// <summary>
        /// Усредненное значение активного сопротивления
        /// </summary>
        public static ObservableCollection<double> R_aver_KZ { get; set; }
        /// <summary>
        /// Усредненное значение реактивного сопротивления
        /// </summary>
        public static ObservableCollection<double> X_aver_KZ { get; set; }
        /// <summary>
        /// Относительная разность измеренных измеренных и базовых значений полного сопротивления
        /// </summary>
        public static ObservableCollection<double> dZ_aver_KZ { get; set; }
        /// <summary>
        /// Усреднение активной составляющей полной мощности
        /// </summary>
        public static ObservableCollection<double> P_aver_KZ { get; set; }
        /// <summary>
        /// Усреднение индуктивности
        /// </summary>
        public static ObservableCollection<double> L_aver_KZ { get; set; }
        /// <summary>
        /// Усреднение емкости
        /// </summary>
        public static ObservableCollection<double> C_aver_KZ { get; set; }
        /// <summary>
        /// Усредненное значение разности фаз
        /// </summary>
        public static ObservableCollection<double> Fi_aver_KZ { get; set; }
        /// <summary>
        /// Усреднение значения тангенса угла диэлектрических потерь
        /// </summary>
        public static ObservableCollection<double> TgSigma_aver_KZ { get; set; }
        /// <summary>
        /// Характер
        /// 1 - Емкостный
        /// 2 - Индуктивный
        /// </summary>
        public static ObservableCollection<byte> cos_lag_aver_KZ { get; set; }
    }
    public static class SA540DataXX
    {
        static SA540DataXX()
        {
            U_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            I_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            f_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            P_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            Pp_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            P_ratio_XX = new ObservableCollection<double>() { 0, 0, 0 };
            dP_ratio_XX = new ObservableCollection<double>() { 0, 0, 0 };
            dP_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            cos_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            Z_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            R_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            X_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            L_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            C_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            Fi_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            TgSigma_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            cos_lag_aver_XX = new ObservableCollection<byte>() { 0, 0, 0 };
        }
        public static void InstallCalc()
        {
            U_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            I_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            f_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            P_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            Pp_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            P_ratio_XX = new ObservableCollection<double>() { 0, 0, 0 };
            dP_ratio_XX = new ObservableCollection<double>() { 0, 0, 0 };
            dP_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            cos_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            Z_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            R_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            X_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            L_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            C_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            Fi_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            TgSigma_aver_XX = new ObservableCollection<double>() { 0, 0, 0 };
            cos_lag_aver_XX = new ObservableCollection<byte>() { 0, 0, 0 };
        }
        /// <summary>
        /// Усредненное значение напряжения
        /// </summary>
        public static ObservableCollection<double> U_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение тока
        /// </summary>
        public static ObservableCollection<double> I_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение частоты
        /// </summary>
        public static ObservableCollection<double> f_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение мощности потерь
        /// </summary>
        public static ObservableCollection<double> P_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение приведенной мощности потерь
        /// </summary>
        public static ObservableCollection<double> Pp_aver_XX { get; set; }
        /// <summary>
        /// Отношение мощностей потерь между фазами
        /// </summary>
        public static ObservableCollection<double> P_ratio_XX { get; set; }
        /// <summary>
        /// Усредненное значение относительного отклонения приведенной активной состовляющей мощности 
        /// </summary>
        public static ObservableCollection<double> dP_aver_XX { get; set; }
        /// <summary>
        /// Относительная разность отношений измеренных и заводских мощностей потерь
        /// </summary>
        public static ObservableCollection<double> dP_ratio_XX { get; set; }
        /// <summary>
        /// Усредненное значение коэффициента мощности
        /// </summary>
        public static ObservableCollection<double> cos_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение полного сопротивления
        /// </summary>
        public static ObservableCollection<double> Z_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение активной составляющей полного сопротивления
        /// </summary>
        public static ObservableCollection<double> R_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение реактивной составляющей полного сопротивления
        /// </summary>
        public static ObservableCollection<double> X_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение индуктивности
        /// </summary>
        public static ObservableCollection<double> L_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение емкости
        /// </summary>
        public static ObservableCollection<double> C_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение разности фаз
        /// </summary>
        public static ObservableCollection<double> Fi_aver_XX { get; set; }
        /// <summary>
        /// Усредненное значение тангенса угла диэлектрических потерь
        /// </summary>
        public static ObservableCollection<double> TgSigma_aver_XX { get; set; }
        /// <summary>
        /// Характер
        /// 1 - Емкостный
        /// 2 - Индуктивный
        /// </summary>
        public static ObservableCollection<byte> cos_lag_aver_XX { get; set; }
    }
    public static class SA540DataXXN
    {
        static SA540DataXXN()
        {
            Umf_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Uf_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            I_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            f_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Pp_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Qp_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Sp_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Fi_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            P_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Q_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            S_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            L_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            R_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            X_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            cos_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            d_first = new ObservableCollection<double>() { 0, 0, 0 };
            cos_lag_aver_XXN = new ObservableCollection<byte>() { 0, 0, 0 };
        }
        public static void InstallCalc()
        {
            Umf_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Uf_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            I_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            f_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Pp_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Qp_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Sp_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Fi_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            P_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            Q_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            S_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            L_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            R_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            X_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            cos_aver_XXN = new ObservableCollection<double>() { 0, 0, 0 };
            d_first = new ObservableCollection<double>() { 0, 0, 0 };
            cos_lag_aver_XXN = new ObservableCollection<byte>() { 0, 0, 0 };
        }
        /// <summary>
        /// Усредненное значение междуфазного напряжения
        /// </summary>
        public static ObservableCollection<double> Umf_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение фазного напряжения
        /// </summary>
        public static ObservableCollection<double> Uf_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение тока
        /// </summary>
        public static ObservableCollection<double> I_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение частоты
        /// </summary>
        public static ObservableCollection<double> f_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение приведенной мощности потерь
        /// </summary>
        public static ObservableCollection<double> Pp_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение приведенной реактивности мощности
        /// </summary>
        public static ObservableCollection<double> Qp_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение приведенной полной мощности
        /// </summary>
        public static ObservableCollection<double> Sp_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение угла
        /// </summary>
        public static ObservableCollection<double> Fi_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение активной составляющей полной мощности
        /// </summary>
        public static ObservableCollection<double> P_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение реактивной составляющей полной мощности
        /// </summary>
        public static ObservableCollection<double> Q_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение полной мощности
        /// </summary>
        public static ObservableCollection<double> S_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение индуктивности
        /// </summary>
        public static ObservableCollection<double> L_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение активной составляющей полного сопротивления
        /// </summary>
        public static ObservableCollection<double> R_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение реактивной составляющей полного сопротивления
        /// </summary>
        public static ObservableCollection<double> X_aver_XXN { get; set; }
        /// <summary>
        /// Усредненное значение коэффициента мощности
        /// </summary>
        public static ObservableCollection<double> cos_aver_XXN { get; set; }
        /// <summary>
        /// Служебное значение
        /// </summary>
        public static ObservableCollection<double> d_first { get; set; }
        public static ObservableCollection<byte> cos_lag_aver_XXN { get; set; }
        /// <summary>
        /// Среднее значение междуфазных напряжений
        /// </summary>
        public static double U0_XXN { get; set; }
        /// <summary>
        /// Ток холостого хода
        /// </summary>
        public static double I0_XXN { get; set; }
        /// <summary>
        /// Среднее значение частоты
        /// </summary>
        public static double F0_XXN { get; set; }
        /// <summary>
        /// Потери холостого хода
        /// </summary>
        public static double P0_XXN { get; set; }
        /// <summary>
        /// Среднее значение реактивной мощности
        /// </summary>
        public static double Q0_XXN { get; set; }
        /// <summary>
        /// Среднее значение полной мощности
        /// </summary>
        public static double S0_XXN { get; set; }
    }
}
