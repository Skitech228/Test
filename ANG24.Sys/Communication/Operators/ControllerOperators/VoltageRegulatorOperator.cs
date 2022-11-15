using ANG24.Sys.Communication.Operators.AbstractOperators;

namespace ANG24.Sys.Communication.Operators.ControllerOperators
{
    public sealed class VoltageRegulatorOperator : StringSerialControllerOperator<VoltageRegulatorData>, IVoltageRegulatorOperator
    {
        public override event Action<VoltageRegulatorData> OnDataReceived;
        public VoltageRegulatorOperator()
        {
            Name = Application.Types.Enum.LabController.VLF.ToString();
            AutoLogging = false;
        }

        private int dPoints; //количество точек для увеличения значения напряжения на 1в
        private double v1;
        private double v2;

        public void GetdPoints()
        {
            SetVoltageLevel(50);
            Thread.Sleep(1000);
            v1 = VoltageRegulatorData.RMS1;
            SetVoltageLevel(100);
            Thread.Sleep(1000);
            v2 = VoltageRegulatorData.RMS1;
            dPoints = (int)(50 / (v2 - v1));
            SetVoltageLevel(0);
            Thread.Sleep(100);
            SetCurentLimitPercent(3);
        }

        private void VoltageChange(bool up)
        {
            if (up)
            {
                Thread.Sleep(100);

                //get level
                //save level value
                //increment level value + 50...
            }
            else
            {
                //get level
                //save level value
                //decrement level value - 50...
            }
        }

        public void Stop() => SetCommandPriority("stop");
        public void GetLimit() => SetCommandPriority("GET_BREAK");
        public void GetVoltageLevel() => SetCommandPriority("GET_LEVEL");
        public void SetVoltageLevel(int level)
        {
            if (level < 0 || level > 1982) throw new ParameterException();
            SetCommandPriority($"SET_LEVEL:{level}");
        }
        public void SetCurentLimitPercent(int percent)
        {
            if (percent < 0 || percent > 4) throw new ParameterException();
            SetCommandPriority($"BREAK_LIM:{percent}");
        }
        public void SetVoltageForTime(int level, int time)
        {
            if (level < 0 || level > 1982) throw new ParameterException();
            SetCommandPriority($"RAMP:{level},{time}");
        }
        public void StartSin(double f)
        {
            if (f <= 0 || f > 0.1) throw new ParameterException();
            SetCommandPriority($"START_SIN:{1 / f}");
        }
        public void StartSinWithVoltLevel(double f, int level)
        {
            if (level < 0 || level > 100) throw new ParameterException();
            if (f <= 0 || f > 0.1) throw new ParameterException();
            SetCommandPriority($"START_SIN_LEVEL:{1 / f},{level}");
        }
        public void SetMagnitAHoldLevel(int level)
        {
            if (level < 0 || level > 999) throw new ParameterException();
            SetCommandPriority($"modePWMa:{level}");
        }
        public void SetMagnitBHoldLevel(int level)
        {
            if (level < 0 || level > 999) throw new ParameterException();
            SetCommandPriority($"modePWMb:{level}");
        }
        public void SetMagnitATriggerLevel(int level)
        {
            if (level < 0 || level > 999) throw new ParameterException();
            SetCommandPriority($"startPWMa:{level}");
        }
        public void SetMagnitBTrigerLevel(int level)
        {
            if (level < 0 || level > 999) throw new ParameterException();
            SetCommandPriority($"startPWMb:{level}");
        }
        public void SetMagnitATriggerTime(int time) => SetCommandPriority($"timePWMa:{time}");
        public void SetMagnitBTriggerTime(int time) => SetCommandPriority($"timePWMb:{time}");

        private void SendCommand(string command) => SetCommandPriority($"#{command};");
        protected override void CommandBroker(VoltageRegulatorData data)
        {
            OnDataReceived?.Invoke(data);
        }
    }
}
