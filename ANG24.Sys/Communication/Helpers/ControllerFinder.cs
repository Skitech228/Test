using ANG24.Sys.Application;
using ANG24.Sys.Application.Interfaces.Services;
using ANG24.Sys.Application.Types;
using ANG24.Sys.Application.Types.Enum;
using System.IO.Ports;

namespace ANG24.Sys.Communication.Helpers;

//класс, предназначенный для поиска контроллера различными способами
public sealed class ControllerFinder
{
    private ControllersInfo controllersInfo;
    private static SerialPort noPort = null;
    private readonly INotificationService message;
    public Port[] Ports { get; private set; }
    public ControllerFinder(INotificationService message) => this.message = message;
    public static async Task<ControllerFinder> CreateNew(INotificationService notification)
    {
        var item = new ControllerFinder(notification);
        await item.Initialize();
        return item;
    }
    private async Task Initialize()
    {
        if (File.Exists(AppFilePaths.ControllersFullPath))
        {
            try
            {
                List<Port> localPorts = new();
                controllersInfo = JsonSerializer.Deserialize<ControllersInfo>(File.OpenRead(AppFilePaths.ControllersFullPath));
                 BackwardCompatibility();

                foreach (var controller in controllersInfo.Controllers)
                {
                    if (!string.IsNullOrWhiteSpace(controller.PortName))
                        localPorts.Add(new Port
                        {
                            Name = controller.Name,
                            serialPort = new()
                            {
                                PortName = controller.PortName,
                                BaudRate = Convert.ToInt32(controller.BaudRate)
                            }
                        });
                }
                Ports = localPorts.ToArray();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                message.SendNotificationOKCancel("Не удалось найти конфигурацию контроллеров, \n " +
                    "пожалуйста, произведите поиск оборудования", null, null);
            }
        }
        else
            await InitializeNewControllersFile();
    }

    public async Task<bool> SearchPorts()
    {
        List<Port> list = new();
        try
        {
            if (Ports != null && Ports.Length != 0) await ClosePort();
            var ports = SerialPort.GetPortNames().Where(x => x != "COM1"); // COM1 - зарезервирован windos // добавить для linux
            await AddWithRandomPort(controllersInfo.Controllers.FirstOrDefault(x => x.Name == "SA540"));
            await AddWithRandomPort(controllersInfo.Controllers.FirstOrDefault(x => x.Name == "SA640"));
            foreach (var port in ports)
            {
                bool find = false;
                foreach (var controller in controllersInfo.Controllers)
                {
                    if (!controller.isChecked)
                    {
                        SerialPort localPort = new();
                        try
                        {
                            localPort.PortName = port;
                            localPort.BaudRate = controller.BaudRate;
                            localPort.ReadTimeout = 1000;
                            localPort.Open();

                            if (controller.ProtocolProtocol == "ModBus")
                            {
                                //switch (controller.Name) // Танцы с бубном
                                //{
                                //    case "GP500":
                                //        localPort.Parity = Parity.None;
                                //        localPort.ReadTimeout = 75;
                                //        localPort.WriteTimeout = 2;
                                //        byte[] bufer = new byte[] { 0x0A, 0x01 };
                                //        localPort.Write(bufer, 0, bufer.Length);
                                //        break;
                                //    default:
                                //        break;
                                //} // добавить после переработки GP500
                                byte[] data = controller.Request.Split(' ').Select(x => Convert.ToByte(x)).ToArray();
                                localPort.Write(data, 0, data.Length);
                                await Task.Delay(200);
                                var id = localPort.ReadByte();
                                localPort.DiscardInBuffer();
                                if (id == int.Parse(controller.Response))
                                {
                                    AddControllerToList();
                                    break;
                                }
                            }
                            else
                            {
                                switch (controller.Name) // Танцы с бубном
                                {
                                    case "Reflect":
                                        localPort.Write("#" + controller.Request);
                                        await Task.Delay(10);
                                        localPort.Write("#" + controller.Request);
                                        await Task.Delay(10);
                                        localPort.DtrEnable = true;
                                        await Task.Delay(100);
                                        localPort.DtrEnable = false;
                                        break;
                                    default:
                                        localPort.Write("#" + controller.Request);
                                        break;
                                }
                                await Task.Delay(100);

                                while (localPort.BytesToRead > 0)
                                {
                                    var buf_line = localPort.ReadLine();
                                    var req = buf_line.Replace('\r', ' ').Replace('\n', ' ').Trim();
                                    if (req.Contains(controller.Response))
                                    {
                                        AddControllerToList();
                                        break;
                                    }
                                }
                            }
                        }
                        catch (TimeoutException) { }
                        catch (IOException) { }
                        catch (InvalidOperationException) { }
                        localPort.Close();
                        await Task.Delay(100);
                        controller.isChecked = true;
                        if (find) break;
                        void AddControllerToList()
                        {
                            controller.PortName = localPort.PortName;
                            list.Add(new Port
                            {
                                Name = controller.Name,
                                serialPort = localPort
                            });
                            find = true;
                        }
                    }
                }
            }
            if (list.Count == 0)
                message.SendNotificationOK("Не удалось найти порты. Проверьте подключены ли контроллеры", null);
            Ports = list.ToArray();
            Save();
            return true;

        }
        catch
        {
            return false;
        }
        async Task AddWithRandomPort(ControllersInfo.Controller controller)
        {
            await Task.Delay(100); // Защита для рандома в условиях NET.Framework
            if (controller == null) return;
            list.Add(new Port
            {
                Name = controller.Name,
                serialPort = new SerialPort() { PortName = $"COM{Random.Shared.Next(1000, 9999)}" }
            });
        }
    }
    private async Task ClosePort()
    {
        foreach (var port in Ports)
            if (port.serialPort.IsOpen)
            {
                port.serialPort.Close();
                await Task.Delay(100);
            }
    }
    public ref SerialPort FindPort(string name)
    {
        if (Ports != null && Ports.Length != 0)
            for (int i = 0; i < Ports.Length; i++)
                if (Ports[i].Name == name)
                    return ref Ports[i].serialPort;

        return ref noPort;
    }
    public void Save()
    {
        foreach (var port in Ports)
        {
            var controller = controllersInfo.Controllers.FirstOrDefault(c => c.Name == port.Name);
            if (controllersInfo is not null)
                controller.PortName = port.serialPort.PortName;
        }
        File.WriteAllText(AppFilePaths.ControllersFullPath, JsonSerializer.Serialize(controllersInfo, new JsonSerializerOptions { WriteIndented = true }));
    }

    private async Task InitializeNewControllersFile()
    {
        controllersInfo = new ControllersInfo
        {
            Controllers = new List<ControllersInfo.Controller>
            {
                new ControllersInfo.Controller
                {
                    Name = LabController.MainController.ToString(),
                    Response = "AngstremLabController"
                },
                new ControllersInfo.Controller
                {
                    Name = LabController.Compensation.ToString(),
                    Response = "CompensationSystem_4bits"
                },
                new ControllersInfo.Controller
                {
                    Name = LabController.VoltageSyncronizer.ToString(),
                    Response = "Voltage Regulator Synchronizer"
                },
                new ControllersInfo.Controller
                {
                    Name = LabController.PowerSelector.ToString(),
                    Response = "Power Selector"
                },
                new ControllersInfo.Controller
                {
                    Name = LabController.VLF.ToString(),
                    Response = "VLF EVR v1.0"
                },
                new ControllersInfo.Controller
                {
                    Name = LabController.BridgeCommutator.ToString(),
                    BaudRate = 115200,
                    Response = "Bridge commutator v1.0"
                },
                new ControllersInfo.Controller
                {
                    Name = LabController.Reflect.ToString(),
                    BaudRate = 115200,
                    Request = "R120#",
                    Response = "Command done"
                },
                new ControllersInfo.Controller
                {
                    Name = LabController.SA540.ToString(),
                    BaudRate = 115200,
                    ProtocolProtocol = "ModBus",
                    Request = "1 255 5 0 251",
                    Response = "1"
                },
                new ControllersInfo.Controller
                {
                    Name = LabController.SA640.ToString(),
                    BaudRate = 115200,
                    ProtocolProtocol = "ModBus",
                    Request = "2 31 5 0 24",
                    Response = "2"
                },
                new ControllersInfo.Controller
                {
                    Name = LabController.GP500.ToString(),
                    BaudRate = 19200,
                    ProtocolProtocol = "ModBus",
                    Request = "2 31 5 0 24",
                    Response = "2"
                }
            }
        };
        if (!await BackwardCompatibility())
            await SearchPorts();
    }
    private async Task<bool> BackwardCompatibility()
    {
        return await Task.Factory.StartNew(() =>
        {
            XElement ports = null;
            if (File.Exists(AppFilePaths.OldPortsFullPath)) // загрузка старого файла
            {
                ports = XDocument.Load(AppFilePaths.OldPortsFullPath).Element("Ports");
                foreach (var item in ports.Elements("Port")) // запись данных
                    controllersInfo.Controllers
                        .FirstOrDefault(x => x.Name == item.Element("Name").Value)
                        .PortName = item.Element("Com").Value;

                Save();
                //после поглащения - перенос в папку Temp
                File.Move(AppFilePaths.OldPortsFullPath, AppFilePaths.TempPath);
                return true;
            }
            return false;
        });
    }
}
public struct Port
{
    public string Name;
    public SerialPort serialPort;
}
