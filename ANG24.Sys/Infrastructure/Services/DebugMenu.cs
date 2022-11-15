using ANG24.Sys.Infrastructure.Helpers;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;
using ANG24.Sys.Application.Types.ServiceTypes;
using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;

namespace ANG24.Sys.Infrastructure.Services;
public sealed class DebugMenu : Executer, IDebugMenu
{
    public event Action<string, bool> OnFlagChanged;
    private DebugFlags flags;
    //private readonly Type Debug = typeof(DebugFlags);
    public DebugMenu(IMainControllerOperator oper, Autofac.ILifetimeScope container) : base(container) => oper.OnDataReceived += OnDataReceived;
    private void OnDataReceived(MEAData data)
    {
        if (data.Module != LabModule.NoMode)
        {
            flags.VoltageUpFlag = data.PowerInfoMessage?.VoltageUpFlag ?? false;
            flags.VoltageDownFlag = data.PowerInfoMessage?.VoltageDownFlag ?? false;

            flags.PI_RNParking = data.PowerInfoMessage?.RNParking ?? false;
            flags.PI_RNManualEnable = data.PowerInfoMessage?.RNManualEnable ?? false;
            flags.PI_HVMEnable = data.PowerInfoMessage?.HVMEnable ?? false;
            flags.PI_GVIEnable = data.PowerInfoMessage?.GVIEnable ?? false;
            flags.PI_BurnEnable = data.PowerInfoMessage?.BurnEnable ?? false;
            flags.PI_GP500Enable = data.PowerInfoMessage?.GP500Enable ?? false;
            flags.PI_LVMEnable = data.PowerInfoMessage?.LVMEnable ?? false;
            flags.PI_MeasEnable = data.PowerInfoMessage?.MeasEnable ?? false;
            flags.PI_JoinBurnEnable = data.PowerInfoMessage?.JoinBurnEnable ?? false;
            flags.PI_VREnable = data.PowerInfoMessage?.VREnable ?? false;
            flags.PI_KM1_MKZEnable = data.PowerInfoMessage?.KM1_MKZEnable ?? false;
            flags.PI_IDMEnable = data.PowerInfoMessage?.IDMEnable ?? false;
            flags.PI_ProtectedDrosselEnable = data.PowerInfoMessage?.ProtectedDrosselEnable ?? false;
            flags.PI_KM3_MKZEnable = data.PowerInfoMessage?.KM3_MKZEnable ?? false;
            flags.PI_MVKUp = data.PowerInfoMessage?.MVKUp ?? false;
            flags.PI_MSKEnable = data.PowerInfoMessage?.MSKEnable ?? false;
            flags.PI_SVIPowerEnable = data.PowerInfoMessage?.SVIPowerEnable ?? false;
            flags.PI_CurrentProtection100Enable = data.PowerInfoMessage?.CurrentProtection100Enable ?? false;
            flags.PI_BridgeEnable = data.PowerInfoMessage?.BridgeEnable ?? false;

            flags.MKZ_LeftDoor = data.MKZState?.DoorLeft ?? false;
            flags.MKZ_RightDoor = data.MKZState?.DoorRight ?? false;
            flags.MKZ_Stop = data.MKZState?.Stop ?? false;
            flags.MKZ_SafeKey = data.MKZState?.SafeKey ?? false;
            flags.MKZ_DangerousPotencial = data.MKZState?.DangerousPotencial ?? false;
            flags.MKZ_Ground = data.MKZState?.Ground ?? false;
        }
    }
    public void ChangeAllFlags(bool state) => OnFlagChanged?.Invoke("all", state);
    public void ChangeFlag(string flag, bool state) => OnFlagChanged?.Invoke(flag, state);

    public DebugFlags GetCurrentState() => flags;
}