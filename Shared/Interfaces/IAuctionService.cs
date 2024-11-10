using Shared.Models;

namespace Shared.Interfaces
{
    /// <summary>
    /// Interface cung cấp các phương thức để tương tác với hệ thống đấu giá
    /// </summary>
    public interface IAuctionService
    {
        /// <summary>
        /// Lấy trạng thái của một phiên đấu giá
        /// </summary>
        /// <param name="auctionId">ID của phiên đấu giá</param>
        /// <returns>Chuỗi mô tả trạng thái của phiên đấu giá</returns>
        Task<string> GetStatus(int auctionId);

        /// <summary>
        /// Lấy danh sách tất cả các phiên đấu giá đang hoạt động
        /// </summary>
        /// <returns>Danh sách các phiên đấu giá đang hoạt động</returns>
        Task<List<Auction>> GetActiveAuctions();

        /// <summary>
        /// Lấy danh sách tất cả các phiên đấu giá đã kết thúc
        /// </summary>
        /// <returns>Danh sách các phiên đấu giá đã kết thúc</returns>
        Task<List<Auction>> GetInactiveAuctions();

        /// <summary>
        /// Đặt giá cho một phiên đấu giá
        /// </summary>
        /// <param name="auctionId">ID của phiên đấu giá</param>
        /// <param name="userId">ID của người đặt giá</param>
        /// <param name="amount">Số tiền đặt giá</param>
        /// <returns>True nếu đặt giá thành công, ngược lại là False</returns>
        Task<bool> PlaceBid(int auctionId, int userId, decimal amount);

        /// <summary>
        /// Lấy danh sách các lượt đặt giá của một phiên đấu giá
        /// </summary>
        /// <param name="auctionId">ID của phiên đấu giá</param>
        /// <returns>Danh sách các lượt đặt giá</returns>
        Task<List<Bid>> GetAuctionBids(int auctionId);

        /// <summary>
        /// Đăng ký người dùng mới
        /// </summary>
        /// <param name="user">Thông tin người dùng cần đăng ký</param>
        /// <returns>True nếu đăng ký thành công, ngược lại là False</returns>
        Task<bool> RegisterUser(User user);

        /// <summary>
        /// Đăng nhập vào hệ thống
        /// </summary>
        /// <param name="username">Tên đăng nhập</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns>Thông tin người dùng nếu đăng nhập thành công</returns>
        Task<User> Login(string username, string password);
        Task<bool> UpdateAuction(Auction updatedAuction);
        Task<bool> DeleteAuction(int auctionId);
        Task<bool> AddAuction(Auction newAuction);

        Task<List<Auction>> GetAuctionsByCategory(string category);
        Task<List<string>> GetCategories();
    }
}
