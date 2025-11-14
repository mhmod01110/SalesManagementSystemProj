using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class InstallmentRepository
    {
        public static int CreateInstallmentPlan(int invoiceId, decimal totalAmount,
                   int numberOfInstallments, DateTime startDate)
        {
            decimal installmentAmount = totalAmount / numberOfInstallments;

            string query = @"
                INSERT INTO InstallmentPlans (InvoiceID, TotalAmount, RemainingAmount, 
                    NumberOfInstallments, InstallmentAmount, StartDate, CreatedDate)
                VALUES (@InvoiceID, @TotalAmount, @TotalAmount, @NumberOfInstallments, 
                    @InstallmentAmount, @StartDate, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            SqlParameter[] parameters = {
                new SqlParameter("@InvoiceID", invoiceId),
                new SqlParameter("@TotalAmount", totalAmount),
                new SqlParameter("@NumberOfInstallments", numberOfInstallments),
                new SqlParameter("@InstallmentAmount", installmentAmount),
                new SqlParameter("@StartDate", startDate)
            };

            int planId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));

            // Create individual installments
            for (int i = 1; i <= numberOfInstallments; i++)
            {
                DateTime dueDate = startDate.AddMonths(i);
                CreateInstallment(planId, i, dueDate, installmentAmount);
            }

            return planId;
        }

        private static bool CreateInstallment(int planId, int installmentNumber,
            DateTime dueDate, decimal amount)
        {
            string query = @"
                INSERT INTO Installments (InstallmentPlanID, InstallmentNumber, DueDate, Amount, Status)
                VALUES (@PlanID, @InstallmentNumber, @DueDate, @Amount, 'Pending')";

            SqlParameter[] parameters = {
                new SqlParameter("@PlanID", planId),
                new SqlParameter("@InstallmentNumber", installmentNumber),
                new SqlParameter("@DueDate", dueDate),
                new SqlParameter("@Amount", amount)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool RecordInstallmentPayment(int installmentId, decimal amount)
        {
            string query = @"
                UPDATE Installments 
                SET PaidAmount = PaidAmount + @Amount, 
                    PaidDate = CASE WHEN PaidAmount + @Amount >= Amount THEN GETDATE() ELSE PaidDate END,
                    Status = CASE WHEN PaidAmount + @Amount >= Amount THEN 'Paid' ELSE 'Pending' END
                WHERE InstallmentID = @InstallmentID";

            SqlParameter[] parameters = {
                new SqlParameter("@InstallmentID", installmentId),
                new SqlParameter("@Amount", amount)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public static DataTable GetDueInstallments(DateTime? upToDate = null)
        {
            DateTime checkDate = upToDate ?? DateTime.Today;

            string query = @"
                SELECT i.*, ip.InvoiceID, si.InvoiceNumber, c.CustomerName, c.Phone
                FROM Installments i
                INNER JOIN InstallmentPlans ip ON i.InstallmentPlanID = ip.InstallmentPlanID
                INNER JOIN SalesInvoices si ON ip.InvoiceID = si.InvoiceID
                INNER JOIN Customers c ON si.CustomerID = c.CustomerID
                WHERE i.Status = 'Pending' AND i.DueDate <= @CheckDate
                ORDER BY i.DueDate";

            return DatabaseHelper.ExecuteQuery(query,
                new[] { new SqlParameter("@CheckDate", checkDate) });
        }

        public static DataTable GetCustomerInstallments(int customerId)
        {
            string query = @"
                SELECT i.*, ip.TotalAmount, ip.PaidAmount AS PlanPaidAmount, 
                       si.InvoiceNumber, si.InvoiceDate
                FROM Installments i
                INNER JOIN InstallmentPlans ip ON i.InstallmentPlanID = ip.InstallmentPlanID
                INNER JOIN SalesInvoices si ON ip.InvoiceID = si.InvoiceID
                WHERE si.CustomerID = @CustomerID
                ORDER BY i.DueDate DESC";

            return DatabaseHelper.ExecuteQuery(query,
                new[] { new SqlParameter("@CustomerID", customerId) });
        }
    }
}
