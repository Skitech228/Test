using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;

namespace Application.Types.CommunicationControllerTypes.MEADataModels
{
    public delegate void StatePositionEventHandler(bool StateCorrect);
    public static class HVPositions
    {
        public static HVSwitchState HVPosition1;
        public static HVSwitchState HVPosition2;
        public static HVSwitchState HVPosition3;
        public static event StatePositionEventHandler StateChecked;
        public static bool ShowMessage = false;

        public static bool checkStateForSinglePosition(HVSwitchState currentState)
        {
            bool res = false;

            List<HVSwitchState> states = new List<HVSwitchState> { HVPosition1, HVPosition2, HVPosition3 };

            if (states.Contains(currentState) && states.FindAll(delegate (HVSwitchState hV) { return hV == HVSwitchState.Ground; }).Count == 2)
            {
                res = true;
            }
            else
            {
                res = false;
                ShowMessage = true;
            }
            StateChecked?.Invoke(res);
            return res;
        }

        public static bool checkStateForSomePosition(HVSwitchState currentState)
        {
            bool res = false;
            List<HVSwitchState> states = new List<HVSwitchState> { HVPosition1, HVPosition2, HVPosition3 };
            if (states.Contains(currentState))
            {
                res = true;
            }
            else
            {
                res = false;
                ShowMessage = true;
            }
            StateChecked?.Invoke(res);
            return res;
        }
    }
}
