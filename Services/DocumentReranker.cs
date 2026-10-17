
namespace AiOrderAgent.Services;

public class DocumentReranker
{
    public List<DocumentChunk> Rerank(
        string question,
        IEnumerable<DocumentChunk> documents,
        int topK = 5)
    {
        var terms = question
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant())
            .Where(x => x.Length > 1)
            .Distinct()
            .ToArray();

        return documents
            .Select(document => new
            {
                Document = document,
                Score = terms.Count(term =>
                    document.Content.Contains(
                        term,
                        StringComparison.OrdinalIgnoreCase))
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Document.Distance)
            .Take(Math.Clamp(topK, 1, 20))
            .Select(x => x.Document)
            .ToList();
    }
}
