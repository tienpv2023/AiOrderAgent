
using AiOrderAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiOrderAgent.Controllers;

[ApiController]
[Route("api/ingestion")]
public class IngestionController : ControllerBase
{
    private readonly DocumentIngestionService _service;

    public IngestionController(DocumentIngestionService service)
    {
        _service = service;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(
        CancellationToken cancellationToken)
    {
        var count = await _service.IngestPendingAsync(
            cancellationToken);

        return Ok(new
        {
            processed = count,
            message = "Embedding ingestion completed."
        });
    }
}
