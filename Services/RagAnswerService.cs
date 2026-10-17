
namespace AiOrderAgent.Services;

public class RagAnswerService
{
    private readonly HybridSearchService _hybridSearch;
    private readonly ContextBuilder _contextBuilder;
    private readonly LlmService _llm;

    public RagAnswerService(
        HybridSearchService hybridSearch,
        ContextBuilder contextBuilder,
        LlmService llm)
    {
        _hybridSearch = hybridSearch;
        _contextBuilder = contextBuilder;
        _llm = llm;
    }

    public async Task<string> AskAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        // 1. Tìm tài liệu liên quan
        var documents = await _hybridSearch.SearchAsync(
            question, 20, cancellationToken);

        // 2. Tạo Context
        var context = _contextBuilder.Build(documents);

        if (string.IsNullOrWhiteSpace(context))
        {
            return "Tôi chưa tìm thấy tài liệu phù hợp để trả lời.";
        }

        // 3. Gọi LLM
        return await _llm.GenerateAnswerAsync(
            question, context, cancellationToken);
    }
}
