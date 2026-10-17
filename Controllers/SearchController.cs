
using AiOrderAgent.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiOrderAgent.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly VectorSearchService _searchService;

    public SearchController(VectorSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpPost("vector")]
    public async Task<IActionResult> Search(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new
            {
                message = "Question is required."
            });
        }

        var results = await _searchService.SearchAsync(
            request.Question,
            5,
            cancellationToken);

        return Ok(results);
    }
}

public class SearchRequest
{
    public string Question { get; set; } = "";
}
