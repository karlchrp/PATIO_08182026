using CsvProcessing.Api.Contracts;
using CsvProcessing.Application.Csv;
using CsvProcessing.Application.Tracking;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CsvProcessing.Api.Controllers;

[ApiController]
[Route("api/v1/files")]
[Produces("application/json")]
public class FilesController : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private readonly ICsvFileProcessor _processor;
    private readonly IFileProcessingTracker _tracker;
    private readonly ILogger<FilesController> _logger;

    public FilesController(ICsvFileProcessor processor, IFileProcessingTracker tracker, ILogger<FilesController> logger)
    {
        _processor = processor;
        _tracker = tracker;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a CSV file and returns an aggregate of one numeric column.
    /// </summary>
    /// <param name="file">The CSV file, sent as multipart/form-data.</param>
    /// <param name="column">Name of the column to aggregate, e.g. Amount.</param>
    /// <param name="operation">average, sum, min, max or count.</param>
    [HttpPost("process")]
    [ProducesResponseType(typeof(FileProcessingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FileProcessingResponse>> ProcessAsync(
        IFormFile? file,
        [FromQuery] string column,
        [FromQuery] string operation = "average",
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return Problem(
                title: "Missing file",
                detail: "Send the CSV file as multipart/form-data using the form field name 'file'.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return Problem(
                title: "Payload too large",
                detail: $"The file is {file.Length} bytes, which exceeds the {MaxFileSizeBytes} byte limit.",
                statusCode: StatusCodes.Status413PayloadTooLarge);
        }

        var fileName = Path.GetFileName(file.FileName);

        if (!fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Problem(
                title: "Unsupported file format",
                detail: "Only .csv files are accepted.",
                statusCode: StatusCodes.Status415UnsupportedMediaType);
        }

        var processingId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        await using var content = file.OpenReadStream();

        var result = await _processor.ProcessAsync(content, column, operation, cancellationToken);

        stopwatch.Stop();
        _tracker.Record(new ProcessedFileRecord(fileName, file.Length, operation, DateTimeOffset.UtcNow, stopwatch.Elapsed.TotalMilliseconds));

        _logger.LogInformation(
            "Processed {FileName} ({SizeInBytes} bytes) in {DurationMs:F1} ms.",
            fileName,
            file.Length,
            stopwatch.Elapsed.TotalMilliseconds);

        return Ok(new FileProcessingResponse(
            ProcessingId: processingId,
            FileName: fileName,
            SizeInBytes: file.Length,
            Format: "csv",
            Operation: operation,
            ProcessDateTime: DateTimeOffset.UtcNow,
            DurationMs: Math.Round(stopwatch.Elapsed.TotalMilliseconds, 3),
            Result: result));
    }
}
