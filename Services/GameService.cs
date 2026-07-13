using CatalogAPI.DTOs;
using CatalogAPI.Exceptions;
using CatalogAPI.Models;
using CatalogAPI.Repositories;

namespace CatalogAPI.Services;

public class GameService
{
    private readonly IGameRepository _repository;

    public GameService(IGameRepository repository)
    {
        _repository = repository;
    }

    public async Task<Game> CreateAsync(CreateGameRequest request)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Genre = request.Genre
        };

        await _repository.AddAsync(game);

        return game;
    }

    public async Task<IEnumerable<Game>>
        GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }
    public async Task<Game> GetByIdAsync(Guid id)
    {
        var game = await _repository.GetByIdAsync(id);

        if (game == null) throw new NotFoundException("Game not found.");

        return game;
    }


    public async Task<Game> UpdateAsync(Guid id,UpdateGameRequest request)
    {
        var game =
            await _repository.GetByIdAsync(id);

        if (game == null)
            throw new NotFoundException(
                "Game not found.");

        game.Name = request.Name;
        game.Price = request.Price;
        game.Genre = request.Genre;

        await _repository.UpdateAsync(game);

        return game;
    }

    public async Task DeleteAsync(Guid id)
    {
        var game = await _repository.GetByIdAsync(id);

        if (game == null)
            throw new NotFoundException(
                "Game not found.");

        await _repository.DeleteAsync(game);
    }
}