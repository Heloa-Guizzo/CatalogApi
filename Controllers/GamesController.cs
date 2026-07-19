using CatalogAPI.DTOs;
using CatalogAPI.Repositories;
using CatalogAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("games")]
public class GamesController : ControllerBase
{
    private readonly GameService _service;

    public GamesController(GameService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGameRequest request)
    {
        var game = await _service.CreateAsync(request);

        return Ok(game);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var game = await _service.GetByIdAsync(id);

        return Ok(game);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateGameRequest request)
    {
        return Ok(await _service.UpdateAsync(id, request));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);

        return NoContent();
    }

    [HttpGet("library")]
    public IActionResult GetLibrary()
    {
        return Ok(UserLibraryStorage.Games);
    }

    [HttpGet("library/{userId}")]
    public async Task<IActionResult> GetLibraryByUser(Guid userId)
    {
        var gameIds = UserLibraryStorage.Games
            .Where(x => x.UserId == userId)
            .Select(x => x.GameId)
            .ToList();

        var games = await _service.GetAllAsync();

        return Ok(
            games.Where(g => gameIds.Contains(g.Id))
        );
    }
}