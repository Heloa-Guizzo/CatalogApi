namespace CatalogAPI.Models
{
    public class Game
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Genre { get; set; } = string.Empty;
    }
}
