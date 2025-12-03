using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class EmailService
    {
        public static bool SendInvoiceEmail(int invoiceId, string toEmail)
        {
            try
            {
                string smtpServer = GetSetting("SMTPServer");
                int smtpPort = int.Parse(GetSetting("SMTPPort"));
                string smtpUsername = GetSetting("SMTPUsername");
                string smtpPassword = GetSetting("SMTPPassword");

                // Get invoice details
                DataTable invoice = DatabaseHelper.ExecuteQuery(
                    "SELECT si.*, c.CustomerName FROM SalesInvoices si " +
                    "INNER JOIN Customers c ON si.CustomerID = c.CustomerID " +
                    "WHERE si.InvoiceID = @InvoiceID",
                    new[] { new SqlParameter("@InvoiceID", invoiceId) });

                if (invoice.Rows.Count == 0) return false;

                DataRow inv = invoice.Rows[0];

                string subject = $"Invoice #{inv["InvoiceNumber"]}";
                string body = $"Dear {inv["CustomerName"]},\n\n" +
                            $"Please find your invoice details below:\n\n" +
                            $"Invoice Number: {inv["InvoiceNumber"]}\n" +
                            $"Date: {Convert.ToDateTime(inv["InvoiceDate"]):yyyy-MM-dd}\n" +
                            $"Total Amount: {inv["TotalAmount"]}\n" +
                            $"Payment Status: {inv["PaymentStatus"]}\n\n" +
                            $"Thank you for your business!";

                // TODO: Implement actual email sending using System.Net.Mail
                // For now, just return success

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
