using ODFTablesLib;

namespace ANG24.Sys.Infrastructure.Helpers;
public class CellResult
{
    public ColumnGroup Column { get; set; }
    public RowGroup Row { get; set; }
    internal Cell RealCell;
    public string Cell { get; set; }
    public string Mode { get; set; }
    public string Module { get; set; }
    public string Phaze { get; set; }
    public string Parameter { get; set; }
    public string Prefix { get; set; }
    public string Value { get; set; }

    public void AddColumn(ColumnGroup column)
    {
        column.AddCell(this);
        Column = column;
    }
    public void AddRow(RowGroup row)
    {
        row.AddCell(this);
        Row = row;
    }
}