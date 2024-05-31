using Skyline.DataMiner.Analytics.GenericInterface;
using System;

/// <summary>
/// Represents a type specific GQI operator that can add columns together.
/// </summary>
internal interface IAdder : IGQIColumnOperator, IGQIRowOperator
{
}

/// <summary>
/// Common logic for all numeric adders.
/// </summary>
/// <typeparam name="TNumeric">Numeric type.</typeparam>
internal abstract class Adder<TNumeric> : IAdder
{
    protected static readonly string OutputColumnName = "Sum";

    public Adder(GQIColumn[] inputColumns, GQIColumn<TNumeric> outputColumn)
    {
        InputColumns = inputColumns;
        OutputColumn = outputColumn;
    }

    public GQIColumn[] InputColumns { get; }

    public GQIColumn<TNumeric> OutputColumn { get; }

    public void HandleColumns(GQIEditableHeader header)
    {
        header.AddColumns(OutputColumn);
    }

    /// <summary>
    /// Calculate the sum for this specific <paramref name="row"/> and write it to the <see cref="OutputColumn"/>.
    /// </summary>
    /// <param name="row">The row for which the sum needs to be calculated.</param>
    public void HandleRow(GQIEditableRow row)
    {
        TNumeric sum = GetSum(row);
        row.SetValue(OutputColumn, sum);
    }

    /// <summary>
    /// Calculate the sum for this specific <paramref name="row"/>.
    /// </summary>
    /// <param name="row">The row for which the sum needs to be calculated.</param>
    /// <returns>The type specific sum.</returns>
    protected abstract TNumeric GetSum(GQIEditableRow row);
}

/// <summary>
/// Adder to sum columns of type <see cref="int"/> to a sum of type <see cref="int"/>.
/// </summary>
internal sealed class IntAdder : Adder<int>
{
    public IntAdder(GQIColumn[] inputColumns) : base(inputColumns, new GQIIntColumn(OutputColumnName))
    {
    }

    protected override int GetSum(GQIEditableRow row)
    {
        int sum = 0;
        foreach (var column in InputColumns)
        {
            if (row.TryGetValue(column, out int value))
                sum += value;
        }

        return sum;
    }
}

/// <summary>
/// Adder to sum columns of type <see cref="int"/> or <see cref="double"/> to a sum of type <see cref="double"/>.
/// </summary>
internal sealed class DoubleAdder : Adder<double>
{
    public DoubleAdder(GQIColumn[] inputColumns) : base(inputColumns, new GQIDoubleColumn(OutputColumnName))
    {
    }

    protected override double GetSum(GQIEditableRow row)
    {
        double sum = 0;
        foreach (var column in InputColumns)
        {
            // Get value as object, since it could be either int or double
            if (row.TryGetValue(column, out object objectValue))
                sum += Convert.ToDouble(objectValue);
        }

        return sum;
    }
}
