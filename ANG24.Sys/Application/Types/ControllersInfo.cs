namespace ANG24.Sys.Application.Types
{
    public class ControllersInfo
    {
        public IList<Controller> Controllers { get; set; } = new List<Controller>();
        public class Controller
        {
            [NonSerialized]
            public bool isChecked; // не для сериализации
            public string Name { get; set; } = "UnknownController";
            public int BaudRate { get; set; } = 9600;
            public string ProtocolProtocol { get; set; } = "Normal";
            public string Request { get; set; } = "#LAB?";
            public string Response { get; set; } = "OK";
            public string PortName { get; set; } = string.Empty;
        }
    }
}
