using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class SalesRepository
    {
        public static int CreateSalesInvoice(int customerId, int warehouseId, int? salesRepId,
                    int currencyId, decimal subTotal, decimal discount, decimal tax, decimal total,
                    decimal paid, string paymentMethod, bool isInstallment, int userId, string notes = null)
        {
            string invoiceNumber = GenerateInvoiceNumber();

            string query = @"
                INSERT INTO SalesInvoices (InvoiceNumber, InvoiceDate, CustomerID, WarehouseID,
                    SalesRepID, CurrencyID, SubTotal, DiscountAmount, TaxAmount, TotalAmount,
                    PaidAmount, RemainingAmount, PaymentStatus, PaymentMethod, IsInstallment,
                    Notes, CreatedBy, CreatedDate)
                VALUES (@InvoiceNumber, GETDATE(), @CustomerID, @WarehouseID, @SalesRepID,
                    @CurrencyID, @SubTotal, @DiscountAmount, @TaxAmount, @TotalAmount,
                    @PaidAmount, @RemainingAmount, @PaymentStatus, @PaymentMethod, @IsInstallment,
                    @Notes, @CreatedBy, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            decimal remaining = total - paid;
            string paymentStatus = remaining == 0 ? "Paid" : (paid > 0 ? "Partial" : "Pending");

            SqlParameter[] parameters = {
                new SqlParameter("@InvoiceNumber", invoiceNumber),
                new SqlParameter("@CustomerID", customerId),
                new SqlParameter("@WarehouseID", warehouseId),
                new SqlParameter("@SalesRepID", salesRepId ?? (object)DBNull.Value),
                new SqlParameter("@CurrencyID", currencyId),
                new SqlParameter("@SubTotal", subTotal),
                new SqlParameter("@DiscountAmount", discount),
                new SqlParameter("@TaxAmount", tax),
                new SqlParameter("@TotalAmount", total),
                new SqlParameter("@PaidAmount", paid),
                new SqlParameter("@RemainingAmount", remaining),
                new SqlParameter("@PaymentStatus", paymentStatus),
                new SqlParameter("@PaymentMethod", paymentMethod ?? (object)DBNull.Value),
                new SqlParameter("@IsInstallment", isInstallment),
                new SqlParameter("@Notes", notes ?? (object)DBNull.Value),
                new SqlParameter("@CreatedBy", userId)
            };

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
        }

        public static bool AddInvoiceDetail(int invoiceId, int productId, int unitId,
            decimal quantity, decimal unitPrice, decimal costPrice, decimal discount,
            decimal tax, decimal total)
        {
            decimal profit = (unitPrice - costPrice) * quantity - (unitPrice * quantity * discount / 100);

            string query = @"
                INSERT INTO SalesInvoiceDetails (InvoiceID, ProductID, UnitID, Quantity,
                    UnitPrice, CostPrice, DiscountPercentage, TaxPercentage, TotalPrice, Profit)
                VALUES (@InvoiceID, @ProductID, @UnitID, @Quantity, @UnitPrice, @CostPrice,
                    @DiscountPercentage, @TaxPercentage, @TotalPrice, @Profit)";

            SqlParameter[] parameters = {
                new SqlParameter("@InvoiceID", invoiceId),
                new SqlParameter("@ProductID", productId),
                new SqlParameter("@UnitID", unitId),
                new SqlParameter("@Quantity", quantity),
                new SqlParameter("@UnitPrice", unitPrice),
                new SqlParameter("@CostPrice", costPrice),
                new SqlParameter("@DiscountPercentage", discount),
                new SqlParameter("@TaxPercentage", tax),
                new SqlParameter("@TotalPrice", total),
                new SqlParameter("@Profit", profit)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public static DataTable GetInvoices(DateTime? startDate = null, DateTime? endDate = null)
        {
            string query = @"
                SELECT si.*, c.CustomerName, w.WarehouseName, sr.RepName, cur.CurrencyCode
                FROM SalesInvoices si
                INNER JOIN Customers c ON si.CustomerID = c.CustomerID
                INNER JOIN Warehouses w ON si.WarehouseID = w.WarehouseID
                LEFT JOIN SalesRepresentatives sr ON si.SalesRepID = sr.SalesRepID
                INNER JOIN Currencies cur ON si.CurrencyID = cur.CurrencyID
                WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();

            if (startDate.HasValue)
            {
                query += " AND si.InvoiceDate >= @StartDate";
                parameters.Add(new SqlParameter("@StartDate", startDate.Value));
            }

            if (endDate.HasValue)
            {
                query += " AND si.InvoiceDate <= @EndDate";
                parameters.Add(new SqlParameter("@EndDate", endDate.Value));
            }

            query += " ORDER BY si.InvoiceDate DESC";

            return DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
        }

        public static DataTable GetInvoiceDetails(int invoiceId)
        {
            string query = @"
                SELECT sid.*, p.ProductName, u.UnitName
                FROM SalesInvoiceDetails sid
                INNER JOIN Products p ON sid.ProductID = p.ProductID
                INNER JOIN Units u ON sid.UnitID = u.UnitID
                WHERE sid.InvoiceID = @InvoiceID";

            return DatabaseHelper.ExecuteQuery(query,
                new[] { new SqlParameter("@InvoiceID", invoiceId) });
        }

        private static string GenerateInvoiceNumber()
        {
            string prefix = "INV";
            string query = "SELECT TOP 1 InvoiceNumber FROM SalesInvoices ORDER BY InvoiceID DESC";
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

        public static DataTable GetProfitAnalysis(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT
                    p.ProductName,
                    SUM(sid.Quantity) AS TotalQuantity,
                    SUM(sid.TotalPrice) AS TotalSales,
                    SUM(sid.CostPrice * sid.Quantity) AS TotalCost,
                    SUM(sid.Profit) AS TotalProfit,
                    (SUM(sid.Profit) / NULLIF(SUM(sid.TotalPrice), 0)) * 100 AS ProfitMargin
                FROM SalesInvoiceDetails sid
                INNER JOIN SalesInvoices si ON sid.InvoiceID = si.InvoiceID
                INNER JOIN Products p ON sid.ProductID = p.ProductID
                WHERE si.InvoiceDate BETWEEN @StartDate AND @EndDate
                GROUP BY p.ProductName
                ORDER BY TotalProfit DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseHelper.ExecuteQuery(query, parameters);
        }

        public static DataTable GetSalesReportBySalesRep(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT
                    sr.RepName,
                    COUNT(si.InvoiceID) AS InvoiceCount,
                    SUM(si.TotalAmount) AS TotalSales,
                    SUM(sid.Profit) AS TotalProfit,
                    (SUM(sid.Profit) * sr.CommissionPercentage / 100) AS Commission
                FROM SalesInvoices si
                INNER JOIN SalesRepresentatives sr ON si.SalesRepID = sr.SalesRepID
                INNER JOIN SalesInvoiceDetails sid ON si.InvoiceID = sid.InvoiceID
                WHERE si.InvoiceDate BETWEEN @StartDate AND @EndDate
                GROUP BY sr.RepName, sr.CommissionPercentage
                ORDER BY TotalSales DESC";

            SqlParameter[] parameters = {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            return DatabaseHelper.ExecuteQuery(query, parameters);
        }
    }
}
