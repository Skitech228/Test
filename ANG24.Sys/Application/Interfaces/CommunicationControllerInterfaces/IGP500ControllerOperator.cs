using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface IGP500ControllerOperator : IControllerOperator<GP500Data>
    {
        event Action<KP500ControllerValues> OnKP500DataReceived;
        void Start();
        void Stop();
        //ControllerState GetState();

        void DownButtonPlus();
        void UpButtonPlus();
        void DownButtonMinus();
        void UpButtonMinus();
        void ButtonReset();
        void ButtonMode();
        void ButtonMatching();
        void SetManualFrequency();
        void SaveFrequency();

        void SetFrequency(KP500FrequencyEnum frequency);
        void SetResistance(KP500Resistance resist);
        void SetModeImp(KP500ModeImp modeImp);
        void SetModeMCH2(KP500SetMCH2 setMCH2);
        void OpenSetFrequency();
        void SetMainMenu();
        void ButtonResetAll();
    }
}
