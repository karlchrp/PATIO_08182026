namespace CsvProcessing.Application.Tracking
{
    public interface IFileProcessingTracker
    {
        void Record(ProcessedFileRecord record);
        ProcessingReport GetReport();
    }
}
