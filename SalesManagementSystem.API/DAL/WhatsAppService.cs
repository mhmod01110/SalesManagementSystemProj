using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class WhatsAppService
    {
        private static string apiKey = GetSetting("WhatsAppAPIKey");
        private static string apiUrl = "https://api.whatsapp.com/send"; // Use actual API endpoint

        public static bool SendInvoice(int invoiceId)
        {
            // Get invoice and customer details
            DataTable invoice = DatabaseHelper.ExecuteQuery(
                "SELECT si.*, c.CustomerName, c.Phone FROM SalesInvoices si " +
                "INNER JOIN Customers c ON si.CustomerID = c.CustomerID " +
                "WHERE si.InvoiceID = @InvoiceID",
                new[] { new SqlParameter("@InvoiceID", invoiceId) });

            if (invoice.Rows.Count == 0) return false;

            DataRow inv = invoice.Rows[0];
            string phone = inv["Phone"].ToString();
            string message = $"Dear {inv["CustomerName"]},\n\n" +
                           $"Invoice #{inv["InvoiceNumber"]} has been generated.\n" +
                           $"Total Amount: {inv["TotalAmount"]}\n" +
                           $"Payment Status: {inv["PaymentStatus"]}\n\n" +
                           $"Thank you for your business!";

            return SendMessage(phone, message, "Invoice", invoiceId);
        }

        public static bool SendPaymentReminder(int installmentId)
        {
            DataTable inst = DatabaseHelper.ExecuteQuery(
                "SELECT i.*, c.CustomerName, c.Phone, si.InvoiceNumber " +
                "FROM Installments i " +
                "INNER JOIN InstallmentPlans ip ON i.InstallmentPlanID = ip.InstallmentPlanID " +
                "INNER JOIN SalesInvoices si ON ip.InvoiceID = si.InvoiceID " +
                "INNER JOIN Customers c ON si.CustomerID = c.CustomerID " +
                "WHERE i.InstallmentID = @InstallmentID",
                new[] { new SqlParameter("@InstallmentID", installmentId) });

            if (inst.Rows.Count == 0) return false;

            DataRow row = inst.Rows[0];
            string phone = row["Phone"].ToString();
            string message = $"Dear {row["CustomerName"]},\n\n" +
                           $"This is a reminder for your installment payment.\n" +
                           $"Invoice: {row["InvoiceNumber"]}\n" +
                           $"Due Date: {Convert.ToDateTime(row["DueDate"]):yyyy-MM-dd}\n" +
                           $"Amount: {row["Amount"]}\n\n" +
                           $"Please make your payment on time.";

            return SendMessage(phone, message, "Reminder", installmentId);
        }

        private static bool SendMessage(string phone, string message, string messageType, int referenceId)
        {
            try
            {
                // Log the message
                string query = @"
                    INSERT INTO WhatsAppMessages (RecipientPhone, MessageType, MessageContent,
                        Status, ReferenceType, ReferenceID, CreatedDate)
                    VALUES (@Phone, @Type, @Message, 'Pending', @RefType, @RefID, GETDATE())";

                SqlParameter[] parameters = {
                    new SqlParameter("@Phone", phone),
                    new SqlParameter("@Type", messageType),
                    new SqlParameter("@Message", message),
                    new SqlParameter("@RefType", messageType),
                    new SqlParameter("@RefID", referenceId)
                };

                DatabaseHelper.ExecuteNonQuery(query, parameters);

                // TODO: Implement actual WhatsApp API call
                // This would use HTTP client to call WhatsApp Business API
                // For now, just mark as sent

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string GetSetting(string key)
        {
            object result = DatabaseHelper.ExecuteScalar(
                "SELECT SettingValue FROM SystemSettings WHERE SettingKey = @Key",
                new[] { new SqlParameter("@Key", key) });

            return result?.ToString() ?? "";
        }
    }
}
