namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels
{
    public class GVIErrorInfo
    {
        public bool ChargerSet { get; set; }
        public bool ChargerReset { get; set; }
        public bool MeasureSet { get; set; }
        public bool MeasureReset { get; set; }
        public bool LeavingCapacitor { get; set; }
        public bool ShutingCapacitor { get; set; }
        public bool LeavingCable { get; set; }
        public bool ShutingCable { get; set; }
        public bool LeavingCap2Cab { get; set; }
        public bool ShutingCap2Cab { get; set; }
        public bool HiTemp { get; set; }
        public bool HiTemp2 { get; set; }
        public bool OverCap { get; set; }
        public bool OverReg { get; set; }
        public bool CriticalErrorCapShoot { get; set; }
        public bool CriticalErrorCabShoot { get; set; }
        public GVIErrorInfo() { }
        public GVIErrorInfo(string InputData)
        {
            var i = int.Parse(InputData);
            if ((i & 0x0001) != 0) ChargerSet = true;               //зарядник не включен
            if ((i & 0x0002) != 0) ChargerReset = true;             //зарядник не выключен
            if ((i & 0x0004) != 0) MeasureSet = true;               //измеритель не включен
            if ((i & 0x0008) != 0) MeasureReset = true;             //измеритель не выключен
            if ((i & 0x0010) != 0) LeavingCapacitor = true;         //магнит замыкания конденсатора не раскоротился
            if ((i & 0x0020) != 0) ShutingCapacitor = true;         //магнит замыкания конденсатора не замкнулся
            if ((i & 0x0040) != 0) LeavingCable = true;             //магнит замыкания кабеля не раскоротился
            if ((i & 0x0080) != 0) ShutingCable = true;             //магнит замыкания кабеля не замкнулся
            if ((i & 0x0100) != 0) LeavingCap2Cab = true;           //магнит замыкания рабочего ключа не раскоротился
            if ((i & 0x0200) != 0) ShutingCap2Cab = true;           //магнит замыкания рабочего ключа не замкнулся
            if ((i & 0x0400) != 0) HiTemp = true;                   //высокая температура трансформатора 1
            if ((i & 0x0800) != 0) HiTemp2 = true;                  //высокая температура трансформатора 2
            if ((i & 0x1000) != 0) OverCap = true;                  //перенапряжение конденсатора
            if ((i & 0x2000) != 0) OverReg = true;                  //перенапряжение входное
            if ((i & 0x4000) != 0) CriticalErrorCapShoot = true;    //критическая ошибка замыкателя конденсатора
            if ((i & 0x8000) != 0) CriticalErrorCabShoot = true;    //критическая ошибка замыкателя кабеля
        }
    }
}
