namespace CsvProcessing.Application.Tracking
{
    public sealed class InMemoryFileProcessingTracker : IFileProcessingTracker
    {
        private readonly object _gate = new();
        private readonly List<ProcessedFileRecord> _records = new();

        public void Record(ProcessedFileRecord record)
        {
            lock (_gate)
            {
                _records.Add(record);
            }
        }

        public ProcessingReport GetReport()
        {
            lock (_gate)
            {
                return new ProcessingReport(_records.Count, _records.ToList());
            }
        }
    }
}
