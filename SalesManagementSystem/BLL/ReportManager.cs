using System;
using System.Data;
using SalesManagementSystem.DAL;

namespace SalesManagementSystem.BLL
{
    public class ReportManager
    {
        public static DataTable GetSalesReport(DateTime startDate, DateTime endDate,
                    int? customerId = null, int? warehouseId = null, int? salesRepId = null)
        {
            string query = @"
                SELECT si.InvoiceNumber, si.InvoiceDate, c.CustomerName, w.WarehouseName,
                       sr.RepName, si.SubTotal, si.DiscountAmount, si.TaxAmount, 
                       si.TotalAmount, si.PaidAmount, si.PaymentStatus,
                       (SELECT SUM(sid.Profit) FROM SalesInvoiceDetails sid 
                        WHERE sid.InvoiceID = si.InvoiceID) AS TotalProfit
                FROM SalesInvoices si
                INNER JOIN Customers c ON si.CustomerID = c.CustomerID
                INNER JOIN Warehouses w ON si.WarehouseID = w.WarehouseID
                LEFT JOIN SalesRepresentatives sr ON si.SalesRepID = sr.SalesRepID
                WHERE si.InvoiceDate BETWEEN @StartDate AND @EndDate";

            List<System.Data.SqlClient.SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>
            {
                new System.Data.SqlClient.SqlParameter("@StartDate", startDate),
                new System.Data.SqlClient.SqlParameter("@EndDate", endDate)
            };

            if (customerId.HasValue)
            {
                query += " AND si.CustomerID = @CustomerID";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@CustomerID", customerId.Value));
            }

            if (warehouseId.HasValue)
            {
                query += " AND si.WarehouseID = @WarehouseID";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@WarehouseID", warehouseId.Value));
            }

            if (salesRepId.HasValue)
            {
                query += " AND si.SalesRepID = @SalesRepID";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@SalesRepID", salesRepId.Value));
            }

            query += " ORDER BY si.InvoiceDate DESC";

            return DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
        }

        public static DataTable GetInventoryReport(int? warehouseId = null)
        {
            string query = @"
                SELECT p.ProductCode, p.ProductName, c.CategoryName, u.UnitName,
                       i.Quantity, p.CostPrice, p.RetailPrice, p.WholesalePrice,
                       (i.Quantity * p.CostPrice) AS TotalValue,
                       w.WarehouseName, p.MinStockLevel,
                       CASE WHEN i.Quantity <= p.MinStockLevel THEN 'Low' ELSE 'OK' END AS StockStatus
                FROM Inventory i
                INNER JOIN Products p ON i.ProductID = p.ProductID
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                INNER JOIN Units u ON p.BaseUnitID = u.UnitID
                INNER JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
                WHERE 1=1";

            List<System.Data.SqlClient.SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>();

            if (warehouseId.HasValue)
            {
                query += " AND i.WarehouseID = @WarehouseID";
                parameters.Add(new System.Data.SqlClient.SqlParameter("@WarehouseID", warehouseId.Value));
            }

            query += " ORDER BY p.ProductName";

            return DatabaseHelper.ExecuteQuery(query, parameters.ToArray());
        }

        public static DataTable GetDashboardKPIs(DateTime startDate, DateTime endDate)
        {
            string query = @"
                SELECT 
                    (SELECT COUNT(*) FROM SalesInvoices WHERE InvoiceDate BETWEEN @StartDate AND @EndDate) AS TotalInvoices,
                    (SELECT SUM(TotalAmount) FROM SalesInvoices WHERE InvoiceDate BETWEEN @StartDate AND @EndDate) AS TotalSales,
                    (SELECT SUM(PaidAmount) FROM SalesInvoices WHERE InvoiceDate BETWEEN @StartDate AND @EndDate) AS TotalCollected,
                    (SELECT SUM(sid.Profit) FROM SalesInvoiceDetails sid 
                     INNER JOIN SalesInvoices si ON sid.InvoiceID = si.InvoiceID 
                     WHERE si.InvoiceDate BETWEEN @StartDate AND @EndDate) AS TotalProfit,
                    (SELECT SUM(Amount) FROM Expenses WHERE ExpenseDate BETWEEN @StartDate AND @EndDate) AS TotalExpenses,
                    (SELECT COUNT(*) FROM Customers WHERE IsActive = 1) AS TotalCustomers,
                    (SELECT COUNT(*) FROM Products WHERE IsActive = 1) AS TotalProducts,
                    (SELECT COUNT(*) FROM Inventory i INNER JOIN Products p ON i.ProductID = p.ProductID 
                     WHERE i.Quantity <= p.MinStockLevel) AS LowStockItems";

            System.Data.SqlClient.SqlParameter[] parameters = {
                new System.Data.SqlClient.SqlParameter("@StartDate", startDate),
                new System.Data.SqlClient.SqlParameter("@EndDate", endDate)
            };

            return DatabaseHelper.ExecuteQuery(query, parameters);
        }
    }
}
