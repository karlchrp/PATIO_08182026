using CsvProcessing.Application.Csv;
using CsvProcessing.Application.Tracking;
using Microsoft.AspNetCore.Mvc;

namespace CsvProcessing.Api.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IFileProcessingTracker _tracker;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IFileProcessingTracker tracker, ILogger<ReportsController> logger)
        {
            _tracker = tracker;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ProcessingReport>> GetReports(CancellationToken cancellationToken = default)
        {
            return Ok(_tracker.GetReport());
        }
    }
}
