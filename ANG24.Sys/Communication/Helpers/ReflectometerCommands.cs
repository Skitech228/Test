using System.Runtime.InteropServices;

namespace ANG24.Sys.Communication.Helpers
{
    internal static class ReflectometerCommands
    {
        [DllImport("RefDLL.dll", EntryPoint = "GetDeviceVersion", CallingConvention = CallingConvention.Cdecl)]
        internal static extern short GetDeviceVersion();
        [DllImport("RefDLL.dll", EntryPoint = "DeviceConnect", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool DeviceConnect();
        [DllImport("RefDLL.dll", EntryPoint = "Init", CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool InitDLL(int td, int vd, int trig, int PulseLen);
        [DllImport("RefDll.dll", EntryPoint = "InitMod", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void InitModDll();
        [DllImport("RefDLL.dll", EntryPoint = "ReadOSC", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ushort ReadOSCDLL([Out] IntPtr c);
        [DllImport("RefDLL.dll", EntryPoint = "SetSampleRate", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetSampleRateDLL(int sr);
        [DllImport("RefDLL.dll", EntryPoint = "GetVoltDiv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ushort GetVoltDivDLL();
        [DllImport("RefDLL.dll", EntryPoint = "SetVoltDiv", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetVoltDivDLL(int index);
        [DllImport("RefDLL.dll", EntryPoint = "SetTriggerLevel", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetTriggerLevelDLL(int TriggerLevel);
        [DllImport("RefDLL.dll", EntryPoint = "GetTriggerLevel", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ushort GetTriggerLevelDLL();
        [DllImport("RefDll.dll", EntryPoint = "ChangePulseLen", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ChangePulseLenDLL(int pulseLen);
        [DllImport("RefDll.dll", EntryPoint = "SetIDM", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetIDMDLL(double freq, int w, int p);
        [DllImport("RefDll.dll", EntryPoint = "SetTriggerMode", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetTriggerModeDll(int ht, int vt);
        [DllImport("RefDll.dll", EntryPoint = "ReadOSC3", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ushort ReadOSC3DLL([In, Out] IntPtr c);
        [DllImport("RefDll.dll", EntryPoint = "CalledProcStart", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CalledProcStartDll();
        [DllImport("RefDll.dll", EntryPoint = "SetREF", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetREFDll();
        [DllImport("RefDll.dll", EntryPoint = "SetFreqIDM", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ushort SetFreqIDMDll(double freq);
        [DllImport("RefDll.dll", EntryPoint = "GetpWave", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ushort GetWaveDll();
        [DllImport("RefDll.dll", EntryPoint = "GetpPeriod", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ushort GetPeriodDll();
    }
}
