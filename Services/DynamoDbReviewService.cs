using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using CatalogAPI.Models;

namespace CatalogAPI.Services;

public class DynamoDbReviewService
{
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly DynamoDBContext _context;
    private readonly string _tableName;
    private static bool _tableInitialized;

    public DynamoDbReviewService(IAmazonDynamoDB dynamoDbClient, IConfiguration configuration)
    {
        _dynamoDbClient = dynamoDbClient;
        _tableName = configuration["DynamoDB:TableName"] ?? "GameReviews";
        _context = new DynamoDBContextBuilder()
            .WithDynamoDBClient(() => dynamoDbClient)
            .Build();
    }

    private async Task EnsureTableExistsAsync()
    {
        if (_tableInitialized) return;

        try
        {
            await _dynamoDbClient.DescribeTableAsync(_tableName);
            _tableInitialized = true;
        }
        catch (ResourceNotFoundException)
        {
            var request = new CreateTableRequest
            {
                TableName = _tableName,
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new("GameId", ScalarAttributeType.S),
                    new("ReviewId", ScalarAttributeType.S)
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new("GameId", KeyType.HASH),
                    new("ReviewId", KeyType.RANGE)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };

            await _dynamoDbClient.CreateTableAsync(request);
            _tableInitialized = true;
        }
    }

    public async Task AddReviewAsync(GameReview review)
    {
        await EnsureTableExistsAsync();
        await _context.SaveAsync(review);
    }

    public async Task<List<GameReview>> GetReviewsByGameIdAsync(Guid gameId)
    {
        await EnsureTableExistsAsync();
        return await _context.QueryAsync<GameReview>(gameId.ToString()).GetRemainingAsync();
    }
}