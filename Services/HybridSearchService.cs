
namespace AiOrderAgent.Services;

public class HybridSearchService
{
    private readonly EmbeddingService _embeddingService;
    private readonly PostgresVectorStore _vectorStore;

    public HybridSearchService(
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

        var embeddingTask =
            _embeddingService.CreateEmbeddingAsync(
                question, cancellationToken);

        var keywordTask =
            _vectorStore.KeywordSearchAsync(
                question, 10, cancellationToken);

        await Task.WhenAll(embeddingTask, keywordTask);

        var vectorResults =
            await _vectorStore.SearchAsync(
                await embeddingTask, 10, cancellationToken);

        var keywordResults = await keywordTask;

        const int rrfK = 60;

        var scores = new Dictionary<long, double>();
        var documents = new Dictionary<long, DocumentChunk>();

        AddRanks(vectorResults, scores, documents, rrfK);
        AddRanks(keywordResults, scores, documents, rrfK);

        return scores
            .OrderByDescending(x => x.Value)
            .Take(Math.Clamp(topK, 1, 20))
            .Select(x => documents[x.Key])
            .ToList();
    }

    private static void AddRanks(
        List<DocumentChunk> results,
        Dictionary<long, double> scores,
        Dictionary<long, DocumentChunk> documents,
        int rrfK)
    {
        for (var i = 0; i < results.Count; i++)
        {
            var document = results[i];

            scores.TryAdd(document.Id, 0);
            scores[document.Id] += 1.0 / (rrfK + i + 1);
            documents.TryAdd(document.Id, document);
        }
    }
}
