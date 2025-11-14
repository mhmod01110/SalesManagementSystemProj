using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class NotificationRepository
    {
        public static bool CreateNotification(int? userId, string type, string title,
                   string message, string priority = "Normal")
        {
            string query = @"
                INSERT INTO Notifications (UserID, NotificationType, Title, Message, Priority, CreatedDate)
                VALUES (@UserID, @Type, @Title, @Message, @Priority, GETDATE())";

            SqlParameter[] parameters = {
                new SqlParameter("@UserID", userId ?? (object)DBNull.Value),
                new SqlParameter("@Type", type),
                new SqlParameter("@Title", title),
                new SqlParameter("@Message", message),
                new SqlParameter("@Priority", priority)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public static DataTable GetUserNotifications(int userId, bool unreadOnly = false)
        {
            string query = @"
                SELECT * FROM Notifications 
                WHERE (UserID = @UserID OR UserID IS NULL)";

            if (unreadOnly)
                query += " AND IsRead = 0";

            query += " ORDER BY CreatedDate DESC";

            return DatabaseHelper.ExecuteQuery(query,
                new[] { new SqlParameter("@UserID", userId) });
        }

        public static bool MarkAsRead(int notificationId)
        {
            string query = "UPDATE Notifications SET IsRead = 1 WHERE NotificationID = @NotificationID";
            return DatabaseHelper.ExecuteNonQuery(query,
                new[] { new SqlParameter("@NotificationID", notificationId) }) > 0;
        }

        public static void CheckLowStockAndNotify()
        {
            DataTable lowStock = InventoryRepository.GetLowStockProducts();

            foreach (DataRow row in lowStock.Rows)
            {
                string message = $"Product {row["ProductName"]} in {row["WarehouseName"]} " +
                                $"is running low. Current: {row["Quantity"]}, Min: {row["MinStockLevel"]}";

                CreateNotification(null, "LowStock", "Low Stock Alert", message, "High");
            }
        }

        public static void CheckDueInstallmentsAndNotify()
        {
            DataTable dueInstallments = InstallmentRepository.GetDueInstallments(DateTime.Today.AddDays(7));

            foreach (DataRow row in dueInstallments.Rows)
            {
                string message = $"Customer {row["CustomerName"]} has installment due on " +
                                $"{Convert.ToDateTime(row["DueDate"]):yyyy-MM-dd}. Amount: {row["Amount"]}";

                CreateNotification(null, "DueInstallment", "Installment Due", message, "Normal");
            }
        }
    }
}
