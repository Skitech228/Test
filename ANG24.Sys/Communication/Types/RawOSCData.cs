using System.Runtime.InteropServices;

namespace ANG24.Sys.Communication.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RawOSCData
    {
        public int lengthArray;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 65536)]
        public ushort[] IntArr;
        public int GS;
        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
        public static bool operator ==(RawOSCData left, RawOSCData right) => left.Equals(right);
        public static bool operator !=(RawOSCData left, RawOSCData right) => !(left == right);
    }
}
