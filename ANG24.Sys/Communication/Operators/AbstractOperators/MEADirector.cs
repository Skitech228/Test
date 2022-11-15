using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;
using ANG24.Sys.Communication.Interfaces;
using Autofac;
using System.IO.Ports;

namespace ANG24.Sys.Communication.Operators.AbstractOperators;

public class MEADirector : IMainControllerOperator, ISerialPortOperator
{
    private IMainControllerOperator oper;
    private readonly ILifetimeScope container;
    public void SetPort(SerialPort port)
    {
        if (oper is ISerialPortOperator) (oper as ISerialPortOperator).SetPort(port);
    }
    public MEADirector(ILifetimeScope container)
    {
        ChangeOperator(container.ResolveNamed<IMainControllerOperator>("Real"));
        LabState.OnSimulationOn += LabState_OnSimulationOn;
        this.container = container;
    }
    private void LabState_OnSimulationOn()
    {
        ChangeOperator(container.ResolveNamed<IMainControllerOperator>("Simulation"));
        LabState.OnSimulationOn -= LabState_OnSimulationOn;
    }
    public void ChangeOperator(IMainControllerOperator oper)
    {
        if (this.oper != null)
        {
            this.oper.OnDataReceived -= OnMainDataReceived;
            this.oper.OnData -= OnMainDataReceivedd;
            this.oper.ModuleStateChanged -= OnMainStateChanged;
        }
        this.oper = oper;
        this.oper.OnDataReceived += OnMainDataReceived;
        this.oper.OnData += OnMainDataReceivedd;
        this.oper.ModuleStateChanged += OnMainStateChanged;
    }
    private void OnMainDataReceived(MEAData data) => OnDataReceived?.Invoke(data);
    private void OnMainDataReceivedd(IControllerOperatorData data) => OnData?.Invoke(data);
    private void OnMainStateChanged(bool b, int i) => ModuleStateChanged?.Invoke(b, i);
    #region Redirection
    public string Name => oper.Name;
    public bool Connected => oper.Connected;
    public IMethodExecuter CurrentProcess => oper.CurrentProcess;
    public event Action<bool, int> ModuleStateChanged;
    public event Action<MEAData> OnDataReceived;
    public event Action<IControllerOperatorData> OnData;
    public bool Connect() => oper.Connect();
    public bool Connect(int AttemptCount) => oper.Connect(AttemptCount);
    public void Disconnect() => oper.Disconnect();
    public void EmergencyStop() => oper.EmergencyStop();
    public void EnterKeys(int key1, int key2, int key3, int key4) => oper.EnterKeys(key1, key2, key3, key4);
    public FazeType GetFazeType() => oper.GetFazeType();
    public void GetTrial() => oper.GetTrial();
    public void RunLab() => oper.RunLab();
    public void PowerOn(IEnumerable<PowerTargets> powerTargets) => oper.PowerOn(powerTargets);
    public void PowerOff() => oper.PowerOff();
    public void RegulatorDisable() => oper.RegulatorDisable();
    public void RegulatorEnable() => oper.RegulatorEnable();
    public void ResetModule() => oper.ResetModule();
    public void SetCommand(string message) => oper.SetCommand(message);
    public void SetModule(LabModule module) => oper.SetModule(module);
    public void SetModuleAndDontOffCurrent(LabModule module) => oper.SetModuleAndDontOffCurrent(module);
    public void SetPower(BurnPower power) => oper.SetPower(power);
    public void SetStep(int step) => oper.SetStep(step);
    public void StartQueue() => oper.StartQueue();
    public void StopQueue() => oper.StopQueue();
    public void VoltageDown() => oper.VoltageDown();
    public void VoltageUp() => oper.VoltageUp();
    #region GVI
    public void GVI_FireStop() => oper.GVI_FireStop();
    public void GVI_LineFire(int count) => oper.GVI_LineFire(count);
    public void GVI_OneFire() => oper.GVI_OneFire();
    public void GVI_SetDelay(int delay) => oper.GVI_SetDelay(delay);
    #endregion
    #region SVI
    public void AddPointACCalib(int serial, int voltage) => oper.AddPointACCalib(serial, voltage);
    public void AddPointDCCalib(int serial, int voltage) => oper.AddPointDCCalib(serial, voltage);
    public void DeleteAllPointsCalib(int serial) => oper.DeleteAllPointsCalib(serial);
    public void EditPointACCalib(int serial, int index, int voltage) => oper.EditPointACCalib(serial, index, voltage);
    public void EditPointDCCalib(int serial, int index, int voltage) => oper.EditPointDCCalib(serial, index, voltage);
    public void GetDateCalib() => oper.GetDateCalib();
    public void GetFirmwareRev() => oper.GetFirmwareRev();
    public void GetKoefACCalib(int serial, int index) => oper.GetKoefACCalib(serial, index);
    public void GetKoefDCCalib(int serial, int index) => oper.GetKoefDCCalib(serial, index);
    public void GetLevelZeroPointCalib() => oper.GetLevelZeroPointCalib();
    public void GetPointCountCalib(int serial) => oper.GetPointCountCalib(serial);
    public void GetSerialNumCalib() => oper.GetSerialNumCalib();
    public void RemovePointACCalib(int serial, int index) => oper.RemovePointACCalib(serial, index);
    public void RemovePointDCCalib(int serial, int index) => oper.RemovePointDCCalib(serial, index);
    public void SetDateCalib(int year, int mouth, int day) => oper.SetDateCalib(year, mouth, day);
    public void SetLevelZeroPointCalib(int delta) => oper.SetLevelZeroPointCalib(delta);
    public void SetSerialNumCalib(int serial) => oper.SetSerialNumCalib(serial);
    #endregion
    #region Autoburn
    public void SetAutoBurning(bool workingMode) => oper.SetAutoBurning(workingMode);
    public void SetStepBackBlocked(bool blocked) => oper.SetStepBackBlocked(blocked);
    public async Task<bool> SetNextStep() => await oper.SetNextStep();
    public async Task<bool> SetPreviousStep() => await oper.SetPreviousStep();
    #endregion
    #endregion
}
