
using OpenAI.Embeddings;

namespace AiOrderAgent.Services;

public class EmbeddingService
{
    private readonly EmbeddingClient _client;

    public EmbeddingService(IConfiguration configuration)
    {
        var apiKey = configuration["OPENAI_API_KEY"]
            ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OPENAI_API_KEY is not configured.");
        }

        _client = new EmbeddingClient(
            "text-embedding-3-small",
            apiKey);
    }

    public async Task<float[]> CreateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var result = await _client.GenerateEmbeddingAsync(
            text,
            cancellationToken: cancellationToken);

        return result.Value.ToFloats().ToArray();
    }
}
