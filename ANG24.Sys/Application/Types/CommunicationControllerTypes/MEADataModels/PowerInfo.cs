using Application.Types.CommunicationControllerTypes.MEADataModels;

namespace ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels
{
    public class PowerInfo
    {
        public bool RNParking { get; set; }
        public bool RNManualEnable { get; set; }
        public bool HVMEnable { get; set; }
        public bool GVIEnable { get; set; }
        public bool BurnEnable { get; set; }
        public bool GP500Enable { get; set; }
        public bool LVMEnable { get; set; }
        public bool MeasEnable { get; set; }
        public bool JoinBurnEnable { get; set; }
        /// <summary>
        /// Рабочая земля от MKZ испытаний
        /// </summary>
        public bool KM1_MKZEnable { get; set; }
        public bool IDMEnable { get; set; }
        public bool ProtectedDrosselEnable { get; set; }
        /// <summary>
        /// Рабочая земла от MKZ ГП-500
        /// </summary>
        public bool KM3_MKZEnable { get; set; }
        public bool MVKUp { get; set; }
        public bool MSKEnable { get; set; }
        public bool SVIPowerEnable { get; set; }
        /// <summary>
        /// Указывает на что, что включена защита на 100(200)mA, если показывает false -- указывает на то, что включена защита 20mA
        /// </summary>
        public bool CurrentProtection100Enable { get; set; }
        public bool VREnable { get; set; }
        public bool BridgeEnable { get; set; }
        public bool VLFEnable { get; set; }
        public bool VoltageUpFlag { get; set; }
        public bool VoltageDownFlag { get; set; }
        public ModulePowerInfo ModulePower => new ModulePowerInfo(this);
        public bool SA540Enable { get; set; }
        public bool SA640Enable { get; set; }
        public bool Tangent2000Enable { get; set; }

        public PowerInfo(string InputData)
        {
            uint i = 0;
            var suc = uint.TryParse(InputData, out i);
            if (suc)
            {
                if ((i & 0x01) != 0) VoltageUpFlag = true;
                if ((i & 0x02) != 0) VoltageDownFlag = true;
                if ((i & 0x80) != 0) RNParking = true;
                if ((i & 0x20) != 0) RNManualEnable = true;
                if ((i & 0x00010000) != 0) HVMEnable = true;
                if ((i & 0x00020000) != 0) GVIEnable = true;
                if ((i & 0x00040000) != 0) BurnEnable = true;
                if ((i & 0x00080000) != 0) GP500Enable = true;
                if ((i & 0x00100000) != 0) LVMEnable = true;
                if ((i & 0x00200000) != 0) MeasEnable = true;
                if ((i & 0x00400000) != 0) JoinBurnEnable = true;
                if ((i & 0x00800000) != 0) VREnable = true;
                if ((i & 0x01000000) != 0) KM1_MKZEnable = true;
                //if ((i & 0x02000000) != 0) IDMEnable = true;
                IDMEnable = false;
                if ((i & 0x04000000) != 0) ProtectedDrosselEnable = true;
                if ((i & 0x08000000) != 0) KM3_MKZEnable = true;
                if ((i & 0x00001000) != 0) MVKUp = true;
                if ((i & 0x00002000) != 0) MSKEnable = true;
                if ((i & 0x00004000) != 0) SVIPowerEnable = true;
                if ((i & 0x00008000) != 0) CurrentProtection100Enable = true;
                if ((i & 0x10000000) != 0) BridgeEnable = true;
            }
            //ModulePower = new ModulePowerInfo(this);

        }
        public PowerInfo()
        {
            //ModulePower = new ModulePowerInfo(this);
        }
    }
}
