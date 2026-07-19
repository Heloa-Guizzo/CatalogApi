using CatalogAPI.DTOs;
using CatalogAPI.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Shared.Events;
using CatalogAPI.Models;
using CatalogAPI.Repositories;


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
    public async Task<IActionResult> Create(PurchaseRequest request)
    {
        Console.WriteLine($"[CatalogAPI] Publishing OrderPlacedEvent | UserId: {request.UserId} | GameId: {request.GameId}");

        await _publish.Publish(
            new OrderPlacedEvent(
                request.UserId,
                request.GameId,
                request.Price));

        Console.WriteLine(
            $"LIBRARY COUNT: {UserLibraryStorage.Games.Count}");

        Console.WriteLine(
            $"[CatalogAPI] Game added to library | UserId: {request.UserId} | GameId: {request.GameId}");

        return Ok("Order sent for payment");
    }
}