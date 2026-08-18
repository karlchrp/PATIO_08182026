using System.Globalization;

namespace CsvProcessing.Application.Csv;

public sealed class CsvFileProcessor : ICsvFileProcessor
{
    private static readonly string[] SupportedOperations = { "average", "sum", "count" };

    public async Task<CsvAggregateResult> ProcessAsync(
        Stream csv,
        string column,
        string operation,
        CancellationToken cancellationToken)
    {
        operation = operation.Trim().ToLowerInvariant();

        if (!SupportedOperations.Contains(operation))
        {
            throw new InvalidCsvException(
                $"Operation '{operation}' is not supported. Use one of: {string.Join(", ", SupportedOperations)}.");
        }

        using var reader = new StreamReader(csv, leaveOpen: true);

        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (headerLine is null)
        {
            throw new InvalidCsvException("The CSV file is empty.");
        }

        var header = headerLine.Split(',');

        var columnIndex = Array.FindIndex(
            header,
            name => name.Trim().Equals(column, StringComparison.OrdinalIgnoreCase));

        if (columnIndex < 0)
        {
            throw new InvalidCsvException(
                $"Column '{column}' was not found. Available columns: {string.Join(", ", header)}.");
        }

        var rowCount = 0;
        var sum = 0m;

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            rowCount++;

            var fields = line.Split(',');

            if (columnIndex >= fields.Length)
            {
                throw new InvalidCsvException($"Row {rowCount} has fewer columns than the header.");
            }

            if (!decimal.TryParse(fields[columnIndex], CultureInfo.InvariantCulture, out var parsed))
            {
                throw new InvalidCsvException(
                    $"Row {rowCount}: '{fields[columnIndex]}' in column '{column}' is not a number.");
            }

            sum += parsed;
        }

        if (rowCount == 0)
        {
            throw new InvalidCsvException("The CSV file contains no data rows.");
        }

        var value = operation switch
        {
            "average" => Math.Round(sum / rowCount, 6),
            "sum" => sum,
            "count" => rowCount,
            _ => 0m
        };

        return new CsvAggregateResult(header[columnIndex].Trim(), operation, value);
    }
}
