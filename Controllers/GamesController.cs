using System.Text.Json;
using CatalogAPI.DTOs;
using CatalogAPI.Models;
using CatalogAPI.Repositories;
using CatalogAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("games")]
public class GamesController : ControllerBase
{
    private readonly GameService _service;
    private readonly IDistributedCache _cache;
    private readonly ElasticsearchService _elasticService;
    private readonly DynamoDbReviewService _reviewService;
    private const string AllGamesCacheKey = "all_games";

    public GamesController(
        GameService service,
        IDistributedCache cache,
        ElasticsearchService elasticService,
        DynamoDbReviewService reviewService)
    {
        _service = service;
        _cache = cache;
        _elasticService = elasticService;
        _reviewService = reviewService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGameRequest request)
    {
        var game = await _service.CreateAsync(request);

        await _elasticService.IndexGameAsync(game);
        await _cache.RemoveAsync(AllGamesCacheKey);

        return Ok(game);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var cachedData = await _cache.GetStringAsync(AllGamesCacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            Response.Headers.Append("X-Cache", "HIT");
            return Ok(JsonSerializer.Deserialize<IEnumerable<Game>>(cachedData));
        }

        Response.Headers.Append("X-Cache", "MISS");
        var games = await _service.GetAllAsync();

        await _cache.SetStringAsync(
            AllGamesCacheKey,
            JsonSerializer.Serialize(games),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var cacheKey = $"game_{id}";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            Response.Headers.Append("X-Cache", "HIT");
            return Ok(JsonSerializer.Deserialize<Game>(cachedData));
        }

        Response.Headers.Append("X-Cache", "MISS");
        var game = await _service.GetByIdAsync(id);

        if (game is not null)
        {
            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(game),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
        }

        return Ok(game);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateGameRequest request)
    {
        var updatedGame = await _service.UpdateAsync(id, request);

        if (updatedGame is not null)
            await _elasticService.IndexGameAsync(updatedGame);

        await _cache.RemoveAsync(AllGamesCacheKey);
        await _cache.RemoveAsync($"game_{id}");

        return Ok(updatedGame);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);

        await _elasticService.DeleteGameAsync(id);
        await _cache.RemoveAsync(AllGamesCacheKey);
        await _cache.RemoveAsync($"game_{id}");

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { error = "Query parameter 'q' is required." });

        var results = await _elasticService.SearchGamesAsync(q);
        return Ok(results);
    }

    [HttpPost("{id}/reviews")]
    public async Task<IActionResult> AddReview(Guid id, [FromBody] CreateReviewRequest request)
    {
        var review = new GameReview
        {
            GameId = id.ToString(),
            Author = request.Author,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _reviewService.AddReviewAsync(review);
        return Ok(review);
    }

    [HttpGet("{id}/reviews")]
    public async Task<IActionResult> GetReviews(Guid id)
    {
        var reviews = await _reviewService.GetReviewsByGameIdAsync(id);
        return Ok(reviews);
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

public record CreateReviewRequest(string Author, int Rating, string Comment);