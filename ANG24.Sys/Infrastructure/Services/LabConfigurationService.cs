using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Infrastructure.Helpers;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using static ANG24.Sys.Application.Types.LabConfiguration.ExternalSoftwareParams.Mods;

namespace ANG24.Sys.Infrastructure.Services
{
    public sealed class LabConfigurationService : Executer, ILabConfigurationService
    {
        private bool isDeserializedSuccessfully = false;
        public LabConfigurationService(Autofac.ILifetimeScope container) : base(container) => Deserialize();

        public object this[string parameterName]
        {
            get
            {
                Configuration.TryGetValue(parameterName, out var value);
                return value;
            }
            set
            {
                if (Configuration.ContainsKey(parameterName))
                    Configuration[parameterName] = value;
            }
        }
        public IDictionary<string, object> Configuration { get; private set; } = new Dictionary<string, object>();
        public LabConfiguration Config { get; set; } = new LabConfiguration();
        public void Save()
        {
            if (isDeserializedSuccessfully)
                File.Move(AppFilePaths.ConfigFullPath, AppFilePaths.TempPath + AppFilePaths.ConfigFileName, true);
            var opt = new JsonSerializerOptions { WriteIndented = true };
            opt.Converters.Add(new JsonStringEnumConverter());
            File.WriteAllText(AppFilePaths.ConfigFullPath, JsonSerializer.Serialize(Config, opt));
        }

        public bool SaveConfig(LabConfiguration config)
        {
            try
            {
                Config = config;
                if (Config.LabControllers?.Count == 0)
                    AddLabControllers();
                Save();
                Configuration = ConvertToDictionary(config);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public LabConfiguration GetConfig() => Config;

        private void Deserialize(bool normalConfig = true)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Config = new LabConfiguration(); // в случае, если нет ни одного варианта конфига, будет хотя бы дефолтный
            try
            {
                var file = normalConfig ? File.ReadAllText(AppFilePaths.ConfigFullPath)
                                        : File.ReadAllText(AppFilePaths.TempPath + AppFilePaths.ConfigFileName);
                var opt = new JsonSerializerOptions { WriteIndented = true };
                opt.Converters.Add(new JsonStringEnumConverter());
                Config = JsonSerializer.Deserialize<LabConfiguration>(file, opt);
                isDeserializedSuccessfully = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex switch
                {
                    FileNotFoundException => "Файл конфигурации не найден. Будут выполнены настройки по умолчанию.",
                    FormatException => ex.Message,
                    _ => $"Произошла непредвиденная ошибка: {ex.Message}"
                });
                if (normalConfig)
                    Deserialize(false);
                else
                    OldDeserialize();
            }
            if (Config.LabControllers?.Count == 0)
                AddLabControllers();
            Configuration = ConvertToDictionary(Config);
            void OldDeserialize()
            {
                try
                {
                    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                    XDocument doc = XDocument.Load(AppFilePaths.BasePath + "Config.xml");
                    XElement root = doc.Element("Config");
                    if (root == null) return;
                    XElement Params = root.Element("Params");
                    XElement ExternalSoftwareParams = root.Element("ExternalSoftwareParams");


                    Config.SerialNumber = Convert.ToInt32(root.Element("SerialNumber")?.Value);
                    Config.FazeType = (FazeType)Convert.ToInt32(root.Element("FazeType")?.Value);
                    Config.MaxACVoltage = Convert.ToInt32(root.Element("MaxACVoltage")?.Value);
                    Config.MaxDCVoltage = Convert.ToInt32(root.Element("MaxDCVoltage")?.Value);
                    Config.CompensationIsPresent = Convert.ToBoolean(root.Element("CompensationIsPresent")?.Value);
                    Config.DebugMode = Convert.ToBoolean(root.Element("DebugMode")?.Value);
                    Config.LVMThreePhaze = Convert.ToBoolean(root.Element("LVMThreePhaze")?.Value);
                    Config.ThermoVision = Convert.ToBoolean(root.Element("ThermoVision")?.Value ?? bool.FalseString);
                    Config.MethodsBurn = Convert.ToBoolean(root.Element("MethodsBurn")?.Value ?? bool.FalseString);
                    Config.ManualIDM = Convert.ToBoolean(root.Element("ManualIDM")?.Value ?? bool.FalseString);
                    Config.IsMDPOn = Convert.ToBoolean(root.Element("IsMDPOn")?.Value ?? bool.FalseString);

                    if (Params != null)
                    {
                        XElement CurrentProtectedParams = Params.Element("CurrentProtectedParams");

                        Config.Parameters.CurrentProtectedParams.CurrentProtectText = Convert.ToInt16(Params.Element("CurrentProtectText")?.Value ?? "100");
                        Config.Parameters.MinKV = Convert.ToDouble(Params.Element("MinKV")?.Value ?? "0.4");
                        Config.Parameters.MaxKV = Convert.ToDouble(Params.Element("MaxKV")?.Value ?? "0.6");

                        Config.Parameters.HVPULSESTEP1 = Convert.ToDouble(Params.Element("HVPULSESTEP1")?.Value ?? "1.12");
                        Config.Parameters.HVPULSESTEP2 = Convert.ToDouble(Params.Element("HVPULSESTEP2")?.Value ?? "2.28");
                        Config.Parameters.HVPULSESTEP3 = Convert.ToDouble(Params.Element("HVPULSESTEP3")?.Value ?? "4.96");

                        Config.Parameters.KJSTEP1 = Convert.ToDouble(Params.Element("KJSTEP1")?.Value ?? "1.12");
                        Config.Parameters.KJSTEP2 = Convert.ToDouble(Params.Element("KJSTEP2")?.Value ?? "2.28");
                        Config.Parameters.KJSTEP3 = Convert.ToDouble(Params.Element("KJSTEP3")?.Value ?? "4.96");
                    }
                    if (Enum.TryParse(root.Element("Protocol")?.Value, out ProtocolType protocol)) Config.Protocol = protocol;
                    foreach (var item in root.Element("LabComposition")?.Elements())
                    {
                        var module = LabModule.NoMode;
                        bool resParse = Enum.TryParse(item.Value, out module);
                        if (resParse)
                            Config.LabComposition.Add(module);
                        else Console.Error.WriteLine("Указанный в конфигурации элемент \"{0}\" не найден в списке существующих.", item.Value);
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("file Config.xml not found");
                }
            }
        }
        private IDictionary<string, object> ConvertToDictionary(object obj)
        {
            // даже если я бы хотел описать, что тут происходит
            // вы бы всё равно не захотели понять это
            // так что примем как факт то, что этот метод работает
            // и не будем его трогать никогда
            // *если что-то не работает - вы забыли добавить атрибут для нового объекта
            Dictionary<string, object> dict = new();

            foreach (var prop in GetProperties(obj))
                dict.Add(prop.Property.Name, prop.Property.GetMethod.Invoke(prop.Sender, null));
            return dict;

            IEnumerable<Prop> GetProperties(object obj)
            {
                Dictionary<PropertyInfo, object> properties = new();

                var objPorps = obj.GetType().GetProperties();
                foreach (var prop in objPorps)
                    yield return new Prop { Property = prop, Sender = obj };

                foreach (var property in objPorps.Where(p => p.GetCustomAttributesData().Any()))
                    foreach (var item in GetProperties(property.GetMethod.Invoke(obj, null)))
                        yield return item;
            }
        }
        private void AddLabControllers()
        {
            var controllers = Config.LabControllers;
            var modules = Config.LabComposition;
            controllers.Add(LabController.MainController); // main

            if (modules.Contains(LabModule.HVMAC) || modules.Contains(LabModule.HVMDCHi))
                controllers.Add(LabController.Compensation); // compensation

            if (modules.Contains(LabModule.GVI) || modules.Contains(LabModule.HVBurn) || modules.Contains(LabModule.Meas))
                controllers.Add(LabController.Reflect); // reflect

            if (modules.Contains(LabModule.SA540) || modules.Contains(LabModule.SA540_1) || modules.Contains(LabModule.SA540_3))
                if (Config.ExternalSoftwareParameters.SoftwareMods.SA540Mode == SoftMode.Internal)
                    controllers.Add(LabController.SA540);

            if (modules.Contains(LabModule.SA640))
                if (Config.ExternalSoftwareParameters.SoftwareMods.SA640Mode == SoftMode.Internal)
                    controllers.Add(LabController.SA640);

            if (controllers.Contains(LabController.SA540) || modules.Contains(LabModule.LVMeas))
            {
                controllers.Add(LabController.PowerSelector); // PowerSelector
                controllers.Add(LabController.VoltageSyncronizer); // VoltageSyncronizer
            }
            if (modules.Contains(LabModule.Tangent2000) || modules.Contains(LabModule.Bridge))
            {
                if (Config.ExternalSoftwareParameters.SoftwareMods.SA7100Mode == SoftMode.Internal)
                    controllers.Add(LabController.SA7100);
                controllers.Add(LabController.BridgeCommutator); // BridgeCommutator
            }
            if (modules.Contains(LabModule.GP500))
                controllers.Add(LabController.GP500);
            if (Config.ThermoVision)
                controllers.Add(LabController.ThermoVision);
            //проверить и исправить
            controllers.Add(LabController.VLF);
        }
    }

    internal struct Prop
    {
        public PropertyInfo Property { get; set; }
        public object Sender { get; set; }
    }
}

