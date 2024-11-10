namespace Shared.Models
{
    public class Investment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int PeriodInMonths { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public decimal ExpectedReturn { get; set; }
    }
}
