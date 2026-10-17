
namespace AiOrderAgent.Services;

public class VectorSearchService
{
    private readonly EmbeddingService _embeddingService;
    private readonly PostgresVectorStore _vectorStore;

    public VectorSearchService(
        EmbeddingService embeddingService,
        PostgresVectorStore vectorStore)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public async Task<List<DocumentChunk>> SearchAsync(
        string question,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException(
                "Question cannot be empty.",
                nameof(question));
        }

        var queryEmbedding =
            await _embeddingService.CreateEmbeddingAsync(
                question, cancellationToken);

        return await _vectorStore.SearchAsync(
            queryEmbedding, topK, cancellationToken);
    }
}
