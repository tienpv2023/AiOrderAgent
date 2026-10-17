//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//var summaries = new[]
//{
//    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
//};

//app.MapGet("/weatherforecast", () =>
//{
//    var forecast =  Enumerable.Range(1, 5).Select(index =>
//        new WeatherForecast
//        (
//            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//            Random.Shared.Next(-20, 55),
//            summaries[Random.Shared.Next(summaries.Length)]
//        ))
//        .ToArray();
//    return forecast;
//})
//.WithName("GetWeatherForecast");

//app.Run();

//record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
//{
//    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
//}

#pragma warning disable OPENAI001

using AiOrderAgent.Data;
using AiOrderAgent.Services;
using AiOrderAgent.Tools;
using Microsoft.EntityFrameworkCore;
using OpenAI.Responses;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.AddScoped<PostgresVectorStore>();
builder.Services.AddScoped<EmbeddingService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<GetOrderStatusTool>();
builder.Services.AddScoped<DocumentSecurityFilter>();
builder.Services.AddScoped<DocumentReranker>();
builder.Services.AddScoped<ContextBuilder>();
builder.Services.AddScoped<AiAgentService>();
builder.Services.AddScoped<LlmService>();
builder.Services.AddScoped<RagAnswerService>();
builder.Services.AddScoped<VectorSearchService>();
builder.Services.AddScoped<DocumentIngestionService>();
builder.Services.AddScoped<HybridSearchService>();
builder.Services.AddSingleton(
    new ResponsesClient(
        Environment.GetEnvironmentVariable(
            "OPENAI_API_KEY")
        ?? throw new InvalidOperationException(
            "OPENAI_API_KEY is not configured")));
var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();