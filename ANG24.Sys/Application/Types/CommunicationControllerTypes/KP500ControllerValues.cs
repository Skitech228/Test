namespace ANG24.Sys.Application.Types.CommunicationControllerTypes
{
    public class KP500ControllerValues
    {
        /// <summary>
        /// Состояние контроллера (0 - Нет подключения, 1 - Подключен, 2 - Идет подключение, 3 - Потеря связи, 4 - Ошибка подключения, 5 - Идет переключение, 6 - Ошибка переключения)
        /// </summary>
        public int State { get; set; }
        public int StateMode { get; set; }
        /// <summary>
        /// Текущий экран
        /// </summary>
        public int SelectScreen { get; set; }
        public string ModeImp { get; set; }
        public int ModeImpState { get; set; }
        public string Frequency { get; set; }
        public int FrequencyState { get; set; }
        public string Matching { get; set; }
        public int MatchingState { get; set; }
        public string Resistance { get; set; }
        public double ResistanceValue { get; set; }
        public int ResistanceState { get; set; }
        public string Overload { get; set; }
        public double OverloadValue { get; set; }
        public int OverloadState { get; set; }
        public string Buffer { get; set; }
        public double Voltage { get; set; }
        public double Current { get; set; }
        public double Ktr { get; set; }
        public double PercentVoltage { get; set; }
        public double PercentCurrent { get; set; }

        public int FreqCurrent { get; set; }
        public int Freq1 { get; set; }
        public int Freq2 { get; set; }
        public int Freq3 { get; set; }

        public string MessageError { get; set; }
    }
}
