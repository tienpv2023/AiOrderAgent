
using Npgsql;
using Pgvector;
using Pgvector.Npgsql;

namespace AiOrderAgent.Services;

public class PostgresVectorStore
{
    private readonly string _connectionString;

    public PostgresVectorStore(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Missing Postgres connection string.");
    }

    public async Task<bool> TestConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command = new NpgsqlCommand(
            "SELECT 1", connection);

        var result = await command.ExecuteScalarAsync(
            cancellationToken);

        return Convert.ToInt32(result) == 1;
    }

    public async Task<List<DocumentChunk>> SearchAsync(
        float[] queryEmbedding,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        if (queryEmbedding.Length != 1536)
        {
            throw new ArgumentException(
                "Embedding must contain 1536 dimensions.",
                nameof(queryEmbedding));
        }

        var results = new List<DocumentChunk>();

        //await using var dataSource =
        //    new NpgsqlDataSourceBuilder(_connectionString)
        //        .UseVector()
        //        .Build();

        //  Đoạn mã mới đã sửa ổn định
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
        dataSourceBuilder.UseVector(); // Kích hoạt pgvector

        await using var dataSource = dataSourceBuilder.Build(); // Gọi Build từ builder


        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken); 

        const string sql = """
            SELECT id, document_name, content,
                   embedding <=> @query AS distance
            FROM document_chunks
            WHERE embedding IS NOT NULL
            ORDER BY embedding <=> @query
            LIMIT @topK;
            """;

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "query", new Vector(queryEmbedding));

        command.Parameters.AddWithValue(
            "topK", Math.Clamp(topK, 1, 20));

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new DocumentChunk
            {
                Id = reader.GetInt64(0),
                DocumentName = reader.GetString(1),
                Content = reader.GetString(2),
                Distance = reader.GetDouble(3)
            });
        }

        return results;
    }

    public async Task<List<DocumentChunk>> KeywordSearchAsync(
        string question,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        var results = new List<DocumentChunk>();

        //  Đoạn mã mới đã sửa ổn định
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
        dataSourceBuilder.UseVector(); // Kích hoạt pgvector

        await using var dataSource = dataSourceBuilder.Build(); // Gọi Build từ builder


        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
        SELECT id, document_name, content,
               ts_rank(
                   to_tsvector('simple', content),
                   plainto_tsquery('simple', @question)
               ) AS score
        FROM document_chunks
        WHERE to_tsvector('simple', content)
              @@ plainto_tsquery('simple', @question)
        ORDER BY score DESC
        LIMIT @topK;
        """;

        await using var command =
            new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue("question", question);
        command.Parameters.AddWithValue(
            "topK", Math.Clamp(topK, 1, 20));

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new DocumentChunk
            {
                Id = reader.GetInt64(0),
                DocumentName = reader.GetString(1),
                Content = reader.GetString(2),
                Distance = 1.0 - reader.GetDouble(3)
            });
        }

        return results;
    }


    public async Task SaveEmbeddingAsync(
        long id,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        if (embedding.Length != 1536)
        {
            throw new ArgumentException(
                "Embedding must contain 1536 dimensions.",
                nameof(embedding));
        }

        //await using var dataSource =
        //    new NpgsqlDataSourceBuilder(_connectionString)
        //        .UseVector()
        //        .Build();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(_connectionString);
        dataSourceBuilder.UseVector(); // Kích hoạt pgvector

        await using var dataSource = dataSourceBuilder.Build(); // Gọi Build từ builder

        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        const string sql = """
        UPDATE document_chunks
        SET embedding = @embedding
        WHERE id = @id;
        """;

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "id", id);

        command.Parameters.AddWithValue(
            "embedding", new Vector(embedding));

        var affectedRows = await command.ExecuteNonQueryAsync(
            cancellationToken);

        if (affectedRows == 0)
        {
            throw new InvalidOperationException(
                $"Document chunk with id {id} was not found.");
        }
    }


}
public class DocumentChunk
{
    public long Id { get; set; }
    public string DocumentName { get; set; } = "";
    public string Content { get; set; } = "";
    public double Distance { get; set; }
    public string TenantId { get; set; } = "default";
    
}
