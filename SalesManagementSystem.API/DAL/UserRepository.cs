using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace SalesManagementSystem.DAL
{
    public class UserRepository
    {
        public static DataTable AuthenticateUser(string username, string password)
        {
            string passwordHash = HashPassword(password);
            string query = @"
                SELECT u.*, r.RoleName
                FROM Users u
                INNER JOIN Roles r ON u.RoleID = r.RoleID
                WHERE u.Username = @Username AND u.PasswordHash = @PasswordHash AND u.IsActive = 1";

            SqlParameter[] parameters = {
                new SqlParameter("@Username", username),
                new SqlParameter("@PasswordHash", passwordHash)
            };

            return DatabaseHelper.ExecuteQuery(query, parameters);
        }

        public static bool CreateUser(string username, string password, string fullName, string email, int roleId)
        {
            string query = @"
                INSERT INTO Users (Username, PasswordHash, FullName, Email, RoleID, CreatedDate)
                VALUES (@Username, @PasswordHash, @FullName, @Email, @RoleID, GETDATE())";

            SqlParameter[] parameters = {
                new SqlParameter("@Username", username),
                new SqlParameter("@PasswordHash", HashPassword(password)),
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@Email", email ?? (object)DBNull.Value),
                new SqlParameter("@RoleID", roleId)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public static void UpdateLastLogin(int userId)
        {
            string query = "UPDATE Users SET LastLogin = GETDATE() WHERE UserID = @UserID";
            DatabaseHelper.ExecuteNonQuery(query, new[] { new SqlParameter("@UserID", userId) });
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public static DataTable GetAllUsers()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT u.*, r.RoleName
                FROM Users u
                INNER JOIN Roles r ON u.RoleID = r.RoleID
                ORDER BY u.Username");
        }

        public static bool CheckPermission(int roleId, string module, string action)
        {
            string query = @"
                SELECT COUNT(*)
                FROM RolePermissions rp
                INNER JOIN Permissions p ON rp.PermissionID = p.PermissionID
                WHERE rp.RoleID = @RoleID AND p.Module = @Module";

            SqlParameter[] parameters = {
                new SqlParameter("@RoleID", roleId),
                new SqlParameter("@Module", module)
            };

            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
            return count > 0;
        }
    }
}
