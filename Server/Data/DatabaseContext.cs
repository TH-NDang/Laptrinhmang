using MySql.Data.MySqlClient;
using System.Data;

namespace Server.Data
{
    public class UserService
    {
        public static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext()
        {
            _connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ??
                              "server=localhost;database=phpmyadmin;user=root";

        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// Initializes the database by creating necessary tables if they don't exist and populating them with sample data.
        /// </summary>
        /// <remarks>
        /// Creates the following tables:
        /// - users: Stores user account information
        /// - user_info: Stores additional user details
        /// - auctions: Stores auction listings
        /// - bids: Stores bidding history
        /// - payment_history: Stores payment transactions
        /// 
        /// If the users table is empty, inserts sample users and auction data.
        /// </remarks>
        /// <returns>A task representing the asynchronous database initialization operation.</returns>
        public async Task InitializeDatabase()
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            using (var cmdUsers = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS users (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    username VARCHAR(50) UNIQUE NOT NULL,
                    password VARCHAR(255) NOT NULL,
                    email VARCHAR(100) UNIQUE NOT NULL,
                    role VARCHAR(20) DEFAULT 'user',
                    wallet_balance DECIMAL(15,2) DEFAULT 0,
                    reward_points INT DEFAULT 0,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    last_login_at TIMESTAMP
                )", conn))
            {
                await cmdUsers.ExecuteNonQueryAsync();
            }

            // T?o b?ng Auctions
            using (var cmdAuctions = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS auctions (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    product_type VARCHAR(50),
                    license_plate_number VARCHAR(20),
                    product_name VARCHAR(100),
                    product_description TEXT,
                    product_image VARCHAR(255),
                    category VARCHAR(50),
                    starting_price DECIMAL(15,2) NOT NULL,
                    current_price DECIMAL(15,2) NOT NULL,
                    minimum_increment DECIMAL(15,2) DEFAULT 1000,
                    start_time TIMESTAMP NOT NULL,
                    end_time TIMESTAMP NOT NULL,
                    winner_id INT,
                    status VARCHAR(20) DEFAULT 'Active',
                    FOREIGN KEY (winner_id) REFERENCES users(id)
                )", conn))
            {
                await cmdAuctions.ExecuteNonQueryAsync();
            }

            using (var cmdBids = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS bids (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    auction_id INT NOT NULL,
                    user_id INT NOT NULL,
                    amount DECIMAL(15,2) NOT NULL,
                    bid_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (auction_id) REFERENCES auctions(id),
                    FOREIGN KEY (user_id) REFERENCES users(id)
                )", conn))
            {
                await cmdBids.ExecuteNonQueryAsync();
            }

            using (var cmdInvestments = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS investments (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    amount DECIMAL(15,2) NOT NULL,
                    interest_rate DECIMAL(5,2) NOT NULL,
                    period_in_months INT NOT NULL,
                    start_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    end_date TIMESTAMP NOT NULL,
                    status VARCHAR(20) DEFAULT 'Active',
                    expected_return DECIMAL(15,2) NOT NULL,
                    FOREIGN KEY (user_id) REFERENCES users(id)
                )", conn))
            {
                await cmdInvestments.ExecuteNonQueryAsync();
            }

            using (var cmdWalletTransactions = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS wallet_transactions (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    amount DECIMAL(15,2) NOT NULL,
                    type VARCHAR(20) NOT NULL,
                    status VARCHAR(20) NOT NULL,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    description TEXT,
                    FOREIGN KEY (user_id) REFERENCES users(id)
                )", conn))
            {
                await cmdWalletTransactions.ExecuteNonQueryAsync();
            }

            var createTables = @"
                CREATE TABLE IF NOT EXISTS user_info (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    user_id INT NOT NULL,
                    full_name VARCHAR(100) NOT NULL,
                    date_of_birth DATE,
                    address VARCHAR(200),
                    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS payment_history (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    history_username VARCHAR(50) NOT NULL,
                    history_license_plate VARCHAR(20) NOT NULL,
                    history_amount DECIMAL(10,2) NOT NULL,
                    payment_method VARCHAR(20) NOT NULL,
                    payment_time DATETIME NOT NULL,
                    status ENUM('Thành công', 'Th?t b?i') NOT NULL,
                    user_id INT,
                    auction_id INT,
                    FOREIGN KEY (user_id) REFERENCES users(id),
                    FOREIGN KEY (auction_id) REFERENCES auctions(id)
                );";

            using var cmdCreateTables = new MySqlCommand(createTables, conn);
            await cmdCreateTables.ExecuteNonQueryAsync();

            // Insert sample data if tables are empty
            if (!await TableHasData(conn, "users"))
            {
                var insertSampleData = @"
                    INSERT INTO users (username, password, email) VALUES
                    ('user1', 'password123', 'user1@example.com'),
                    ('user2', 'password123', 'user2@example.com');
                    INSERT INTO auctions (license_plate_number, starting_price, current_price, 
                                        start_time, end_time, status) VALUES
                    ('51F-123.45', 1000000, 1000000, NOW(), DATE_ADD(NOW(), INTERVAL 1 DAY), 'Active'),
                    ('51F-678.90', 2000000, 2000000, NOW(), DATE_ADD(NOW(), INTERVAL 1 DAY), 'Active');";

                using var cmdInsert = new MySqlCommand(insertSampleData, conn);
                await cmdInsert.ExecuteNonQueryAsync();
            }
        }

        private async Task<bool> TableHasData(MySqlConnection conn, string tableName)
        {
            using var cmd = new MySqlCommand($"SELECT COUNT(*) FROM {tableName}", conn);
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }
        // Method to retrieve a username by user ID
        public async Task<string> GetUsernameById(int id)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT username FROM users WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            var username = await cmd.ExecuteScalarAsync();
            return username?.ToString();
        }
        public async Task<int> GetAuctionIdByUserId(int id)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            using var cmd = new MySqlCommand("SELECT id FROM auctions WHERE winner_id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : -1;  // Tr? v? -1 n?u không tìm th?y ??u giá
        }

        public async Task<(string licensePlateNumber, decimal totalAmount)> GetAuctionDetailsAndTotalAmount(int id)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string licensePlateNumber = null;
            decimal totalAmount = 0;

            // Lay thong tin bien so cua cuoc duu gia ma nguoi dung da thang
            using (var cmd = new MySqlCommand("SELECT license_plate_number FROM auctions WHERE winner_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    // Ki?m tra xem giá tr? có ph?i là DBNull không
                    if (!reader.IsDBNull(reader.GetOrdinal("license_plate_number")))
                    {
                        licensePlateNumber = reader.GetString("license_plate_number");
                    }
                }
            }

            // Tính t?ng s? ti?n thanh toán t? b?ng bids
            using (var cmd = new MySqlCommand("SELECT SUM(amount) FROM bids WHERE user_id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                var result = await cmd.ExecuteScalarAsync();
                // Ki?m tra xem giá tr? có ph?i là DBNull không
                totalAmount = result != null && !Convert.IsDBNull(result) ? Convert.ToDecimal(result) : 0;
            }

            return (licensePlateNumber, totalAmount);
        }
        public async Task SavePaymentHistory(string username, string licensePlateNumber, decimal amount, string paymentMethod, string status, int userId, int auctionId)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            var query = @"
        INSERT INTO payment_history (history_username, history_license_plate, history_amount, payment_method, payment_time, status, user_id, auction_id)
        VALUES (@username, @licensePlateNumber, @amount, @paymentMethod, NOW(), @status, @userId, @auctionId);
    ";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@licensePlateNumber", licensePlateNumber);
            cmd.Parameters.AddWithValue("@amount", amount);
            cmd.Parameters.AddWithValue("@paymentMethod", paymentMethod);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@auctionId", auctionId);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> GetPaymentHistoryByUserId(int id)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();

            string query = @"
            SELECT id, 
                   history_username, 
                   history_license_plate_number,
                   history_amount, 
                   history_payment_method, 
                   payment_time, 
                   history_status'
            FROM payment_history 
            WHERE user_id = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userId", id);

            using var adapter = new MySqlDataAdapter(cmd);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable;
        }


        public async Task<T> QuerySingleAsync<T>(string sql, object param = null)
        {
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(sql, conn);

            if (param != null)
            {
                foreach (var prop in param.GetType().GetProperties())
                {
                    cmd.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(param) ?? DBNull.Value);
                }
            }

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return (T)Convert.ChangeType(reader[0], typeof(T));
            }
            return default;
        }

        public async Task<List<T>> QueryAsync<T>(string sql, object param = null) where T : new()
        {
            var results = new List<T>();
            using var conn = GetConnection();
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(sql, conn);

            if (param != null)
            {
                foreach (var prop in param.GetType().GetProperties())
                {
                    cmd.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(param) ?? DBNull.Value);
                }
            }

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var item = new T();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var property = typeof(T).GetProperty(ToPascalCase(reader.GetName(i)));
                    if (property != null && reader[i] != DBNull.Value)
                    {
                        property.SetValue(item, Convert.ChangeType(reader[i], property.PropertyType));
                    }
                }
                results.Add(item);
            }
            return results;
        }

        private string ToPascalCase(string str)
        {
            var words = str.Split('_');
            for (var i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }
            return string.Concat(words);
        }

    }
}
