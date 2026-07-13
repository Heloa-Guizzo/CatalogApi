using CatalogAPI.DTOs;
using CatalogAPI.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Shared.Events;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("purchase")]
public class PurchaseController : ControllerBase
{
    private readonly GameService _service;
    private readonly IPublishEndpoint _publish;

    public PurchaseController(
        GameService service,
        IPublishEndpoint publish)
    {
        _service = service;
        _publish = publish;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        PurchaseRequest request)
    {
        Console.WriteLine("PASSO 1");

        var game =
            await _service.GetByIdAsync(
                request.GameId);

        Console.WriteLine("PASSO 2");

        Console.WriteLine(
            $"PUBLICANDO OrderPlacedEvent - GameId: {request.GameId}");

        await _publish.Publish(
            new OrderPlacedEvent(
                request.UserId,
                request.GameId,
                game.Price));

        Console.WriteLine("PASSO 3");

        return Ok("Order sent for payment");
    }
}