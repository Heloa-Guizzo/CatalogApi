using Amazon.DynamoDBv2.DataModel;

namespace CatalogAPI.Models;

[DynamoDBTable("GameReviews")]
public class GameReview
{
    [DynamoDBHashKey] 
    public string GameId { get; set; } = string.Empty;
    [DynamoDBRangeKey] 
    public string ReviewId { get; set; } = Guid.NewGuid().ToString();

    public string Author { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}