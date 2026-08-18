namespace CsvProcessing.Application.Csv;

public interface ICsvFileProcessor
{
    Task<CsvAggregateResult> ProcessAsync(Stream csv, string column, string operation, CancellationToken cancellationToken);
}
