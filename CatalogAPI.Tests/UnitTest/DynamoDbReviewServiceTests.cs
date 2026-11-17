using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using CatalogAPI.Models;
using CatalogAPI.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CatalogAPI.Tests;

public class DynamoDbReviewServiceTests
{
    private static void ResetStaticTableFlag()
    {
        var field = typeof(DynamoDbReviewService).GetField(
            "_tableInitialized",
            BindingFlags.NonPublic | BindingFlags.Static);

        field?.SetValue(null, false);
    }

    private static IConfiguration BuildConfiguration(string? tableName = "GameReviewsTest")
    {
        var settings = new Dictionary<string, string?>();
        if (tableName is not null)
        {
            settings["DynamoDB:TableName"] = tableName;
        }

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public async Task AddReviewAsync_ShouldCreateTableWithHashAndRangeKeys_WhenTableDoesNotExist()
    {
        ResetStaticTableFlag();

        var dynamoMock = new Mock<IAmazonDynamoDB>();
        var config = BuildConfiguration("GameReviewsCustom");

        dynamoMock
            .Setup(d => d.DescribeTableAsync("GameReviewsCustom", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("Table not found"));

        dynamoMock
            .Setup(d => d.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateTableResponse());

        var service = new DynamoDbReviewService(dynamoMock.Object, config);
        var review = new GameReview
        {
            GameId = Guid.NewGuid().ToString(),
            ReviewId = Guid.NewGuid().ToString()
        };

        await Record.ExceptionAsync(() => service.AddReviewAsync(review));

        dynamoMock.Verify(d => d.CreateTableAsync(
            It.Is<CreateTableRequest>(req =>
                req.TableName == "GameReviewsCustom" &&
                req.BillingMode == BillingMode.PAY_PER_REQUEST &&
                req.KeySchema.Any(k => k.AttributeName == "GameId" && k.KeyType == KeyType.HASH) &&
                req.KeySchema.Any(k => k.AttributeName == "ReviewId" && k.KeyType == KeyType.RANGE)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddReviewAsync_ShouldNotCreateTable_WhenTableAlreadyExists()
    {
        ResetStaticTableFlag();

        var dynamoMock = new Mock<IAmazonDynamoDB>();
        var config = BuildConfiguration("GameReviewsExisting");

        dynamoMock
            .Setup(d => d.DescribeTableAsync("GameReviewsExisting", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeTableResponse());

        var service = new DynamoDbReviewService(dynamoMock.Object, config);
        var review = new GameReview
        {
            GameId = Guid.NewGuid().ToString(),
            ReviewId = Guid.NewGuid().ToString()
        };

        await Record.ExceptionAsync(() => service.AddReviewAsync(review));

        dynamoMock.Verify(d => d.DescribeTableAsync("GameReviewsExisting", It.IsAny<CancellationToken>()), Times.Once);
        dynamoMock.Verify(d => d.CreateTableAsync(It.IsAny<CreateTableRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetReviewsByGameIdAsync_ShouldUseDefaultTableName_WhenConfigurationIsMissing()
    {
        ResetStaticTableFlag();

        var dynamoMock = new Mock<IAmazonDynamoDB>();
        var config = BuildConfiguration(tableName: null);

        dynamoMock
            .Setup(d => d.DescribeTableAsync("GameReviews", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeTableResponse());

        var service = new DynamoDbReviewService(dynamoMock.Object, config);

        await Record.ExceptionAsync(() => service.GetReviewsByGameIdAsync(Guid.NewGuid()));

        dynamoMock.Verify(d => d.DescribeTableAsync("GameReviews", It.IsAny<CancellationToken>()), Times.Once);
    }
}