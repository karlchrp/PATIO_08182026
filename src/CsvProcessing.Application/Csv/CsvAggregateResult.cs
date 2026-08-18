namespace CsvProcessing.Application.Csv;

public sealed record CsvAggregateResult(
    string Column,
    string Operation,
    decimal Value);
