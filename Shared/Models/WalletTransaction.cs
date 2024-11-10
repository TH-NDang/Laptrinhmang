// Shared/Models/WalletTransaction.cs
namespace Shared.Models
{
    public class WalletTransaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // Deposit, Withdraw, Investment, AuctionPayment
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Description { get; set; }
    }

    // Các model phụ trợ cho Wallet
    public class DepositRequest
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class WithdrawRequest
    {
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string BankAccount { get; set; }
    }

    public class TransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal? NewBalance { get; set; }
    }

    public class WalletResponse
    {
        public decimal Balance { get; set; }
        public int RewardPoints { get; set; }
        public List<WalletTransaction> RecentTransactions { get; set; }
    }
}
