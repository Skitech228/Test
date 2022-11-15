using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Types.CommunicationControllerTypes;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Types.CommunicationControllerTypes.MEADataModels.Enum;

namespace ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces
{
    public interface IMainControllerOperator : IControllerOperator<MEAData>, IGVIOperator, ISVIOperator, IAutoburner
    {
        /// <summary>
        /// bool:
        /// true - started | false - stoped
        /// int  - return code
        /// </summary>
        event Action<bool, int> ModuleStateChanged;
        void SetCommand(string message);
        void SetModule(LabModule module);
        void SetModuleAndDontOffCurrent(LabModule module);
        void ResetModule();
        void PowerOn(IEnumerable<PowerTargets> powerTargets);
        /// <summary>
        /// Запуск лабаратории (или остановка)
        /// </summary>
        void RunLab();
        void PowerOff();
        void VoltageUp();
        void VoltageDown();
        /// <summary>
        /// Установка ступени прожига
        /// </summary>
        /// <param name="step">Ступень 0 - 7</param>
        void SetStep(int step);
        /// <summary>
        /// Установка мощности прожига
        /// </summary>
        /// <param name="power">Мощность 50 \ 100%</param>
        void SetPower(BurnPower power);
        /// <summary>
        /// Определяет фазность лаборатории(Rev 1.1)
        /// </summary>
        /// <returns></returns>
        FazeType GetFazeType();
        void RegulatorDisable();
        void RegulatorEnable();
        void GetTrial();
        void EnterKeys(int key1, int key2, int key3, int key4);
    }
    public interface ISVIOperator : IControllerOperator<MEAData>
    {
        void GetDateCalib();
        void SetDateCalib(int year, int mouth, int day);
        void GetSerialNumCalib();
        void SetSerialNumCalib(int serial);
        void GetLevelZeroPointCalib();
        void SetLevelZeroPointCalib(int delta);
        void AddPointDCCalib(int serial, int voltage);
        void AddPointACCalib(int serial, int voltage);
        void DeleteAllPointsCalib(int serial);
        void GetPointCountCalib(int serial);
        void RemovePointDCCalib(int serial, int index);
        void RemovePointACCalib(int serial, int index);
        void EditPointDCCalib(int serial, int index, int voltage);
        void EditPointACCalib(int serial, int index, int voltage);
        void GetKoefDCCalib(int serial, int index);
        void GetKoefACCalib(int serial, int index);
        void GetFirmwareRev();
    }
    public interface IGVIOperator : IControllerOperator<MEAData>
    {
        void GVI_OneFire();
        void GVI_LineFire(int count);
        void GVI_FireStop();
        void GVI_SetDelay(int delay);
    }
    public interface IAutoburner
    {
        /// <summary>
        /// Включить\выключить автопрожиг
        /// </summary>
        /// <param name="workingMode"> 0 - выключить; 1 - включить</param>
        void SetAutoBurning(bool workingMode);
        void SetStepBackBlocked(bool blocked);
        Task<bool> SetNextStep();
        Task<bool> SetPreviousStep();
    }
}
