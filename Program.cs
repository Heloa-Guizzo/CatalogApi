using CatalogAPI;
using CatalogAPI.Data;
using CatalogAPI.Middlewares;
using CatalogAPI.Repositories;
using CatalogAPI.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Amazon.DynamoDBv2;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")));

// Repositories
builder.Services.AddScoped<IGameRepository,GameRepository>();

builder.Services.AddScoped<IUserLibraryRepository, UserLibraryRepository>();

// Services
builder.Services.AddScoped<GameService>();

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    options.InstanceName = "FCG_Catalog_";
});

// DynamoDB 
builder.Services.AddSingleton<IAmazonDynamoDB>(_ =>
{
    var serviceUrl = builder.Configuration["DynamoDB:ServiceUrl"];
    if (!string.IsNullOrWhiteSpace(serviceUrl))
    {
        var config = new AmazonDynamoDBConfig { ServiceURL = serviceUrl };
        return new AmazonDynamoDBClient("local", "local", config);
    }

    return new AmazonDynamoDBClient(Amazon.RegionEndpoint.USEast1);
});
builder.Services.AddScoped<DynamoDbReviewService>();

// Elasticsearch 
builder.Services.AddSingleton(_ =>
{
    var uri = new Uri(builder.Configuration["Elasticsearch:Uri"] ?? "http://localhost:9200");
    var apiKey = builder.Configuration["Elasticsearch:ApiKey"];
    var settings = new ElasticsearchClientSettings(uri);

    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        settings = settings.Authentication(new ApiKey(apiKey));
    }

    return new ElasticsearchClient(settings);
});
builder.Services.AddScoped<ElasticsearchService>();

// RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentProcessedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Middlewares
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Swagger

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();