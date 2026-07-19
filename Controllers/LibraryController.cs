using CatalogAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CatalogAPI.Controllers;

[ApiController]
[Route("library")]
public class LibraryController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(UserLibraryStorage.Games);
    }

    [HttpGet("{userId}")]
    public IActionResult GetByUser(Guid userId)
    {
        return Ok(
            UserLibraryStorage.Games
                .Where(x => x.UserId == userId)
                .ToList());
    }
}
