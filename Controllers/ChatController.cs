using AiOrderAgent.Models;
using AiOrderAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiOrderAgent.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly AiAgentService _agent;

    public ChatController(AiAgentService agent)
    {
        _agent = agent;
    }

    [HttpPost]
    public async Task<IActionResult> Chat(
        ChatRequest request)
    {
        var answer =
            await _agent.AskAsync(request.Message);

        return Ok(new
        {
            answer
        });
    }
}