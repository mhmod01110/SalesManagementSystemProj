using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class AccountingRepository
    {
        public static DataTable GetChartOfAccounts()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT a.*, pa.AccountName AS ParentAccountName
                FROM ChartOfAccounts a
                LEFT JOIN ChartOfAccounts pa ON a.ParentAccountID = pa.AccountID
                WHERE a.IsActive = 1
                ORDER BY a.AccountCode");
        }

        public static int CreateJournalEntry(string description, string referenceType,
            int? referenceId, int userId)
        {
            string entryNumber = GenerateJournalEntryNumber();

            string query = @"
                INSERT INTO JournalEntries (EntryNumber, EntryDate, Description, ReferenceType,
                    ReferenceID, CreatedBy, CreatedDate)
                VALUES (@EntryNumber, GETDATE(), @Description, @ReferenceType, @ReferenceID,
                    @CreatedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            SqlParameter[] parameters = {
                new SqlParameter("@EntryNumber", entryNumber),
                new SqlParameter("@Description", description ?? (object)DBNull.Value),
                new SqlParameter("@ReferenceType", referenceType ?? (object)DBNull.Value),
                new SqlParameter("@ReferenceID", referenceId ?? (object)DBNull.Value),
                new SqlParameter("@CreatedBy", userId)
            };

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
        }

        public static bool AddJournalEntryDetail(int journalEntryId, int accountId,
            decimal debitAmount, decimal creditAmount, string description)
        {
            string query = @"
                INSERT INTO JournalEntryDetails (JournalEntryID, AccountID, DebitAmount,
                    CreditAmount, Description)
                VALUES (@JournalEntryID, @AccountID, @DebitAmount, @CreditAmount, @Description)";

            SqlParameter[] parameters = {
                new SqlParameter("@JournalEntryID", journalEntryId),
                new SqlParameter("@AccountID", accountId),
                new SqlParameter("@DebitAmount", debitAmount),
                new SqlParameter("@CreditAmount", creditAmount),
                new SqlParameter("@Description", description ?? (object)DBNull.Value)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public static DataTable GetTrialBalance(DateTime asOfDate)
        {
            string query = @"
                SELECT
                    a.AccountCode,
                    a.AccountName,
                    a.AccountType,
                    SUM(jed.DebitAmount) AS TotalDebit,
                    SUM(jed.CreditAmount) AS TotalCredit,
                    SUM(jed.DebitAmount) - SUM(jed.CreditAmount) AS Balance
                FROM ChartOfAccounts a
                LEFT JOIN JournalEntryDetails jed ON a.AccountID = jed.AccountID
                LEFT JOIN JournalEntries je ON jed.JournalEntryID = je.JournalEntryID
                WHERE (je.EntryDate <= @AsOfDate OR je.EntryDate IS NULL)
                GROUP BY a.AccountCode, a.AccountName, a.AccountType
                HAVING SUM(jed.DebitAmount) - SUM(jed.CreditAmount) != 0
                ORDER BY a.AccountCode";

            return DatabaseHelper.ExecuteQuery(query,
                new[] { new SqlParameter("@AsOfDate", asOfDate) });
        }

        public static DataTable GetIncomeStatement(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT
                    a.AccountType,
                    a.AccountName,
                    SUM(jed.CreditAmount) - SUM(jed.DebitAmount) AS Amount
                FROM ChartOfAccounts a
                INNER JOIN JournalEntryDetails jed ON a.AccountID = jed.AccountID
                INNER JOIN JournalEntries je ON jed.JournalEntryID = je.JournalEntryID
                WHERE a.AccountType IN ('Revenue', 'Expense')
                  AND je.EntryDate BETWEEN @StartDate AND @EndDate
                GROUP BY a.AccountType, a.AccountName
                ORDER BY a.AccountType, a.AccountName";

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseHelper.ExecuteQuery(query, parameters);
        }

        private static string GenerateJournalEntryNumber()
        {
            string prefix = "JE";
            string query = "SELECT TOP 1 EntryNumber FROM JournalEntries ORDER BY JournalEntryID DESC";
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
