using CatalogAPI.DTOs;
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
    public IActionResult Create(CreateGameRequest request)
    {
        var game = _service.Create(request);
        return Ok(game);
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_service.GetAll());
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var success = _service.Delete(id);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpPost("purchase")]
    public IActionResult Purchase(PurchaseRequest request)
    {
        var game = _service.GetById(request.GameId);

        if (game == null)
            return NotFound("Game not found");

        return Ok(new
        {
            message = "Compra iniciada com sucesso",
            game = game.Name
        });
    }

}

