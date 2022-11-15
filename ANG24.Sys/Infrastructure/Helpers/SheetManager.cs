using ODFTablesLib;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ANG24.Sys.Infrastructure.Helpers
{
    internal class SheetManager
    {
        private ODFTables odf;
        private string path = string.Empty;
        private readonly Regex regex_ts = new Regex(@"\[(ts):?(\d+)?,?:?(\d+)?,?(i)?\](.*)"); //паттерн для поиска заголовка таблицы ([ts[:<RowsCount, ColumnsCount>, <i>(инверсия)][<TableName>]])
        private readonly Regex regex_device = new Regex(@"\[(device):([^\]\[]*)\]"); //паттерн для определения девайса ([device:<device-dataType>])
        private readonly Regex regex_eo = new Regex(@"\[EnergyObject:(?<number>[\d]),(?<property>.+)\]"); //паттерн для определения энергообъекта ([EO:<numberColumn>])
        public SheetManager() { }
        public SheetManager(string path) => OpenDocument(path);
        public void OpenDocument(string path)
        {
            odf = new ODFTables(path);
            this.path = path;
        }

        public void Save() => Save(path);
        public bool Save(string path)
        {
            try
            {
                odf.Save(path);
                this.path = path; //переключаемся c ранее открытого на сохраненный, для его дальнейшего изменения
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// анализирует документ, определяя ключевые слова и их местоположение
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DocTable> GetDocTables()
        {
            //GetEnergyObject();
            var result = new List<DocTable>();

            //получаем пространство ячеек документа
            var cells = odf.Cells;
            var cell_tr = (0, 0);
            if (cells.FindText(regex_ts, out _, out _)) //если обнаружена таблица 
            {
                while (cells.FindText(regex_ts, out cell_tr.Item1, out cell_tr.Item2))
                {
                    //получаем результат выполнения регулярного выражения
                    var matches = regex_ts.Matches(odf.Cells[cell_tr.Item1, cell_tr.Item2].Value.ToString())[0];
                    var row = cell_tr.Item1 + 1;                                                             //переключаемся на строку ниже (там находится таблица)
                    var column = 0;                                                                          //колонка остается 0
                    var content = matches.Groups[5].Value.ToString().Trim();                                 //получаем содержимое ячейки
                    odf.Cells[cell_tr.Item1, cell_tr.Item2].Value = content;                                  //заменяем текущее содержимое ячейки значением без ключевого слова
                    var width = int.Parse(matches.Groups[3].Value.Trim());                                //получаем длину таблицы (параметр 1 ключевого слова)
                    var height = int.Parse(matches.Groups[2].Value.Trim());                                //получаем ширину таблицы (параметр 2 ключевого слова)
                    var range = cells.GetSubrangeRelative(row, column, width, height);                      //получаем инструментальную область для таблицы для дальнейшей обработки
                    var DocTab = new DocTable()
                    {
                        Name = content,
                        Cell = cells[cell_tr.Item1, cell_tr.Item2].Name,
                        Width = width,
                        Height = height,
                        Range = range,
                        odf = odf,
                    };
                    var find = DocTab.SearchTableGroupsInRange();                                                       //выполняем поиск всех групп ключевых слов
                    if (find) result.Add(DocTab);                                                                      //добавляем результат в список
                }
            }
            return result;
        }

        public IDictionary<int, List<Property>> GetEnergyObject()
        {
            //var result = new List<DocTable>();
            Dictionary<int, List<Property>> EnergyObjects = new Dictionary<int, List<Property>>();
            //получаем пространство ячеек документа
            var cells = odf.Cells;
            var cell_eo = (0, 0);
            if (cells.FindText(regex_eo, out _, out _)) //если обнаружена таблица 
            {
                while (cells.FindText(regex_eo, out cell_eo.Item1, out cell_eo.Item2))
                {
                    //получаем результат выполнения регулярного выражения
                    var matches = regex_eo.Matches(odf.Cells[cell_eo.Item1, cell_eo.Item2].Value.ToString())[0];
                    var property = new Property() { NameColumn = matches.Groups["property"]?.Value.Trim() };             // Получаем имя столбца
                    property.Cell.Add(odf.Cells[cell_eo.Item1, cell_eo.Item2]);                                   // Добавляем ячейки
                    var content = odf.Cells[cell_eo.Item1, cell_eo.Item2].Value;
                    odf.Cells[cell_eo.Item1, cell_eo.Item2].Value = content.Replace(matches.Groups[0].Value, ""); // Удаление ключевого слова
                    var key = int.Parse(matches.Groups["number"].Value.Trim());                                 // Получение номера энергообъекта
                    if (!EnergyObjects.ContainsKey(key)) EnergyObjects[key] = new List<Property>() { property };  // Добавляем новый элемент
                    else
                    {
                        //Если такой элемент уже существует, проверяем свойство на существование
                        List<Property> prop = new List<Property>();
                        EnergyObjects.TryGetValue(key, out prop);
                        if (prop.Count > 0)
                        {
                            //Свойства существуют
                            var select = prop.Where(x => x.NameColumn == property.NameColumn).FirstOrDefault(); // Ищем существует ли свойство
                            if (select != null)
                            {
                                //Cуществует
                                select.Cell.Add(odf.Cells[cell_eo.Item1, cell_eo.Item2]); // Добавляем новую ячейку
                            }
                            else
                            {
                                // Не существует
                                prop.Add(property);
                            }
                        }
                    }
                }
            }
            return EnergyObjects;
        }
        public DocDevice GetDevices()
        {
            DocDevice docDevice = default;
            var table = odf.Cells;
            var cell5 = (0, 0);
            if (table.FindText(regex_device, out cell5.Item1, out cell5.Item2)) // Ищем устройства
            {
                var type = (0, 0);
                var name = (0, 0);
                var number = (0, 0);
                var poverki_date = (0, 0);
                var next_poverki_date = (0, 0);
                while (table.FindText(regex_device, out cell5.Item1, out cell5.Item2))
                {
                    var device_matches = regex_device.Matches(odf.Cells[cell5.Item1, cell5.Item2].Value.ToString())[0]; //Ищем ячейку с device
                    var device = device_matches.Groups[2].Value.ToString().Trim();
                    switch (device) // идентифицируем, что за столбец
                    {
                        case "type":
                            type = cell5;
                            break;
                        case "name":
                            name = cell5;
                            break;
                        case "number":
                            number = cell5;
                            break;
                        case "old-verify-date":
                            poverki_date = cell5;
                            break;
                        case "next-verify-date":
                            next_poverki_date = cell5;
                            break;
                    }
                    odf.Cells[cell5.Item1, cell5.Item2].Value = null; //удаляем ключ из ячейки
                }
                docDevice = new DocDevice()
                {
                    Type = type,
                    Name = name,
                    Number = number,
                    OldDateVerify = poverki_date,
                    NextDateVerify = next_poverki_date
                };
            }
            return docDevice;
        }

        public bool InputDocData(string cell, string value)
        {
            try
            {
                odf.Cells[cell].Value = value;
                return true;
            }
            catch (Exception)
            {
                return false;
            };
        }
        public string GetValue(string cell)
        {
            string content = string.Empty;
            try
            {
                return content = odf.Cells[cell].Value;
            }
            catch (Exception)
            {
                return content;
            };
        }
        public bool InputDocData(int row, int column, string value)
        {
            try
            {
                odf.Cells[row, column].Value = value;
                return true;
            }
            catch
            {
                return false;
            };
        }
        public void RemoveCellDocData(string cell) => odf.Cells[cell].Value = null;
    }


    public class DocDevice
    {
        public (int, int) Type { get; set; }
        public (int, int) Name { get; set; }
        public (int, int) Number { get; set; }
        public (int, int) OldDateVerify { get; set; }
        public (int, int) NextDateVerify { get; set; }
    }
    public abstract class TableGroup
    {
        public ObservableCollection<CellResult> Cells { get; set; }
        public string Name { get; set; }
        public string Cell { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        internal CellRange Range { get; set; }
        public TableGroup()
        {
            Cells = new ObservableCollection<CellResult>();
        }
        public void AddCell(CellResult cell)
        {
            if (!Cells.Any(x => x.GetHashCode() == cell.GetHashCode()))
            {
                Cells.Add(cell);
            }
        }
        public abstract void AssociateCells(ObservableCollection<CellResult> cells);
    }
    public class ColumnGroup : TableGroup
    {
        public override void AssociateCells(ObservableCollection<CellResult> cells)
        {
            foreach (var item in cells)
            {
                if (Range.FirstOrDefault(x => x.Name == item.Cell) != null)
                {
                    item.AddColumn(this);
                }
            }
        }
    }
    public class RowGroup : TableGroup
    {
        public override void AssociateCells(ObservableCollection<CellResult> cells)
        {
            foreach (var item in cells)
            {
                if (Range.FirstOrDefault(x => x.Name == item.Cell) != null)
                {
                    item.AddRow(this);
                }
            }
        }
    }

    public class Property
    {
        public string NameColumn { get; set; }
        public List<Cell> Cell { get; set; } = new List<Cell>();
        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                foreach (var item in Cell)
                    if (item != null) item.Value = value;
            }
        }
    }
    public class DocTable
    {
        private readonly Regex regex_rs = new Regex(@"\[rs\](.*)"); //паттерн для поиска строк-разделителей([rs][<RowName>])
        private readonly Regex regex_cs = new Regex(@"\[cs\](.*)"); //паттерн для поиска столбцов-разделителей([cs][<ColumnName>])
        private readonly Regex regex_tr = new(@"\[(tr):(?<Module>[^,]+),(?<Phaze>[^,]+),(?:(?:{)(?:(?<Mode>[^,]+),(?<Parameter>[^,]+)(?:}))|(?<Parameter>[^,]+))(?:(?:\])|(?:,)(?<Prefix>[^,]+)(?:\]))");
        //паттерн для определения ячейки результата испытания
        public string Name { get; set; }
        public string Cell { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        internal CellRange Range { get; set; }
        internal ODFTables odf { get; set; }
        internal bool SearchTableGroupsInRange()
        {
            var result = new ObservableCollection<TableGroup>();
            var columns = SearchColumnGroups();
            var rows = SearchRowGroups();
            TableCells = SearchCells();
            if (TableCells.Count != 0)
            {
                foreach (var item in columns)
                    result.Add(item);
                foreach (var item in rows)
                    result.Add(item);
                foreach (var item in result)
                {
                    item.AssociateCells(TableCells);
                }

                TableGroups = result;
                return true;
            }
            return false;
        }

        public TableGroup this[int index]
        {
            get => TableGroups?[index] ?? null;
        }
        public ObservableCollection<CellResult> SearchCells()
        {
            var result = new ObservableCollection<CellResult>();
            //определить поиск ячеек по отношению к данной группе
            var cell_tr = (0, 0);
            //определить поиск ячеек по отношению к данной группе 
            while (Range.FindText(regex_tr, out cell_tr.Item1, out cell_tr.Item2))
            {
                var realCell = odf.Cells[cell_tr.Item1, cell_tr.Item2];
                var tr_matches = regex_tr.Matches(odf.Cells[cell_tr.Item1, cell_tr.Item2].Value.ToString())[0]; //Ищем ячейку с tr
                var cell_str = odf.Cells[cell_tr.Item1, cell_tr.Item2].Name;
                var module = tr_matches.Groups[2].Value.ToString().Trim();
                var phase = tr_matches.Groups["Phaze"].Value.ToString().Trim();
                var mode = tr_matches.Groups["Mode"]?.Value.ToString().Trim();
                var parameter = tr_matches.Groups["Parameter"]?.Value.ToString().Trim();
                var prefix = tr_matches.Groups["Prefix"]?.Value.ToString().Trim();
                odf.Cells[cell_tr.Item1, cell_tr.Item2].Value = null;
                var cell = new CellResult()
                {
                    RealCell = realCell,
                    Cell = cell_str,
                    Module = module,
                    Phaze = phase,
                    Mode = mode,
                    Parameter = parameter,
                    Prefix = prefix
                };
                result.Add(cell);
            }
            return result;
        }
        private ObservableCollection<RowGroup> SearchRowGroups()
        {
            var result = new ObservableCollection<RowGroup>();
            var cell_rs = (0, 0);
            if (Range.FindText(regex_rs, out cell_rs.Item1, out cell_rs.Item2))
            {
                while (Range.FindText(regex_rs, out cell_rs.Item1, out cell_rs.Item2))
                {
                    var rs_matches = regex_rs.Matches(odf.Cells[cell_rs.Item1, cell_rs.Item2].Value.ToString())[0].Groups;
                    var rs_content = rs_matches[1].Value.ToString().Trim();
                    var ex_Cell = odf.Cells[cell_rs.Item1, cell_rs.Item2];
                    var cell = ex_Cell.Name;
                    var cell_range = odf.Cells[cell_rs.Item1, cell_rs.Item2].MergedRange; //благодаря этому свойству можно определить границы строки, если ячейка объединена
                    CellRange RS_range = null;
                    var row = new RowGroup
                    {
                        Cell = cell,
                        Name = rs_content
                    };
                    if (cell_range != null)    //и если она не null
                        RS_range = Range.GetSubrangeAbsolute(cell_range.FirstRowIndex, cell_range.FirstColumnIndex, cell_range.LastRowIndex, Range.LastColumnIndex);
                    else
                        RS_range = Range.GetSubrangeAbsolute(ex_Cell.Row, ex_Cell.Column, ex_Cell.Row, Range.LastColumnIndex);
                    row.Width = RS_range?.Width ?? 1;
                    row.Height = RS_range?.Height ?? 1;
                    row.Range = RS_range ?? Range;      //мера предосторожности, чтобы не было null параметра при поиске
                    result.Add(row);
                    odf.Cells[cell_rs.Item1, cell_rs.Item2].Value = rs_content;
                }
            }
            return result;
        }
        private ObservableCollection<ColumnGroup> SearchColumnGroups()
        {
            var result = new ObservableCollection<ColumnGroup>();
            var cell_сs = (0, 0);
            if (Range.FindText(regex_cs, out cell_сs.Item1, out cell_сs.Item2))
            {
                while (Range.FindText(regex_cs, out cell_сs.Item1, out cell_сs.Item2))
                {
                    var cs_matches = regex_cs.Matches(odf.Cells[cell_сs.Item1, cell_сs.Item2].Value.ToString())[0].Groups;
                    var cs_content = cs_matches[1].Value.ToString().Trim();
                    var ex_Cell = odf.Cells[cell_сs.Item1, cell_сs.Item2];
                    var cell = ex_Cell.Name;
                    var cell_range = odf.Cells[cell_сs.Item1, cell_сs.Item2].MergedRange; //благодаря этому свойству можно определить границы строки, если ячейка объединена
                    CellRange CS_Range = null;
                    var column = new ColumnGroup
                    {
                        Cell = cell,
                        Name = cs_content
                    };
                    if (cell_range != null)
                        CS_Range = Range.GetSubrangeAbsolute(cell_range.FirstRowIndex, cell_range.FirstColumnIndex, Range.LastRowIndex, cell_range.LastColumnIndex);
                    else
                        CS_Range = Range.GetSubrangeAbsolute(ex_Cell.Row, ex_Cell.Column, Range.LastRowIndex, ex_Cell.Column);
                    column.Width = CS_Range?.Width ?? 1;
                    column.Height = CS_Range?.Height ?? 1;
                    column.Range = CS_Range ?? Range;
                    result.Add(column);
                    odf.Cells[cell_сs.Item1, cell_сs.Item2].Value = cs_content;
                }
            }
            return result;
        }
        public ObservableCollection<TableGroup> TableGroups { get; set; }
        public ObservableCollection<CellResult> TableCells { get; set; }
    }
}