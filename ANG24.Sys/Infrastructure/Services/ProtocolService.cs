//using Lab.Infrastucture.Components.Types.Models;
//using Lab.Infrastucture.Components.ViewModels;
//using Lab.Infrastucture.Helpers;
//using Lab.Infrastucture.Services.Interfaces;
//using Lab.Infrastucture.Types;
//using Lab.Infrastucture.Types.ControllerDataModel;
//using Microsoft.Win32;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace Infrastructure.Services
//{
//    public class ProtocolService : IProtocolService
//    {
//        const string FILE = "ConfigProtocol.xml";
//        private readonly INotificationMessage message;
//        private readonly IMainControllerService service;
//        private List<Protocol> protocols = new List<Protocol>();
//        private Protocol currentProtocol;
//        public bool Activity => ActiveProtocol();
//        public event UpdateListProtocolEventHandler UpdateListProtocol;
//        public event ActivityProtocolEventHandler ActivityProtocol;
//        public event SelectedProtocolEventHandler SelectedProtocol;
//        public event DeselectedProtocolEventHandler DeselectedProtocol;
//        //Для испытаний
//        public event ResetMaxParamEventHandler ResetMaxParam;
//        public ProtocolService(INotificationMessage message, IMainControllerService service)
//        {
//            this.message = message;
//            this.service = service;
//            Initialize();
//        }
//        private void Initialize()
//        {
//            if (!File.Exists(FILE))
//            {
//                ////Получаем список модулей из конфигурации лабы
//                List<string> modules = new List<string>();
//                modules.Add("HVMAC");
//                modules.Add("HVMDC");
//                modules.Add("SA640");
//                modules.Add("SA540");
//                modules.Add("Bridge");
//                //Создаем новый конфиг зависимостей для протоколов
//                XDocument doc = new XDocument();
//                XElement newRoot = new XElement("protocols");
//                foreach (var item in modules)
//                {
//                    newRoot.Add(new XElement("protocol",
//                        new XElement("module", item),
//                        new XElement("fileName", ""),
//                        new XElement("pathProtocol", ""),
//                        new XElement("active", "False")
//                        ));
//                }
//                XElement options = new XElement("options");
//                options.Add(new XElement("energyObject",
//                                    new XElement("fileName", ""),
//                                    new XElement("path", ""),
//                                    new XElement("sourceData", "none")
//                                    ));
//                doc.Add(newRoot);
//                newRoot.Add(options);
//                doc.Save(FILE);
//            }
//        }
//        public void EditDependencyProtocol(string Module)
//        {
//            OpenFileDialog openDialog = new OpenFileDialog();
//            openDialog.Filter = "Ods File (.ods)|*ods";
//            if (openDialog.ShowDialog() == true)
//            {
//                XDocument doc = XDocument.Load(FILE);
//                var item = doc.Element("protocols").Elements("protocol").Where(x => x.Element("module").Value == Module).FirstOrDefault();
//                item.Element("fileName").Value = openDialog.SafeFileName;
//                item.Element("pathProtocol").Value = openDialog.FileName;
//                item.Element("active").Value = "True";
//                doc.Save(FILE);
//                UpdateListProtocol?.Invoke();
//                ActivityProtocol?.Invoke(true);
//            }
//        }
//        private bool ActiveProtocol()
//        {
//            var protocol = GetProtocolDependency();
//            if (protocol != null && !string.IsNullOrEmpty(protocol.PathProtocol))
//            {
//                return protocol.Active;
//            }
//            return false;
//        }
//        private ProtocolModel GetProtocolDependency()
//        {
//            var list = ReadDependencyProtocols();
//            var protocol = list.Where(x => service.SelectedModule.ToString().Contains(x.Module)).FirstOrDefault();
//            return protocol;
//        }
//        public IEnumerable<DocTable> GetDataProtocol()
//        {
//            var data = new ObservableCollection<DocTable>();
//            var protocol = InitProtocol();
//            if (protocol is null) return data;
//            else if (protocol == true)
//            {
//                if (currentProtocol.DocTables is null) currentProtocol.DocTables = currentProtocol.Sheet.GetDocTables();
//                foreach (DocTable table in currentProtocol.DocTables)
//                {
//                    if (table.TableCells.Any(x => service.SelectedModule.ToString().Contains(x.Module)))
//                        data.Add(table);
//                }
//                return data;
//            }
//            return null;
//        }
//        public Dictionary<int, List<Property>> GetEnergyObjects()
//        {
//            Dictionary<int, List<Property>> energyObjects = new Dictionary<int, List<Property>>();
//            var protocol = InitProtocol();
//            if (protocol is null) return energyObjects;
//            else if (protocol == true)
//            {
//                if (currentProtocol.EnergyObjects is null) currentProtocol.EnergyObjects = currentProtocol.Sheet.GetEnergyObject();
//                return currentProtocol.EnergyObjects;
//            }
//            return null;
//        }
//        public void updateDataProtocol()
//        {
//            var DataTables = GetDataProtocol();
//            foreach (var table in DataTables)
//            {
//                if (!string.IsNullOrEmpty(currentProtocol.Sheet.GetValue(table.Cell)))
//                    table.Name = currentProtocol.Sheet.GetValue(table.Cell);
//                foreach (var item in table.TableGroups?.Where(x => x is ColumnGroup).ToList().ConvertAll(x => x as ColumnGroup))
//                {
//                    if (!string.IsNullOrEmpty(currentProtocol.Sheet.GetValue(item.Cell)))
//                        item.Name = currentProtocol.Sheet.GetValue(item.Cell);
//                }
//                foreach (var item in table.TableGroups?.Where(x => x is RowGroup).ToList().ConvertAll(x => x as RowGroup))
//                {
//                    if (!string.IsNullOrEmpty(currentProtocol.Sheet.GetValue(item.Cell)))
//                        item.Name = currentProtocol.Sheet.GetValue(item.Cell);
//                }
//            }
//            UpdateListProtocol?.Invoke();
//        }
//        public DocDevice GetDocDevice() => currentProtocol.DocDevice;
//        public Dictionary<string, bool> GetModulesProtocol() => currentProtocol.Modules;
//        public bool? InitProtocol()
//        {
//            bool? success = false;
//            var list = ReadDependencyProtocols();
//            var protocolConfig = list.Where(x => service.SelectedModule.ToString().Contains(x.Module)).FirstOrDefault();
//            if (protocolConfig != null && !string.IsNullOrEmpty(protocolConfig.PathProtocol))
//            {
//                if (protocolConfig.Active)
//                {
//                    if (File.Exists(protocolConfig.PathProtocol))
//                    {
//                        Protocol find = protocols.Where(x => x.Path == protocolConfig.PathProtocol).FirstOrDefault();
//                        if (find is null)
//                        {
//                            SheetManager sheet = null;
//                            try
//                            {
//                                sheet = new SheetManager(protocolConfig.PathProtocol);
//                            }
//                            catch { }
//                            if (sheet is null)
//                            {
//                                message.CustomMessage("Ошибка открытия файла. " +
//                                    "Возможно файл открыт сторонней программой. Закройте файлы и повторите попытку");
//                                return false;
//                            }
//                            currentProtocol = new Protocol
//                            {
//                                Sheet = sheet,
//                                //currentProtocol.EnergyObjects = currentProtocol.Sheet.GetEnergyObject();
//                                Path = protocolConfig.PathProtocol
//                            };
//                            currentProtocol.DocDevice = currentProtocol.Sheet.GetDevices();
//                            //if (currentProtocol.EnergyObjects.Count == 0)
//                            //{
//                            //currentProtocol.DocTables = currentProtocol.Sheet.GetDocTables();
//                            //currentProtocol.DocDevice = currentProtocol.Sheet.GetDevices();
//                            //}
//                            protocols.Add(currentProtocol);
//                        }
//                        else
//                            currentProtocol = find;
//                        success = true;
//                    }
//                    else
//                    {
//                        message.CustomMessage("Файла протокола по этому пути " + protocolConfig.PathProtocol + " не существует.\r\n Возможно он был удален или переименован. Добавьте, пожалуйста, протокол снова");
//                        DeleteDependencyProtocol(protocolConfig.Module);
//                        success = false;
//                    }
//                }
//                else
//                {
//                    success = null;
//                }
//            }
//            return success;
//        }

//        private void DeleteDependencyProtocol(string module)
//        {
//            XDocument doc = XDocument.Load(FILE);
//            var root = doc.Element("protocols").Elements("protocol");
//            var item = doc.Element("protocols").Elements("protocol").Where(x => x.Element("module").Value == module.ToString()).FirstOrDefault();
//            item.Element("fileName").Value = "";
//            item.Element("pathProtocol").Value = "";
//            item.Element("active").Value = "False";
//            doc.Save(FILE);
//        }
//        public void ChangeActiveProtocol(ProtocolModel model)
//        {
//            XDocument doc = XDocument.Load(FILE);
//            var protocol = doc.Element("protocols").Elements("protocol").Where(x => x.Element("module").Value == model.Module).FirstOrDefault();
//            protocol.Element("active").Value = model.Active.ToString();
//            doc.Save(FILE);
//            if (protocol.Element("pathProtocol").Value != "")
//                if (service.SelectedModule.ToString().Contains(model.Module))
//                    ActivityProtocol?.Invoke(model.Active);
//        }
//        public List<ProtocolModel> ReadDependencyProtocols()
//        {
//            List<ProtocolModel> protocols = new List<ProtocolModel>();
//            XDocument doc = XDocument.Load(FILE);
//            var root = doc.Element("protocols").Elements("protocol");
//            foreach (var item in root)
//            {
//                ProtocolModel protocol = new ProtocolModel(this);
//                protocol.Module = item.Element("module").Value;
//                protocol.FileName = item.Element("fileName").Value;
//                protocol.PathProtocol = item.Element("pathProtocol").Value;
//                protocol.Active = Convert.ToBoolean(item.Element("active").Value);
//                protocols.Add(protocol);
//            }
//            return protocols;
//        }

//        public void saveParam(int row, int column, string value)
//        {
//            currentProtocol.Sheet.InputDocData(row, column, value);
//        }
//        public bool saveParam(string cell, string value)
//        {
//            bool result = default;
//            if (!(string.IsNullOrEmpty(cell) && string.IsNullOrEmpty(value)))
//                result = currentProtocol.Sheet.InputDocData(cell, value);
//            return result;
//        }
//        public void saveEnergyObject()
//        {

//        }
//        public void CloseProtocol()
//        {
//            DeselectedProtocol?.Invoke();
//            saveFile = null;
//        }
//        public void OpenProtocol()
//        {
//            var nameProtocol = GetProtocolDependency()?.FileName;
//            SelectedProtocol?.Invoke(nameProtocol);
//        }
//        private SaveFileDialog saveFile;
//        public void SaveProtocol()
//        {
//            try
//            {
//                if (saveFile == null)
//                {
//                    saveFile = new SaveFileDialog
//                    {
//                        InitialDirectory = @"c:\",
//                        DefaultExt = ".ods",
//                        Filter = "Ods File (.ods)|*ods"
//                    };
//                    if (saveFile.ShowDialog() == true && !string.IsNullOrEmpty(saveFile.FileName))
//                    {

//                        var success = currentProtocol.Sheet.Save(saveFile.FileName);
//                        if (!success) throw new Exception();
//                        message.CustomMessage("Протокол успешно сохранен");
//                    }
//                    else saveFile = null;
//                }
//                else
//                {
//                    var success = currentProtocol.Sheet.Save(saveFile.FileName);
//                    if (!success) throw new Exception();
//                    message.CustomMessage("Протокол успешно сохранен");
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Ошибка сохранения файла протокола\r\n" + ex);
//                saveFile = null;
//                message.CustomMessage("Произошла ошибка сохранения файла протокола. Закройте сторонние файлы и повторите попытку");
//            }
//        }
//        public void ResetMaxParameters() => ResetMaxParam?.Invoke();
//        public void removeParam(string cell)
//        {
//            if (!string.IsNullOrEmpty(cell)) currentProtocol.Sheet.RemoveCellDocData(cell);
//        }
//        public void IntermediateTrigger()
//        {
//            if (!currentProtocol.Modules.ContainsKey(service.SelectedModule.ToString()))
//                currentProtocol.Modules[service.SelectedModule.ToString()] = true;
//        }
//        #region EnergyObject
//        public bool GetVisibilityEnergyObject()
//        {
//            var source = GetEnergyObject();
//            var EnergyObject = GetEnergyObjects();
//            if (source.sourceData == "file" && EnergyObject?.Count > 0) return true;
//            return false;
//        }
//        public void ChangeSourceDataEnergyObject(string sourceData)
//        {
//            XDocument doc = XDocument.Load(FILE);
//            var item = doc.Element("protocols").Element("options").Element("energyObject");
//            switch (sourceData)
//            {
//                case "file":
//                    item.Element("sourceData").Value = "file";
//                    break;
//                case "dataBase":
//                    item.Element("sourceData").Value = "dataBase";
//                    break;
//                case "none":
//                    item.Element("sourceData").Value = "none";
//                    break;
//            }
//            doc.Save(FILE);
//        }
//        private string PathEnergyObject;
//        public string SelectSourceDataEnergyObject()
//        {
//            string name = string.Empty;
//            OpenFileDialog openDialog = new OpenFileDialog();
//            openDialog.Filter = "CSV File (.csv)|*csv";
//            if (openDialog.ShowDialog() == true)
//            {
//                name = openDialog.SafeFileName;
//                PathEnergyObject = openDialog.FileName;
//            }
//            return name;
//        }
//        public void SaveSourceDataEnergyObject(string name)
//        {
//            XDocument doc = XDocument.Load(FILE);
//            var item = doc.Element("protocols").Element("options").Element("energyObject");
//            if (!string.IsNullOrEmpty(PathEnergyObject))
//            {

//                item.Element("fileName").Value = name;
//                item.Element("path").Value = PathEnergyObject;
//                item.Element("sourceData").Value = "file";

//            }
//            else
//            {
//                item.Element("sourceData").Value = "file";
//            }
//            doc.Save(FILE);
//        }
//        public (string name, string path, string sourceData) GetEnergyObject()
//        {
//            string name = default;
//            string path = default;
//            string sourceData = default;
//            XDocument doc = XDocument.Load(FILE);
//            var item = doc.Element("protocols").Element("options").Element("energyObject");
//            if (File.Exists(item.Element("path").Value))
//            {
//                name = item.Element("fileName").Value;
//                path = item.Element("path").Value;
//                sourceData = item.Element("sourceData").Value;
//            }
//            else
//            {
//                item.Element("fileName").Value = "";
//                item.Element("path").Value = "";
//                item.Element("sourceData").Value = "none";
//            }
//            return (name, path, sourceData);
//        }
//        #endregion
//    }

//    class Protocol
//    {
//        public Protocol()
//        {
//            Modules = new Dictionary<string, bool>();
//        }
//        public SheetManager Sheet { get; set; }
//        public string Path { get; set; }
//        public IEnumerable<DocTable> DocTables { get; set; }
//        public Dictionary<string, bool> Modules { get; set; }
//        public DocDevice DocDevice { get; set; }
//        public Dictionary<int, List<Property>> EnergyObjects { get; set; }
//    }
//}
