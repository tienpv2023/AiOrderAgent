
using AiOrderAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiOrderAgent.Controllers;

[ApiController]
[Route("api/rag")]
public class RagController : ControllerBase
{
    private readonly RagAnswerService _ragService;

    public RagController(RagAnswerService ragService)
    {
        _ragService = ragService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(
        [FromBody] RagRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new
            {
                message = "Question is required."
            });
        }

        var answer = await _ragService.AskAsync(
            request.Question, cancellationToken);

        return Ok(new { answer });
    }
}

public class RagRequest
{
    public string Question { get; set; } = "";
}
