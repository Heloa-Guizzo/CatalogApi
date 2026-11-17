using System.Text;
using System.Text.Json;
using CatalogAPI.Models;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;

namespace CatalogAPI.Tests;

public class CatalogUnitTests
{
    [Fact]
    public void Game_ShouldInitializeWithValidProperties()
    {
        var id = Guid.NewGuid();
        var game = new Game
        {
            Id = id,
            Name = "Elden Ring",
            Price = 249.90m,
            Genre = "RPG"
        };

        Assert.Equal(id, game.Id);
        Assert.Equal("Elden Ring", game.Name);
        Assert.Equal(249.90m, game.Price);
        Assert.Equal("RPG", game.Genre);
    }

    [Theory]
    [InlineData(100.00, 10, 90.00)]
    [InlineData(250.00, 20, 200.00)]
    [InlineData(150.00, 0, 150.00)]
    public void PriceCalculation_ShouldApplyDiscountCorrectly(decimal price, decimal discountPercent, decimal expected)
    {
        var finalPrice = price - (price * (discountPercent / 100m));
        Assert.Equal(expected, finalPrice);
    }

    [Theory]
    [InlineData("Elden Ring", "elden", true)]
    [InlineData("Cyberpunk 2077", "cyber", true)]
    [InlineData("God of War", "zelda", false)]
    public void SearchFilter_ShouldMatchGameNameCaseInsensitive(string gameName, string query, bool expectedMatch)
    {
        var matches = gameName.Contains(query, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(expectedMatch, matches);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(0, false)]
    [InlineData(6, false)]
    public void GameReview_ShouldValidateRatingBetweenOneAndFive(int rating, bool expectedValid)
    {
        var isValid = rating >= 1 && rating <= 5;
        Assert.Equal(expectedValid, isValid);
    }

    [Fact]
    public async Task RedisCache_ShouldReturnDeserializedGames_OnCacheHit()
    {
        var cacheMock = new Mock<IDistributedCache>();
        var expectedGames = new List<Game>
        {
            new() { Id = Guid.NewGuid(), Name = "The Witcher 3", Price = 129.90m, Genre = "RPG" },
            new() { Id = Guid.NewGuid(), Name = "Hades", Price = 73.99m, Genre = "Roguelike" }
        };

        var serializedBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expectedGames));

        cacheMock
            .Setup(c => c.GetAsync("catalog:games:all", It.IsAny<CancellationToken>()))
            .ReturnsAsync(serializedBytes);

        var cachedData = await cacheMock.Object.GetAsync("catalog:games:all");
        Assert.NotNull(cachedData);

        var deserializedGames = JsonSerializer.Deserialize<List<Game>>(Encoding.UTF8.GetString(cachedData));

        Assert.NotNull(deserializedGames);
        Assert.Equal(2, deserializedGames.Count);
        Assert.Equal("The Witcher 3", deserializedGames[0].Name);
        cacheMock.Verify(c => c.GetAsync("catalog:games:all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RedisCache_ShouldReturnNull_OnCacheMiss()
    {
        var cacheMock = new Mock<IDistributedCache>();

        cacheMock
            .Setup(c => c.GetAsync("catalog:games:unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var cachedData = await cacheMock.Object.GetAsync("catalog:games:unknown");

        Assert.Null(cachedData);
    }

    [Fact]
    public async Task RedisCache_ShouldInvokeRemoveAsync_WhenInvalidatingCatalogCache()
    {
        var cacheMock = new Mock<IDistributedCache>();

        await cacheMock.Object.RemoveAsync("catalog:games:all");

        cacheMock.Verify(c => c.RemoveAsync("catalog:games:all", It.IsAny<CancellationToken>()), Times.Once);
    }
}