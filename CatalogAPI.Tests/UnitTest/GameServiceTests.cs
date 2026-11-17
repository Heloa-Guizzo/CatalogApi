using CatalogAPI.DTOs;
using CatalogAPI.Exceptions;
using CatalogAPI.Models;
using CatalogAPI.Repositories;
using CatalogAPI.Services;
using Moq;
using Xunit;

namespace CatalogAPI.Tests;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _repositoryMock;
    private readonly GameService _service;

    public GameServiceTests()
    {
        _repositoryMock = new Mock<IGameRepository>();
        _service = new GameService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnGame_WithGeneratedId()
    {
        var request = new CreateGameRequest
        {
            Name = "Baldur's Gate 3",
            Price = 199.99m,
            Genre = "RPG"
        };

        var result = await _service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Baldur's Gate 3", result.Name);
        Assert.Equal(199.99m, result.Price);
        Assert.Equal("RPG", result.Genre);
        _repositoryMock.Verify(r => r.AddAsync(It.Is<Game>(g => g.Id == result.Id && g.Name == "Baldur's Gate 3")), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllGamesFromRepository()
    {
        var games = new List<Game>
        {
            new() { Id = Guid.NewGuid(), Name = "Game 1", Price = 50m, Genre = "Action" },
            new() { Id = Guid.NewGuid(), Name = "Game 2", Price = 100m, Genre = "Adventure" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(games);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count());
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGame_WhenGameExists()
    {
        var id = Guid.NewGuid();
        var game = new Game { Id = id, Name = "Red Dead Redemption 2", Price = 249.90m, Genre = "Action" };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(game);

        var result = await _service.GetByIdAsync(id);

        Assert.Equal(id, result.Id);
        Assert.Equal("Red Dead Redemption 2", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Game?)null);

        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(id));

        Assert.Equal("Game not found.", exception.Message);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFieldsAndReturnGame_WhenGameExists()
    {
        var id = Guid.NewGuid();
        var existingGame = new Game { Id = id, Name = "Old Name", Price = 100m, Genre = "Indie" };
        var updateRequest = new UpdateGameRequest
        {
            Name = "New Name",
            Price = 149.90m,
            Genre = "RPG"
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingGame);

        var result = await _service.UpdateAsync(id, updateRequest);

        Assert.Equal("New Name", result.Name);
        Assert.Equal(149.90m, result.Price);
        Assert.Equal("RPG", result.Genre);
        _repositoryMock.Verify(r => r.UpdateAsync(existingGame), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        var id = Guid.NewGuid();
        var updateRequest = new UpdateGameRequest { Name = "New Name", Price = 149.90m, Genre = "RPG" };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Game?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync(id, updateRequest));
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Game>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCallDeleteOnRepository_WhenGameExists()
    {
        var id = Guid.NewGuid();
        var existingGame = new Game { Id = id, Name = "Game to Delete", Price = 59.90m, Genre = "Puzzle" };

        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingGame);

        await _service.DeleteAsync(id);

        _repositoryMock.Verify(r => r.DeleteAsync(existingGame), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenGameDoesNotExist()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Game?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(id));
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Game>()), Times.Never);
    }
}