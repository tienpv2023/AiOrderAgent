
using Npgsql;

namespace AiOrderAgent.Services;

public class DocumentIngestionService
{
    private readonly string _connectionString;
    private readonly EmbeddingService _embeddingService;
    private readonly PostgresVectorStore _vectorStore;

    public DocumentIngestionService(
        IConfiguration configuration,
        EmbeddingService embeddingService,
        PostgresVectorStore vectorStore)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Missing Postgres connection string.");

        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
    }

    public async Task<int> IngestPendingAsync(
        CancellationToken cancellationToken = default)
    {
        var chunks = new List<(long Id, string Content)>();

        await using (var connection =
            new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                SELECT id, content
                FROM document_chunks
                WHERE embedding IS NULL
                ORDER BY id;
                """;

            await using var command =
                new NpgsqlCommand(sql, connection);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                chunks.Add((
                    reader.GetInt64(0),
                    reader.GetString(1)));
            }
        }

        foreach (var chunk in chunks)
        {
            var embedding =
                await _embeddingService.CreateEmbeddingAsync(
                    chunk.Content, cancellationToken);

            await _vectorStore.SaveEmbeddingAsync(
                chunk.Id, embedding, cancellationToken);
        }

        return chunks.Count;
    }
}
