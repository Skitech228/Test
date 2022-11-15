using System.Diagnostics;

namespace ANG24.Sys.Infrastructure.Logging
{
    public static class ControllerLogger
    {
        private static readonly string dirName = "C:\\Log\\";
        private static readonly DirectoryInfo dirInfo;
        private static readonly object locker = new object();
        public static double GP500Voltage;
        public static double GP500Current;
        public static bool GP500On;
        public static bool Read = false;
        static ControllerLogger()
        {
            dirInfo = new DirectoryInfo(dirName);
            if (!dirInfo.Exists)
                dirInfo.Create();
        }
        public static async void WriteString(string message, bool command = false)
        {
            if (!Read)
                await Task.Run(() =>
                {
                    lock (locker)
                        try
                        {
                            StreamWriter fm = new StreamWriter(@"" + dirInfo.FullName + @"\LogInfo_" +
                                    DateTime.Now.ToString("yyyy-MM-dd") +
                                     ".log", true);
                            if (!command)
                                fm.Write("{0:T} -> ", DateTime.Now);
                            else
                                fm.Write("{0:T} <- ", DateTime.Now);

                            fm.Write(@"{0}", message);
                            fm.WriteLine();
                            fm.Close();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                });
        }
    }
}
