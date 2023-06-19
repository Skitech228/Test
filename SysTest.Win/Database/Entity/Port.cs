using SQLite;
using System.Diagnostics.CodeAnalysis;

namespace SysTest.Win.Database.Entity
{
    public class Port
    {
        public int PortId { get; set; }
        public ushort? PortNum { get; set; }
        public string HostName { get; set; }

        public string Protocol { get; set; }

        public byte? StringEndByte { get; set; }
        public bool IsConnected { get; set; }

    }
}