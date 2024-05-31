using Skyline.DataMiner.Analytics.GenericInterface;
using System;

[GQIMetaData(Name = "Sum columns")]
public sealed class SumColumnsOperator : IGQIRowOperator, IGQIColumnOperator, IGQIInputArguments
{
    private readonly GQIColumnListArgument _columnsArg;
    private IAdder _adder;

    /// <summary>
    /// Initializes a new instance of the <see cref="SumColumnsOperator"/> class.
    /// Initializes the <see cref="_columnsArg"/> with a required argument to select numeric columns.
    /// Called by GQI and should be parameterless.
    /// </summary>
    public SumColumnsOperator()
    {
        _columnsArg = new GQIColumnListArgument("Columns")
        {
            IsRequired = true,
            Types = new[] { GQIColumnType.Int, GQIColumnType.Double },
        };
    }

    /// <summary>
    /// Called by GQI to define the input arguments.
    /// Defines an argument to select one or more numeric columns.
    /// </summary>
    /// <returns>The defined arguments.</returns>
    public GQIArgument[] GetInputArguments()
    {
        return new[] { _columnsArg };
    }

    /// <summary>
    /// Called by GQI to expose the chosen argument values.
    /// </summary>
    /// <param name="args">Collection of chosen argument values.</param>
    /// <returns>Unused.</returns>
    public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
    {
        var columns = args.GetArgumentValue(_columnsArg);
        _adder = CreateAdder(columns);

        return default;
    }

    /// <summary>
    /// Called by GQI to determine the resulting columns.
    /// Lets the type specific <see cref="_adder"/> add a new column that will contain the sum.
    /// </summary>
    /// <param name="header">The current collection of columns that should be modified.</param>
    public void HandleColumns(GQIEditableHeader header)
    {
        _adder.HandleColumns(header);
    }

    /// <summary>
    /// Called by GQI to handle each <paramref name="row"/> in turn.
    /// Lets the type specific <see cref="_adder"/> to calculate the sum and save it in the <paramref name="row"/>.
    /// </summary>
    /// <param name="row">The next row that needs to be handled.</param>
    public void HandleRow(GQIEditableRow row)
    {
        _adder.HandleRow(row);
    }

    /// <summary>
    /// Creates type specific operator logic that can add the selected <paramref name="inputColumns"/> together.
    /// </summary>
    /// <param name="inputColumns">Columns to add together.</param>
    /// <returns>The type specific logic to add the <paramref name="inputColumns"/>.</returns>
    /// <exception cref="Exception">If the <paramref name="inputColumns"/> contains non-numeric columns.</exception>
    private IAdder CreateAdder(GQIColumn[] inputColumns)
    {
        var sumAsDouble = false;
        foreach (var column in inputColumns)
        {
            switch (column)
            {
                case GQIColumn<int> intColumn:
                    continue;
                case GQIColumn<double> doubleColumn:
                    sumAsDouble = true;
                    continue;
                case null:
                    throw new Exception("An invalid column was selected.");
                default:
                    throw new Exception($"Column '{column.Name}' is not numeric.");
            }
        }

        if (sumAsDouble)
            return new DoubleAdder(inputColumns);
        else
            return new IntAdder(inputColumns);
    }
}