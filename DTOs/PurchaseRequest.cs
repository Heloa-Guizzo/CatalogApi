namespace CatalogAPI.DTOs
{
    public class PurchaseRequest
    {
        public Guid GameId { get; set; }
        public Guid UserId { get; set; }
    }
}
