using ANG24.Sys.Infrastructure.Helpers;
using ANG24.Sys.Infrastructure.Logging;
using ANG24.Sys.Application.Interfaces.DBRepositories;
using ANG24.Sys.Application.Types.ServiceTypes;
using System.Text.RegularExpressions;
using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;

namespace ANG24.Sys.Infrastructure.Services
{
    public sealed class TimeWorkService : Executer, ITimeWorkService
    {
#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
        private readonly string PATHLOG = AppFilePaths.OldLogsPath;
        private readonly string PATHTIME = AppFilePaths.TimeWorkPath;
        private readonly IModuleRepository moduleRepository;
        private readonly ILabConfigurationService config;
        private LabelTime Label;
        private int TimeUpdate = default;
        private readonly List<Module> Time;

        public TimeWorkService(IModuleRepository moduleRepository,
                               ILabConfigurationService config,
                               Autofac.ILifetimeScope container) : base(container)
        {
            this.moduleRepository = moduleRepository;
            this.config = config;
            Time = new List<Module>();
            if (!File.Exists(PATHTIME)) CreateFile();
            else
                GetModulesTime();
            //Task.Factory.StartNew(GetTime);
        }
        public IEnumerable<Module> SynchronizationTime()
        {
            if (Time.Count == 0) GetModulesTime();
            DirectoryInfo dir = new(PATHLOG);
            Dictionary<string, int> time = new();
            var files = dir.GetFiles(@"LogInfo**.log");
            if (files.Length != 0 && Label.Name != "")
            {
                var fileList = files.SkipWhile(x => x.Name != Label.Name).ToArray();
                ParseData(fileList, Label.Line);
            }
            else ParseData(files);
            SaveUpdateTime();
            return Time;
        }
        /// <summary>
        /// Время успешной синхронизации
        /// </summary>
        /// <returns>Возвращает время последней успешной синхронизации </returns>
        public int TimeUpdateSynchronization() => TimeUpdate;

        private void CreateFile()
        {
            var list = moduleRepository.GetList();
            var modules = config.Config.LabComposition;
            XDocument doc = new();
            XElement timeModule = new("TimeModules");
            foreach (var item in modules)
            {
                if (item == LabModule.Reflect) continue;
                timeModule.Add(new XElement("Module",
                    new XElement("Synonym", item.ToString()),
                    new XElement("Name", list.Where(x => x.Synonym == item.ToString()).Select(x => x.Name).FirstOrDefault()),
                    new XElement("Time", 0)
                    ));
            }
            doc.Add(new XElement("Time",
               new XElement("Label",
                   new XElement("Name", ""),
                   new XElement("Line", 0)
                   ),
               new XElement(timeModule)
               ));
            doc.Save(PATHTIME);
        }
        //Получает время до синхронизации
        private IEnumerable<Module> GetModulesTime()
        {
            XDocument document = XDocument.Load(PATHTIME);
            XElement root = document.Element("Time");
            XElement timeModules = root.Element("TimeModules");
            XElement label = root.Element("Label");
            Label = new LabelTime
            {
                Name = label.Element("Name").Value,
                Line = Convert.ToInt32(label.Element("Line").Value)
            };
            foreach (var item in timeModules.Elements("Module"))
            {
                if (item.Element("Synonym").Value == "GVI") item.Element("Synonym").Value = "HVPULSE";
                if (item.Element("Synonym").Value == "Reflect") continue;
                Time.Add(new Module
                {
                    Synonym = item.Element("Synonym").Value,
                    Time = TimeSpan.FromSeconds(Convert.ToInt32(item.Element("Time").Value)),
                    Name = item.Element("Name").Value
                });
            }
            return Time;
        }
        /// <summary>
        /// Синхронизирует время работы модулей
        /// </summary>
        /// <returns></returns>
        /// <returns>Список актуального времения</returns>
        private bool SaveUpdateTime()
        {
            try
            {
                //Сохраняем все в файл
                XDocument document = XDocument.Load(PATHTIME);
                XElement root = document.Element("Time");
                XElement label = root?.Element("Label");
                label.Element("Name").Value = Label.Name;
                label.Element("Line").Value = Label.Line.ToString();
                var gvi = root.Elements("TimeModules").Elements("Module").FirstOrDefault(x => x.Element("Synonym").Value == "GVI");
                if (gvi != null) gvi.Element("Synonym").Value = "HVPULSE";
                foreach (var item in root.Elements("TimeModules").Elements("Module"))
                {
                    if (item.Element("Synonym").Value == "Reflect") continue;
                    item.Element("Time").Value = Time.FirstOrDefault(x => x.Synonym == item.Element("Synonym").Value).Time.TotalSeconds.ToString();
                }
                document.Save(PATHTIME);
            }
            catch
            {
                return false;
            }
            return true;
        }
        private void ParseData(FileInfo[] files, int stroke = -1)
        {
            Dictionary<string, int> timeModules = new();
            string endNameFile = default;
            bool readTime = false; // Чтение запущенного модуля
            int indexLine = default; // Номер строки в файле
            Regex pattern = new Regex(@"^(?:(\d{1,2}:\d{1,2}:\d{1,2})(?:(?:\s?->\s?)||(?:\s?<-\s?)))(?:(?:(\w+)(?:,))(?:(?:\w+)(?:,)){17}(?:\w+)|.*)");
            foreach (var file in files)
            {
                if (file.Name.Contains(DateTime.Now.ToString("yyyy-MM-dd")))
                {
                    ControllerLogger.Read = true;
                    Thread.Sleep(1000);
                    parseFile(file);
                    ControllerLogger.Read = false;
                }
                else parseFile(file);
                endNameFile = file.Name;
            }

            Label.Name = endNameFile ?? "";
            Label.Line = indexLine;

            void parseFile(FileInfo file)
            {
                try
                {
                    StreamReader sr = new StreamReader(file.FullName);
                    string line;
                    int currentTime = default; // Время в читаемой строке
                    string? module = default; // Читаемый модуль
                    indexLine = default;
                    bool initialize = true;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //пишем количество строк в файле
                        indexLine++;
                        if (stroke != -1 && stroke >= indexLine) continue;
                        else
                        {
                            line = line.Trim('\r', '\n').Trim();
                            MatchCollection match = pattern.Matches(line);
                            if (match != null && match?.Count > 0)
                            {
                                if (initialize)
                                {
                                    //Инициализация начальных значений
                                    module = match[0]?.Groups[2]?.Value;
                                    if (TimeSpan.TryParse(match[0]?.Groups[1]?.Value, out TimeSpan curtime))
                                        currentTime = (int)curtime.TotalSeconds;
                                    initialize = false;
                                }
                                var moduleLine = match[0]?.Groups[2]?.Value;
                                int timeLine = default;
                                if (TimeSpan.TryParse(match[0]?.Groups[1]?.Value, out TimeSpan time))
                                    timeLine = (int)time.TotalSeconds;
                                if (string.IsNullOrEmpty(moduleLine))
                                {
                                    //Сообщение
                                    if (line.Contains("POWERUP"))
                                    {
                                        readTime = true;
                                        currentTime = timeLine;
                                        continue;
                                    }
                                    else if (line.Contains("POWERDOWN")) readTime = false;
                                }
                                else
                                {
                                    //Пакет
                                    //Произошла смена модуля при чтении времени
                                    if (module != moduleLine)
                                    {
                                        readTime = false;
                                        module = moduleLine;
                                    }
                                }
                                //Производим запись времени
                                if (readTime)
                                {
                                    if (module != null)
                                    {
                                        //Проверка времени задержки команды (5 сек)
                                        if (timeLine > currentTime + 5) readTime = false;
                                        else if (timeLine > currentTime) addTime(module, timeLine - currentTime);
                                    }
                                }
                                currentTime = timeLine;
                                TimeUpdate = currentTime;
                            }
                        }
                    }
                    sr.Close();
                }
                catch
                {
                    //ControllerLogger.read = true;
                    //Thread.Sleep(1000);
                    ////parseFile(fileName);
                    //ControllerLogger.read = false;
                }
                void addTime(string module, int time)
                {
                    if (module.Contains("SA540"))
                    {
                        var item = Time.FirstOrDefault(x => x.Synonym == "SA540");
                        if (item != null)
                            item.Time += TimeSpan.FromSeconds(time);
                        return;
                    }
                    for (int i = 0; i < Time.Count; i++)
                    {
                        if (module.ToUpper() == "HVPULSE" && Time[i].Synonym.ToUpper() == LabModule.GVI.ToString())
                        {
                            Time[i].Time += TimeSpan.FromSeconds(time);
                            break;
                        }
                        else if (Time[i].Synonym.ToUpper() == module.ToUpper())
                        {
                            Time[i].Time += TimeSpan.FromSeconds(time);
                            break;
                        }
                    }
                }
            }
        }
#pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.
        private struct LabelTime
        {
            public string Name { get; set; }
            public int Line { get; set; }
        }

    }
}
