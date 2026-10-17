
using System.Text;

namespace AiOrderAgent.Services;

public class ContextBuilder
{
    public string Build(
        IEnumerable<DocumentChunk> documents,
        int maxCharacters = 8000)
    {
        var context = new StringBuilder();

        foreach (var document in documents)
        {
            var item = $"[Source: {document.DocumentName}]\n" +
                       $"{document.Content}\n\n";

            if (context.Length + item.Length > maxCharacters)
            {
                break;
            }

            context.Append(item);
        }

        return context.ToString();
    }
}
