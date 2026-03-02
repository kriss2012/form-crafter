using FormMaker.Shared.Enums;

namespace FormMaker.Shared.Models.Elements;

/// <summary>
/// Table/Grid element for structured data
/// </summary>
public class TableElement : FormElement
{
    public TableElement()
    {
        Type = ElementType.Table;
        Width = 600;
        Height = 200;
        Rows = 3;
        Columns = 3;
        Headers = new List<string> { "Column 1", "Column 2", "Column 3" };
        CellData = InitializeCellData(3, 3);
        ShowHeaders = true;
        BorderColor = "#cccccc";
    }

    /// <summary>
    /// Number of rows (excluding header)
    /// </summary>
    public int Rows { get; set; }

    /// <summary>
    /// Number of columns
    /// </summary>
    public int Columns { get; set; }

    /// <summary>
    /// Column header labels
    /// </summary>
    public List<string> Headers { get; set; }

    /// <summary>
    /// Cell data stored as 2D array [row][column]
    /// </summary>
    public List<List<string>> CellData { get; set; }

    /// <summary>
    /// Show header row
    /// </summary>
    public bool ShowHeaders { get; set; }

    /// <summary>
    /// Border color for table cells
    /// </summary>
    public string BorderColor { get; set; }

    private static List<List<string>> InitializeCellData(int rows, int columns)
    {
        var data = new List<List<string>>();
        for (int i = 0; i < rows; i++)
        {
            var row = new List<string>();
            for (int j = 0; j < columns; j++)
            {
                row.Add("");
            }
            data.Add(row);
        }
        return data;
    }

    public void AddRow()
    {
        var newRow = new List<string>();
        for (int i = 0; i < Columns; i++)
        {
            newRow.Add("");
        }
        CellData.Add(newRow);
        Rows++;
    }

    public void RemoveRow()
    {
        if (Rows > 1)
        {
            CellData.RemoveAt(CellData.Count - 1);
            Rows--;
        }
    }

    public void AddColumn()
    {
        Headers.Add($"Column {Columns + 1}");
        foreach (var row in CellData)
        {
            row.Add("");
        }
        Columns++;
    }

    public void RemoveColumn()
    {
        if (Columns > 1)
        {
            Headers.RemoveAt(Headers.Count - 1);
            foreach (var row in CellData)
            {
                row.RemoveAt(row.Count - 1);
            }
            Columns--;
        }
    }

    public override FormElement Clone()
    {
        var clonedCellData = new List<List<string>>();
        foreach (var row in CellData)
        {
            clonedCellData.Add(new List<string>(row));
        }

        return new TableElement
        {
            Id = Guid.NewGuid(), // New ID for cloned element
            Type = this.Type,
            X = this.X + 10, // Offset slightly
            Y = this.Y + 10,
            Width = this.Width,
            Height = this.Height,
            Properties = this.Properties.Clone(),
            Label = this.Label,
            IsRequired = this.IsRequired,
            Rows = this.Rows,
            Columns = this.Columns,
            Headers = new List<string>(this.Headers),
            CellData = clonedCellData,
            ShowHeaders = this.ShowHeaders,
            BorderColor = this.BorderColor
        };
    }

    public override string GetDisplayName()
    {
        return "Table";
    }
}
