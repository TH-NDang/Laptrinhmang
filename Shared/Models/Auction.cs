namespace Shared.Models
{
    public class Auction
    {
        public int Id { get; set; }
        public string ProductType { get; set; } // Thêm trường mới để phân loại sản phẩm
        public string LicensePlateNumber { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? WinnerId { get; set; }
        public string Status { get; set; }// "Active", "Completed", "Cancelled"

        // Thêm các trường mới
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductImage { get; set; }
        public string Category { get; set; }
        public decimal MinimumIncrement { get; set; }
    }
}
