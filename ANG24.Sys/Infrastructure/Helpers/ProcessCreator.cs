using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces;
using System.Diagnostics;

namespace ANG24.Sys.Infrastructure.Helpers
{
    public class WindowsProcessCreator : Executer, IProcessCreator
    {
        public event Action OnExit;
        public bool IsWorking;

        // Предположим, что единовременно может быть запущен лишь 1 процесс
        private Process process;
        private readonly ILabConfigurationService config;

        public WindowsProcessCreator(IMainControllerOperator mainControllerOperator,
                                     ILabConfigurationService config,
                                     Autofac.ILifetimeScope container) : base(container)
        {
            this.config = config;
            mainControllerOperator.ModuleStateChanged += PowerChanged;
        }
        private void PowerChanged(bool iSstarted, int retCode)
        {
            if (retCode == 0)
            {
                if (!iSstarted) // Если питание отключили
                {
                    KillProcess();
                    return;
                }
                if (iSstarted)
                {
                    StartProcess(LabState.CurrentModule switch
                    {
                        LabModule.VLF => config["VLFSoftFilePath"] as string,
                        LabModule.Bridge => config["SA7100SoftFilePath"] as string,
                        LabModule.LVMeas => config["LVMeasSoftFilePath"] as string,
                        LabModule.Meas => config["MeasSoftFilePath"] as string,
                        LabModule.SA640 => config["SA640SoftFilePath"] as string,

                        LabModule.SA540 => config["SA540SoftFilePath"] as string,
                        LabModule.SA540_1 => config["SA540SoftFilePath"] as string,
                        LabModule.SA540_3 => config["SA540SoftFilePath"] as string,
                        _ => null
                    });
                    return;
                }
            }

        }
        public bool StartProcess(string path)
        {
            if (string.IsNullOrEmpty(path)) return false; //throw new ArgumentException("Path is not valid or not defined");
            try
            {
                path = path.Replace('/', Path.DirectorySeparatorChar);
                var ar = path.Split(Path.DirectorySeparatorChar);
                var directory = path.Replace($"{ar[ar.Length - 1]}", "");
                process = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = directory,
                        FileName = path
                    }
                };
                process.Start();

                process.Exited += Exited;
                IsWorking = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при запуске программы \n " + ex.Message);
            }
            return false;
        }
        public void KillProcess()
        {
            if (process is null) return;
            if (!process.HasExited)
                process.Kill();
            process.Exited -= Exited;
            IsWorking = false;
        }
        private void Exited(object sender, EventArgs e)
        {
            OnExit?.Invoke();
            IsWorking = false;
            process.Exited -= Exited;
        }
    }
}
