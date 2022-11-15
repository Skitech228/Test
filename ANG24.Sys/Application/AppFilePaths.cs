namespace ANG24.Sys.Application
{
    public static class AppFilePaths
    {
        static AppFilePaths()
        {
            Directory.CreateDirectory(BasePath);
            Directory.CreateDirectory(TempPath);
        }
        public static char s = Path.DirectorySeparatorChar;
        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $@"{s}AngstremTech{s}";
        public static string TempPath = BasePath + $"Temp{s}";
        public static string LogsPath = BasePath + $"Logs{s}";
        public static string ConfigFileName = "LabConfig.json";
        public static string ControllersFileName = "Controllers.json";

        public static string ConfigFullPath = BasePath + ConfigFileName;
        public static string ControllersFullPath = BasePath + ControllersFileName;
        public static string OldPortsFullPath = BasePath + "Ports.xml";

        //public static string ScriptsPath = TempPath + "Scripts.json";
        public static string TimeWorkPath = LogsPath + "TimeWork.xml";


        public static string OldLogsPath = $"C:{s}Log{s}";
    }

}
