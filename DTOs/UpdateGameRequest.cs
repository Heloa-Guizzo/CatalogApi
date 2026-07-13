namespace CatalogAPI.DTOs
{
    public class UpdateGameRequest
    {
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Genre { get; set; } = string.Empty;
    }
}
