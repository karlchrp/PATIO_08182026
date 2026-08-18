namespace CsvProcessing.Application.Tracking
{
    public sealed record ProcessingReport(
        int TotalFilesProcessed,
        IReadOnlyList<ProcessedFileRecord> Files);
}
