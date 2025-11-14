using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class ExpenseRepository
    {
        public static int CreateExpense(int categoryId, decimal amount, int currencyId,
                 string paymentMethod, string description, string receiptNumber, int userId)
        {
            string expenseNumber = GenerateExpenseNumber();

            string query = @"
                INSERT INTO Expenses (ExpenseNumber, ExpenseDate, ExpenseCategoryID, Amount, 
                    CurrencyID, PaymentMethod, Description, ReceiptNumber, CreatedBy, CreatedDate)
                VALUES (@ExpenseNumber, GETDATE(), @CategoryID, @Amount, @CurrencyID, 
                    @PaymentMethod, @Description, @ReceiptNumber, @CreatedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            SqlParameter[] parameters = {
                new SqlParameter("@ExpenseNumber", expenseNumber),
                new SqlParameter("@CategoryID", categoryId),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@CurrencyID", currencyId),
                new SqlParameter("@PaymentMethod", paymentMethod ?? (object)DBNull.Value),
                new SqlParameter("@Description", description ?? (object)DBNull.Value),
                new SqlParameter("@ReceiptNumber", receiptNumber ?? (object)DBNull.Value),
                new SqlParameter("@CreatedBy", userId)
            };

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
        }

        public static DataTable GetExpenses(DateTime? startDate = null, DateTime? endDate = null)
        {
            string query = @"
                SELECT e.*, ec.CategoryName, cur.CurrencyCode, u.FullName AS CreatedByName
                FROM Expenses e
                INNER JOIN ExpenseCategories ec ON e.ExpenseCategoryID = ec.ExpenseCategoryID
                INNER JOIN Currencies cur ON e.CurrencyID = cur.CurrencyID
                INNER JOIN Users u ON e.CreatedBy = u.UserID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (startDate.HasValue)
            {
                query += " AND e.ExpenseDate >= @StartDate";
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                query += " AND e.ExpenseDate <= @EndDate";
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }

            query += " ORDER BY e.ExpenseDate DESC";

            return DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
        }

        public static DataTable GetExpensesSummaryByCategory(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT 
                    ec.CategoryName,
                    COUNT(e.ExpenseID) AS ExpenseCount,
                    SUM(e.Amount) AS TotalAmount,
                    cur.CurrencyCode
                FROM Expenses e
                INNER JOIN ExpenseCategories ec ON e.ExpenseCategoryID = ec.ExpenseCategoryID
                INNER JOIN Currencies cur ON e.CurrencyID = cur.CurrencyID
                WHERE e.ExpenseDate BETWEEN @StartDate AND @EndDate
                GROUP BY ec.CategoryName, cur.CurrencyCode
                ORDER BY TotalAmount DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseHelper.ExecuteQuery(query, parameters);
        }

        private static string GenerateExpenseNumber()
        {
            string prefix = "EXP";
            string query = "SELECT TOP 1 ExpenseNumber FROM Expenses ORDER BY ExpenseID DESC";
            object result = DatabaseHelper.ExecuteScalar(query);

            int nextNumber = 1;
            if (result != null)
            {
                string lastNumber = result.ToString().Replace(prefix, "");
                if (int.TryParse(lastNumber, out int num))
                    nextNumber = num + 1;
            }

            return $"{prefix}{nextNumber:D6}";
        }
    }
}
