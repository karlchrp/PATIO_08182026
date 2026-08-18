using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CsvProcessing.Api.Controllers;

[ApiController]
[Route("api/v1/files")]
public class FilesController : ControllerBase
{
    [HttpPost("process")]
    public async Task<ActionResult<int>> ProcessAsync()
    {
        return Ok(1);
    }
}