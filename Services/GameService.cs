
using CatalogAPI.Models;
using CatalogAPI.DTOs;

namespace CatalogAPI.Services;

public class GameService
{
    private static List<Game> games = new();

    public Game Create(CreateGameRequest request)
    {
        var game = new Game
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Price = request.Price,
            Genre = request.Genre
        };

        games.Add(game);
        return game;
    }

    public List<Game> GetAll()
    {
        return games;
    }

    public Game? GetById(Guid id)
    {
        return games.FirstOrDefault(x => x.Id == id);
    }

    public bool Delete(Guid id)
    {
        var game = games.FirstOrDefault(x => x.Id == id);

        if (game == null)
            return false;

        games.Remove(game);
        return true;
    }
}

