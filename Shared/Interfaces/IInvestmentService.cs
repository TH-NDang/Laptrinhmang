using Shared.Models;

namespace Shared.Interfaces
{
    public interface IInvestmentService
    {
        Task<Investment> CreateInvestment(int userId, decimal amount, int periodInMonths);
        Task<List<Investment>> GetUserInvestments(int userId);
        Task<decimal> CalculateExpectedReturn(decimal amount, int periodInMonths);
        Task<bool> WithdrawInvestment(int investmentId);
    }
}
