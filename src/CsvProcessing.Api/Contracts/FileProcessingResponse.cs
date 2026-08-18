namespace CsvProcessing.Api.Contracts
{
    public sealed record FileProcessingResponse(
    Guid ProcessingId,
    string FileName,
    long SizeInBytes,
    string Format,
    string Operation,
    DateTimeOffset ProcessDateTime,
    double DurationMs,
    object Result);
}
