using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;

namespace ANG24.Sys.Application
{
    public class LabState
    {
        // мм, статический евент...
        public static event Action? OnSimulationOn;
        // до подключения к mainController мы условно в симуляции
        private static bool isSimulationOn = true;

        public static LabModule CurrentModule { get; set; }
        public static bool IsDebugModeOn { get; set; }
        public static bool IsSimulationOn
        {
            get => isSimulationOn;
            set
            {
                isSimulationOn = value;
                if (value)
                    OnSimulationOn?.Invoke();
            }
        }
    }
}
