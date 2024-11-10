// Server/Services/WalletService.cs
using Server.Data;
using MySql.Data.MySqlClient;
using Shared.Models;
using Shared.Interfaces;
using System.Data;

namespace Server.Services
{
    public class WalletService : IWalletService
    {
        private readonly DatabaseContext _dbContext;

        public WalletService(DatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<decimal> GetBalance(int userId)
        {
            using var conn = _dbContext.GetConnection();
            await conn.OpenAsync();

            var sql = "SELECT wallet_balance FROM users WHERE id = @userId";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToDecimal(result) : 0m;
        }

        public async Task<bool> Deposit(int userId, decimal amount)
        {
            using var conn = _dbContext.GetConnection();
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Cập nhật số dư ví
                var updateBalanceSql = @"
                    UPDATE users 
                    SET wallet_balance = wallet_balance + @amount 
                    WHERE id = @userId";

                using (var cmd = new MySqlCommand(updateBalanceSql, conn))
                {
                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Tạo bản ghi giao dịch
                var insertTransactionSql = @"
                    INSERT INTO wallet_transactions 
                    (user_id, amount, type, status, created_at, description)
                    VALUES 
                    (@userId, @amount, 'Deposit', 'Completed', NOW(), 'Nạp tiền vào ví')";

                using (var cmd = new MySqlCommand(insertTransactionSql, conn))
                {
                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> Withdraw(int userId, decimal amount)
        {
            using var conn = _dbContext.GetConnection();
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Kiểm tra số dư
                var checkBalanceSql = "SELECT wallet_balance FROM users WHERE id = @userId";
                decimal currentBalance;

                using (var cmd = new MySqlCommand(checkBalanceSql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    var result = await cmd.ExecuteScalarAsync();
                    currentBalance = Convert.ToDecimal(result);
                }

                if (currentBalance < amount)
                    return false;

                // Cập nhật số dư ví
                var updateBalanceSql = @"
                    UPDATE users 
                    SET wallet_balance = wallet_balance - @amount 
                    WHERE id = @userId";

                using (var cmd = new MySqlCommand(updateBalanceSql, conn))
                {
                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Tạo bản ghi giao dịch
                var insertTransactionSql = @"
                    INSERT INTO wallet_transactions 
                    (user_id, amount, type, status, created_at, description)
                    VALUES 
                    (@userId, @amount, 'Withdraw', 'Completed', NOW(), 'Rút tiền từ ví')";

                using (var cmd = new MySqlCommand(insertTransactionSql, conn))
                {
                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<WalletTransaction>> GetTransactionHistory(int userId)
        {
            var transactions = new List<WalletTransaction>();
            using var conn = _dbContext.GetConnection();
            await conn.OpenAsync();

            var sql = @"
                SELECT * FROM wallet_transactions 
                WHERE user_id = @userId 
                ORDER BY created_at DESC 
                LIMIT 20";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                transactions.Add(new WalletTransaction
                {
                    Id = reader.GetInt32("id"),
                    UserId = reader.GetInt32("user_id"),
                    Amount = reader.GetDecimal("amount"),
                    Type = reader.GetString("type"),
                    Status = reader.GetString("status"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    Description = reader.GetString("description")
                });
            }

            return transactions;
        }

        public async Task<bool> ProcessAuctionPayment(int userId, int auctionId, decimal amount)
        {
            using var conn = _dbContext.GetConnection();
            await conn.OpenAsync();
            using var transaction = await conn.BeginTransactionAsync();

            try
            {
                // Kiểm tra số dư
                var checkBalanceSql = "SELECT wallet_balance FROM users WHERE id = @userId";
                decimal currentBalance;

                using (var cmd = new MySqlCommand(checkBalanceSql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    var result = await cmd.ExecuteScalarAsync();
                    currentBalance = Convert.ToDecimal(result);
                }

                if (currentBalance < amount)
                    return false;

                // Cập nhật số dư ví
                var updateBalanceSql = @"
                    UPDATE users 
                    SET wallet_balance = wallet_balance - @amount 
                    WHERE id = @userId";

                using (var cmd = new MySqlCommand(updateBalanceSql, conn))
                {
                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    await cmd.ExecuteNonQueryAsync();
                }

                // Tạo bản ghi giao dịch
                var insertTransactionSql = @"
                    INSERT INTO wallet_transactions 
                    (user_id, amount, type, status, created_at, description)
                    VALUES 
                    (@userId, @amount, 'AuctionPayment', 'Completed', NOW(), @description)";

                using (var cmd = new MySqlCommand(insertTransactionSql, conn))
                {
                    cmd.Transaction = transaction;
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    cmd.Parameters.AddWithValue("@description", $"Thanh toán đấu giá #{auctionId}");
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
