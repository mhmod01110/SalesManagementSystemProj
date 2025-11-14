using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class CustomerRepository
    {
        public static DataTable GetAllCustomers()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT c.*, sr.RepName AS SalesRepName
                FROM Customers c
                LEFT JOIN SalesRepresentatives sr ON c.SalesRepID = sr.SalesRepID
                WHERE c.IsActive = 1
                ORDER BY c.CustomerName");
        }

        public static int CreateCustomer(string code, string name, string type, string phone,
            string email, string address, decimal creditLimit, decimal discount, int? salesRepId)
        {
            string query = @"
                INSERT INTO Customers (CustomerCode, CustomerName, CustomerType, Phone, Email, 
                    Address, CreditLimit, DiscountPercentage, SalesRepID, CreatedDate)
                VALUES (@Code, @Name, @Type, @Phone, @Email, @Address, @CreditLimit, 
                    @Discount, @SalesRepID, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            SqlParameter[] parameters = {
                new SqlParameter("@Code", code),
                new SqlParameter("@Name", name),
                new SqlParameter("@Type", type),
                new SqlParameter("@Phone", phone ?? (object)DBNull.Value),
                new SqlParameter("@Email", email ?? (object)DBNull.Value),
                new SqlParameter("@Address", address ?? (object)DBNull.Value),
                new SqlParameter("@CreditLimit", creditLimit),
                new SqlParameter("@Discount", discount),
                new SqlParameter("@SalesRepID", salesRepId ?? (object)DBNull.Value)
            };

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
        }

        public static bool UpdateCustomerBalance(int customerId, decimal amount)
        {
            string query = "UPDATE Customers SET Balance = Balance + @Amount WHERE CustomerID = @CustomerID";

            SqlParameter[] parameters = {
                new SqlParameter("@CustomerID", customerId),
                new SqlParameter("@Amount", amount)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }
    }
}
