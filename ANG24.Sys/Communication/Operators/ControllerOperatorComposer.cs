using ANG24.Sys.Aplication.Types.CommunicationControllerTypes.Enums;
using ANG24.Sys.Application.Core.DTO;
using ANG24.Sys.Application.Helpers;
using ANG24.Sys.Application.Interfaces.CommunicationControllerInterfaces.Base;
using ANG24.Sys.Application.Interfaces.Services;
using ANG24.Sys.Application.Types;
using ANG24.Sys.Application.Types.Enum;
using Autofac;
using System.Globalization;
using static ANG24.Sys.Application.Types.LabConfiguration.ExternalSoftwareParams.Mods;

namespace ANG24.Sys.Communication.Operators
{
    public sealed class ControllerOperatorComposer : IControllerOperatorComposer
    {
        public event Action<OperatorMessageDto> OnDataReceived;
        public IMethodExecuter CurrentProcess { get; private set; }
        private IDictionary<LabController, IControllerOperator<IControllerOperatorData>> operators;
        private readonly ILabConfigurationService config;
        private readonly ILifetimeScope container;

        //получить список можно, но изменять его нельзя
        public IDictionary<LabController, IControllerOperator<IControllerOperatorData>> Operators =>
            new Dictionary<LabController, IControllerOperator<IControllerOperatorData>>(operators);
        public ControllerOperatorComposer(ILabConfigurationService labConfigurator, ILifetimeScope container)
        {
            config = labConfigurator;
            this.container = container;
            Init();
            //GetOperatorCommands(LabController.MainController.ToString());
        }
        public bool ExecuteCommand(Action<IMethodExecuter> command)
        {
            var com = container.Resolve<IMethodExecuter>();
            command?.Invoke(com);

            if (com.OperatorName == "ControllerComposer")
                com.Execute(this, new CancellationToken());
            else
            {
                var oper = GetOperatorByName(com.OperatorName).Result;
                if (oper is null) return false;
                oper.Execute(com);
            }
            return true;
        }
        public async Task<IEnumerable<Method>> GetOperatorCommands(string operatorName)
        {
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return await Task.Factory.StartNew(() =>
            {
                var cOperator = GetOperatorByName(operatorName).Result;
                if (cOperator == null) return null;

                List<Method> retMethods = new List<Method>();
                var t = cOperator.GetType();
                var methods = t.GetMethods();
                foreach (var method in methods)
                {
                    if (method.Name.StartsWith("add_")
                     || method.Name.StartsWith("remove_")
                     || method.Name.StartsWith("get_")
                     || method.Name.StartsWith("set_")) continue; //Не уверен, может всё таки оставить свойства...

                    var parameters = method.GetParameters();
                    var types = new Type[parameters.Length];
                    for (int i = 0; i < types.Length; i++)
                        types[i] = parameters[i].ParameterType;

                    retMethods.Add(new Method
                    {
                        Name = method.Name,
                        Types = types.Select(x => x.Name).ToArray(),
                        ReturnType = method.ReturnType.Name
                    });
                }
                return retMethods;
            });
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }
        public async Task Subscribe(string operName) => ChangeSubscribtion(await GetOperatorByName(operName), true);
        public async Task Unsubscribe(string operName) => ChangeSubscribtion(await GetOperatorByName(operName), false);

        public async Task<IControllerOperator<IControllerOperatorData>> GetOperatorByName(string name)
        {
#pragma warning disable CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
            return await Task.Factory.StartNew(() =>
            {
                Enum.TryParse(typeof(LabController), name, out var controller);
                operators.TryGetValue((LabController)controller, out var cOperator);
                return cOperator;
            });
#pragma warning restore CS8603 // Возможно, возврат ссылки, допускающей значение NULL.
        }

        #region Commands
        public async Task<bool> SearchForControllers() => await container.Resolve<ControllerFinder>().SearchPorts();
        #endregion
        private void Init()
        {
            operators = new Dictionary<LabController, IControllerOperator<IControllerOperatorData>>();
            InitOperators();
            CheckForExternalSoft();
            //StartScript(File.ReadAllText(AppFilePaths.BasePath + "script.txt"), null).Wait();

            void InitOperators()
            {
                var factory = container.Resolve<ControllerOperatorFactory>();

                foreach (var item in (IEnumerable<LabController>)config["LabControllers"])
                {
                    var service = factory.Create(item);
                    if (service != null)
                        operators.Add(item, service);
                }
            }
            void CheckForExternalSoft()
            {
                var External = config.Config.ExternalSoftwareParameters;
                if (External.SoftwareMods.SA540Mode == SoftMode.External && !string.IsNullOrWhiteSpace(External.SoftwarePaths.SA540SoftFilePath)
                 || External.SoftwareMods.SA640Mode == SoftMode.External && !string.IsNullOrWhiteSpace(External.SoftwarePaths.SA640SoftFilePath)
                 || External.SoftwareMods.SA7100Mode == SoftMode.External && !string.IsNullOrWhiteSpace(External.SoftwarePaths.SA7100SoftFilePath)
                 || External.SoftwareMods.MeasMode == SoftMode.External && !string.IsNullOrWhiteSpace(External.SoftwarePaths.MeasSoftFilePath)
                 || External.SoftwareMods.LVMeasMode == SoftMode.External && !string.IsNullOrWhiteSpace(External.SoftwarePaths.LVMeasSoftFilePath)
                 || External.SoftwareMods.VLFMode == SoftMode.External && !string.IsNullOrWhiteSpace(External.SoftwarePaths.VLFSoftFilePath))
                {
                    container.Resolve<IProcessCreator>();
                }
            }
        }
        private void ChangeSubscribtion(IControllerOperator<IControllerOperatorData> oper, bool subscribe)
        {
            var name = oper?.Name; // против замыканий
            if (oper is not null)
            {
                oper.OnData -= OnData;
                if (subscribe)
                {
                    oper.OnData += OnData;
                    if (!oper.Connected)
                        oper.Connect();
                }
            }

            void OnData(IControllerOperatorData data) => OnDataReceived?.Invoke(new OperatorMessageDto
            {
                Message = data.Message,
                OperatorName = name, // против замыканий
                Optional = data.OptionalInfo
            });
        }
        #region scripts
        public Dictionary<LabController, IControllerOperator<IControllerOperatorData>> scriptOperators = new();
        public Dictionary<LabController, object> operatorsData = new();
        public async Task<bool> StartScript(string script, string imports)
        {
            try
            {
                await ScriptManager.StartScript(this, script, string.IsNullOrWhiteSpace(imports) ? null : imports.Split());
            }
            catch (Exception)
            {
                //логгирование
                return false;
            }
            return true;

            //async Task TestMethod()
            //{
            //    //хз...
            //    Console.WriteLine("Script started...");
            //    ChangeOperators(LabController.MainController, true);
            //    Start(LabModule.HVMDC);
            //    var main = scriptOperators[LabController.MainController] as IMainControllerOperator;
            //    await Task.Delay(3000);
            //    while ((operatorsData[LabController.MainController] as MEAData).Voltage < 100)
            //    {
            //        await Task.Delay(100);
            //        main.VoltageUp();
            //    }

            //    Console.WriteLine("Script end.");
            //}

        }

        public void ChangeOperators(LabController labController, bool isAdd)
        {
            var labOperator = Operators[labController];
            if (isAdd)
            {
                scriptOperators.TryAdd(labController, labOperator);
                operatorsData.TryAdd(labController, null);
                labOperator.OnData -= OnDataRecived; // защита
                labOperator.OnData += OnDataRecived;
                return;
            }
            scriptOperators.Remove(labController);
            labOperator.OnData -= OnDataRecived;

            void OnDataRecived(IControllerOperatorData data) => operatorsData[labController] = data;
        }
        public void Start(LabModule module)
        {
            var main = scriptOperators[LabController.MainController] as IMainControllerOperator;
            main.SetModule(module);
            main.RunLab();
        }
        public void StopLab()
        {
            var main = scriptOperators[LabController.MainController] as IMainControllerOperator;
            main.PowerOff();
            main.SetModule(LabModule.Main);
        }

        [Obsolete]
        public async void StartScriptOld(Action<IOperatorScript> script)
        {
            var scr = container.Resolve<IOperatorScript>();
            script?.Invoke(scr);
            foreach (var action in scr.Methods)
            {
                var token = new CancellationToken();
                var oper = GetOperatorByName(action.OperatorName);
                if (action.IsAsync) // некоторые комманды можно не ждать
                    _ = Task.Factory.StartNew(() => action.Execute(oper, token));
                else
                    await Task.Factory.StartNew(() => action.Execute(oper, token));
            }
        }
        [Obsolete]
        public async Task<bool> StartScriptObsolete(string str)
        {
            // Велосипед из костылей
            // Памятник бессмысленно потеряной недели
            // 20.06.22 - 28.06.22 RIP
            var lines = str.Replace("\r", "").Split('\n');
            var operators = new Dictionary<LabController, IControllerOperator<IControllerOperatorData>>();
            var operatorsData = new Dictionary<LabController, object>();
            for (int i = 0; i < lines.Length; i++)
                i += await CheckLine(lines[i], i);
            return true;
            #region local methods
            async Task<int> CheckLine(string line, int index)
            {
                if (string.IsNullOrWhiteSpace(line)) return 0;
                var parsedLine = line.Split(' ');
                switch (parsedLine[0].ToLower())
                {
                    case "addoperator":
                        ChangeOperators(parsedLine[1], true);
                        break;
                    case "removeoperator":
                        ChangeOperators(parsedLine[1], false);
                        break;
                    case "wait":
                        await Task.Delay(int.Parse(parsedLine[1]));
                        break;
                    case "execute":
                        Execute(parsedLine[1], parsedLine[2], parsedLine[3..]);
                        break;
                    case "start":
                        Start(parsedLine[1]);
                        break;
                    case "stop":
                        Stop(parsedLine[1]);
                        break;
                    case "checkobj":
                        CheckObj(line);
                        break;
                    case "while":
                        return await While(line, index); // пропуск строк задейсовованых в цикле
                    case "for":
                        return await For(line, index); // пропуск строк задейсовованых в цикле
                    default:
                        throw new Exception($"Can't parse line {index}");
                }
                return 0;
            }
            bool CheckObj(string line)
            {
                var parsedLine = line.Split(' ');
                string oper = parsedLine[1];
                string property = parsedLine[2];
                string condition = parsedLine[3];
                var compareValue = parsedLine[4];

                var obj = operatorsData[Enum.Parse<LabController>(oper)];
                var method = obj?.GetType().GetProperty(property).GetGetMethod();
                var value = method?.Invoke(obj, null);
                if (value is null) return false;

                // сравнивание в double
                if (double.TryParse(value.ToString(), out var @double))
                    return CompareNumber(@double, double.Parse(compareValue), condition);
                return CompareEquals(value, compareValue, condition);
            }
            bool CompareNumber(double value, double compareValue, string condition)
                => condition switch
                {
                    ">" => value > compareValue,
                    ">=" => value >= compareValue,
                    "<" => value < compareValue,
                    "<=" => value <= compareValue,
                    "!=" => value != compareValue,
                    "==" => value == compareValue,
                    _ => false,
                };
            bool CompareEquals(object value, object compareValue, string condition)
                => condition switch
                {
                    "!=" => !value.Equals(compareValue),
                    "==" => value.Equals(compareValue),
                    _ => false,
                };

            void ChangeOperators(string oper, bool isAdd)
            {
                var labController = Enum.Parse<LabController>(oper);
                var labOperator = this.operators[labController];
                if (isAdd)
                {
                    operators.TryAdd(labController, labOperator);
                    operatorsData.TryAdd(labController, null);
                    labOperator.OnData -= OnDataRecived; // защита
                    labOperator.OnData += OnDataRecived;
                    return;
                }
                operators.Remove(labController);
                labOperator.OnData -= OnDataRecived;

                void OnDataRecived(IControllerOperatorData data) => operatorsData[labController] = data;
            }


            void Start(string module)
            {
                var main = operators[LabController.MainController] as IMainControllerOperator;
                main.SetModule(Enum.Parse<LabModule>(module));
                main.RunLab();
            }
            void Stop(string module)
            {
                var main = operators[LabController.MainController] as IMainControllerOperator;
                main.PowerOff();
                main.SetModule(LabModule.Main);
            }

            void Execute(string oper, string methodName, params string[] parameters)
            {
                var controllerOperator = operators[Enum.Parse<LabController>(oper)];
                var method = container.Resolve<IMethodExecuter>();
                method.Parameters = new List<object>();
                method.MethodName = methodName;
                foreach (string parameter in parameters)
                {
                    var obj = parameter.Split(':');
                    method.Parameters.Add(GetParameterType(obj[0], obj[1]));
                }
                method.Execute(controllerOperator);

                object GetParameterType(string type, string obj)
                {
                    return type switch
                    {
                        "int" => int.Parse(obj),
                        "double" => double.Parse(obj),
                        "bool" => bool.Parse(obj),
                        _ => obj
                    };
                }
            }
            async Task<int> For(string line, int index)
            {
                int linesCount = 1;
                var condition = line.Replace("for", "", true, CultureInfo.InvariantCulture)
                                    .Replace(" (", "")
                                    .Replace("(", "")
                                    .Replace(")", "")
                                    .Split(';');
                var cycleConditionValues = condition[1].Split();
                Func<int, bool> cycleCondition = (index) => CompareNumber(index, double.Parse(cycleConditionValues[2]), cycleConditionValues[1]);

                for (int j = index + 1; j < line.Length; j++, linesCount++)
                    if (lines[j].ToLower().Contains("endfor")) break;

                var i = int.Parse(condition[0].Split('=')[1]);
                Action cycleAction = condition[2] switch
                {
                    "i++" => () => i++,
                    "i--" => () => i--,
                    _ => throw new NotImplementedException(),
                };
                for (; cycleCondition(i); cycleAction())
                {
                    for (int j = index + 1; j < index + linesCount; j++)
                        j += await CheckLine(lines[j], j);
                }
                return linesCount;
            }
            async Task<int> While(string line, int index)
            {
                var condition = line.Replace("while", "", true, CultureInfo.InvariantCulture)
                                    .Replace(" (", "")
                                    .Replace("(", "")
                                    .Replace(")", "");
                Func<string, bool> conditionFunc = null;
                int linesCount = 1;
                var parsed = condition.Split(' ');
                if (parsed[0].ToLower().Contains("checkobj")) conditionFunc = CheckObj; //добавить другие проверки
                for (int i = index + 1; i < line.Length; i++, linesCount++)
                    if (lines[i].ToLower().Contains("endwhile")) break;

                while (!conditionFunc(condition))
                    for (int i = index + 1; i < index + linesCount; i++)
                    {
                        i += await CheckLine(lines[i], i);

                        //var obj = operatorsData[Enum.Parse<LabController>("MainController")];
                        //var property = obj.GetType().GetProperty("Voltage");

                        //property.GetSetMethod()?.Invoke(obj, new object[] { (double)property.GetGetMethod()?.Invoke(obj, null) + 1 });
                    }

                return linesCount;
            }
            #endregion
        }
        #endregion
    }
}
