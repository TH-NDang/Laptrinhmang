namespace Shared.Interfaces
{
    public interface IRewardService
    {
        Task<int> GetPoints(int userId);
        Task<bool> AddPoints(int userId, int points);
        Task<bool> UsePoints(int userId, int points);
    }
}
