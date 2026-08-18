namespace CsvProcessing.Application.Tracking
{
    public sealed record ProcessedFileRecord(
        string FileName,
        long SizeInBytes,
        string operation,
        DateTimeOffset ProcessDateTime,
        double DurationMs);
}
