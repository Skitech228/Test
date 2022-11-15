using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace ANG24.Sys.Communication.Operators.ControllerOperators;

public sealed class ThermoVisionControlManager : IThermoVisionControlManager
{
    private const string msg1 = "Calling HTPA series devices";
    private const string BindMsg = "Bind HTPA series device";
    private const string ReleaseMsg = "x Release HTPA series device";
    private const string StopMsg = "x";
    private const string StartReciveData = "K";
    private const int PORT = 30444;
    private readonly UdpClient client = new UdpClient();

    //UdpClient client = new UdpClient(30444, AddressFamily.InterNetwork);
    private IPEndPoint RemoteIpEndPoint = new(IPAddress.Any, PORT);
    private readonly Thread MainLoop;

    public event Action<UDPThermoPacket> DataReceived;
    public event Action<ThermoData> OnDataReceived;
    public event Action<IControllerOperatorData> OnData;

    public List<int> Exclusions = new();
    public bool Connected { get; set; } = false;
    public bool Binded { get; set; } = false;
    public bool DataCollectingStart { get; set; } = false;

    public ThermoVisionControlManager()
    {
        OnDataReceived?.Invoke(null);
        OnDataReceived += (data) => OnData?.Invoke(data);
        client.EnableBroadcast = true;
        client.Client.ReceiveTimeout = 500;
        MainLoop = new Thread(state =>
        {
            Thread.Sleep(1000);
            while (DataCollectingStart)
            {

                int counter = 0;
                byte[] buffer = new byte[1283 * 10];
                while (counter != 10)
                {
                    byte[] recived_data = new byte[1];
                    try
                    {
                        recived_data = client.Receive(ref RemoteIpEndPoint);

                        //Console.WriteLine(Encoding.ASCII.GetString(recived_data));
                        Array.Copy(recived_data, 0, buffer, 1283 * counter, recived_data.Length);
                        counter = recived_data[0];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(Encoding.ASCII.GetString(recived_data));
                        Console.WriteLine(ex.Message);
                    }
                }

                if (buffer.Length > 0)
                {
                    var dat = new UDPThermoPacket(buffer);
                    DataReceived?.Invoke(dat);
                    counter = 0;
                }
            }
        })
        {
            IsBackground = true
        };
    }
    public bool FindAndConnect()
    {
        try
        {
            client.Client.Bind(new IPEndPoint(IPAddress.Parse(GetLocalIpAddress()), 80));
            //var from = new IPEndPoint(IPAddress.Any, PORT);

            var data = Encoding.UTF8.GetBytes("Calling HTPA series devices");
            var targetPoint = new IPEndPoint(IPAddress.Broadcast, PORT);
            client.Send(data, data.Length, targetPoint);
            var datagram = client.Receive(ref RemoteIpEndPoint);
            var data_string = Encoding.ASCII.GetString(datagram);

            if (data_string.Contains("HTPA series responsed!"))
            {
                Console.WriteLine($"Find: {RemoteIpEndPoint.Address.ToString()}");
                //Console.ReadKey();
                //client.Client.ReceiveTimeout = 10000;
                client.Connect(RemoteIpEndPoint);
                Connected = true;
                return true;
            }
            else
            {
                Console.WriteLine($"not right resp");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"not resp: {ex.Message}");
        }
        return false;
    }
    public static string GetLocalIpAddress()
    {
        UnicastIPAddressInformation mostSuitableIp = null;

        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var network in networkInterfaces)
        {
            if (network.OperationalStatus != OperationalStatus.Up)
                continue;

            var properties = network.GetIPProperties();

            if (properties.GatewayAddresses.Count == 0)
                continue;

            foreach (var address in properties.UnicastAddresses)
            {
                if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                    continue;

                if (IPAddress.IsLoopback(address.Address))
                    continue;

                if (!address.IsDnsEligible)
                {
                    if (mostSuitableIp == null)
                        mostSuitableIp = address;
                    continue;
                }

                // The best IP is the IP got from DHCP server
                if (address.PrefixOrigin != PrefixOrigin.Dhcp)
                {
                    if (mostSuitableIp == null || !mostSuitableIp.IsDnsEligible)
                        mostSuitableIp = address;
                    continue;
                }

                return address.Address.ToString();
            }
        }

        return mostSuitableIp != null
            ? mostSuitableIp.Address.ToString()
            : "";
    }
    public bool BindDevice()
    {
        if (Connected)
        {
            if (!Binded)
            {
                try
                {
                    var msg_bytes = Encoding.ASCII.GetBytes(BindMsg);
                    client.Send(msg_bytes, msg_bytes.Length);
                    var responce_bytes = client.Receive(ref RemoteIpEndPoint);
                    var responce = Encoding.ASCII.GetString(responce_bytes);
                    if (responce.Contains("HW Filter is"))
                    {
                        Console.WriteLine("Device Binded");
                        Binded = true;
                        return true;
                    }
                    else if (responce.Contains("Device already bound"))
                    {
                        Console.WriteLine("Device Binded (was bounded)");
                        Binded = true;
                        return true;
                    }
                    else return false;
                }
                catch (SocketException)
                {
                    return false;
                }
            }
            else return true;
        }
        else return false;
    }
    public void StartCollect()
    {

        try
        {
            var msg_bytes = Encoding.ASCII.GetBytes("K");
            client.Send(msg_bytes, msg_bytes.Length);
            Thread.Sleep(10);
            var msg_responce_bytes = client.Receive(ref RemoteIpEndPoint);
            var msg_responce = Encoding.ASCII.GetString(msg_responce_bytes);
            if (msg_responce.Contains("START"))
            {
                Console.WriteLine("Device Collect Data Started");
                Thread.Sleep(1000);
                DataCollectingStart = true;
                MainLoop.Start();
            }
            else if (msg_responce_bytes.Length == 1283)
            {
                Console.WriteLine("Device Collect Data Started");
                Thread.Sleep(1000);
                DataCollectingStart = true;
                MainLoop.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    public void StopCollect(bool Exit = false)
    {
        if (!Exit)
        {
            try
            {
                if (client.Client.Connected)
                {
                    var msg_bytes = Encoding.ASCII.GetBytes(StopMsg);
                    client.Send(msg_bytes, msg_bytes.Length);
                    var msg_responce_bytes = client.Receive(ref RemoteIpEndPoint);
                    var msg_responce = Encoding.ASCII.GetString(msg_responce_bytes);
                    if (msg_responce.Contains("STOP"))
                    {
                        DataCollectingStart = false;
                    }
                }
            }
            catch (SocketException sock)
            {
                Console.WriteLine(sock.Message);
            }
        }
        else
        {
            try
            {
                if (client.Client.Connected)
                {
                    var msg_bytes = Encoding.ASCII.GetBytes(ReleaseMsg);
                    client.Send(msg_bytes, msg_bytes.Length);
                    //var msg_responce_bytes = client.Receive(ref RemoteIpEndPoint);
                    //var msg_responce = Encoding.ASCII.GetString(msg_responce_bytes);
                    DataCollectingStart = false;
                }
            }
            catch (SocketException sock)
            {
                Console.WriteLine(sock.Message);
            }
        }
    }
    public void SetExclusions(int[] indexes)
    {
        foreach (var item in indexes)
        {
            Exclusions.Add(item);
        }
        Exclusions = Exclusions.Distinct().ToList();
        Exclusions.Sort();
    }

    public bool Connect()
    {
        throw new NotImplementedException();
    }
    public bool Connect(int AttemptCount)
    {
        throw new NotImplementedException();
    }
    public void Disconnect()
    {
        throw new NotImplementedException();
    }
    public void StartQueue()
    {
        throw new NotImplementedException();
    }
    public void StopQueue()
    {
        throw new NotImplementedException();
    }
    public void EmergencyStop()
    {
        throw new NotImplementedException();
    }
    public string Name => Application.Types.Enum.LabController.ThermoVision.ToString();
    public IMethodExecuter CurrentProcess => throw new NotImplementedException();
}
