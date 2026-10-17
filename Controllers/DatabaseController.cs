
using AiOrderAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiOrderAgent.Controllers;

[ApiController]
[Route("api/database")]
public class DatabaseController : ControllerBase
{
    private readonly PostgresVectorStore _store;

    public DatabaseController(PostgresVectorStore store)
    {
        _store = store;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test(
        CancellationToken cancellationToken)
    {
        var connected = await _store.TestConnectionAsync(
            cancellationToken);

        return Ok(new { connected });
    }
}
