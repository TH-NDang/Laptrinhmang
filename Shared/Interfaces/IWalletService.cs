using Shared.Models;

namespace Shared.Interfaces
{
    public interface IWalletService
    {
        Task<decimal> GetBalance(int userId);
        Task<bool> Deposit(int userId, decimal amount);
        Task<bool> Withdraw(int userId, decimal amount);
        Task<List<WalletTransaction>> GetTransactionHistory(int userId);
    }
}
