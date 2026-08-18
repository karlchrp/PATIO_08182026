namespace CsvProcessing.Application.Csv;

public class InvalidCsvException : Exception
{
    public InvalidCsvException(string message) : base(message)
    {
    }
}
