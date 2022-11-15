namespace ANG24.Sys.Application.Types.ServiceTypes;

public struct DebugFlags
{
    public bool VoltageUpFlag { get; set; }
    public bool VoltageDownFlag { get; set; }

    public bool PI_RNParking { get; set; }
    public bool PI_RNManualEnable { get; set; }
    public bool PI_HVMEnable { get; set; }
    public bool PI_GVIEnable { get; set; }
    public bool PI_BurnEnable { get; set; }
    public bool PI_GP500Enable { get; set; }
    public bool PI_LVMEnable { get; set; }
    public bool PI_MeasEnable { get; set; }
    public bool PI_JoinBurnEnable { get; set; }
    public bool PI_KM1_MKZEnable { get; set; }
    public bool PI_KM3_MKZEnable { get; set; }
    public bool PI_IDMEnable { get; set; }
    public bool PI_ProtectedDrosselEnable { get; set; }
    public bool PI_MVKUp { get; set; }
    public bool PI_MSKEnable { get; set; }
    public bool PI_SVIPowerEnable { get; set; }
    public bool PI_CurrentProtection100Enable { get; set; }
    public bool PI_VREnable { get; set; }
    public bool PI_BridgeEnable { get; set; }
    public bool PI_VLFEnable { get; set; }
    public bool PI_SA540Enable { get; set; }
    public bool PI_SA640Enable { get; set; }
    public bool PI_Tangent2000Enable { get; set; }

    public bool MKZ_LeftDoor { get; set; }
    public bool MKZ_RightDoor { get; set; }
    public bool MKZ_Stop { get; set; }
    public bool MKZ_SafeKey { get; set; }
    public bool MKZ_DangerousPotencial { get; set; }
    public bool MKZ_Ground { get; set; }
    public bool IsAutoVoltage { get; set; }
}
