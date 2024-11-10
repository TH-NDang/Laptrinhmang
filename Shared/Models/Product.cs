namespace Shared.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public decimal BasePrice { get; set; }
        public string Condition { get; set; } // New, Used, etc.
        public Dictionary<string, string> Specifications { get; set; }
        public int SellerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
