using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class InventoryRepository
    {
        public static DataTable GetInventoryByWarehouse(int warehouseId)
        {
            string query = @"
                SELECT i.*, p.ProductCode, p.ProductName, p.MinStockLevel,
                       u.UnitName, w.WarehouseName
                FROM Inventory i
                INNER JOIN Products p ON i.ProductID = p.ProductID
                INNER JOIN Units u ON p.BaseUnitID = u.UnitID
                INNER JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
                WHERE i.WarehouseID = @WarehouseID
                ORDER BY p.ProductName";

            return DatabaseHelper.ExecuteQuery(query,
                new[] { new SqlParameter("@WarehouseID", warehouseId) });
        }

        public static DataTable GetLowStockProducts()
        {
            string query = @"
                SELECT p.ProductCode, p.ProductName, p.MinStockLevel,
                       i.Quantity, w.WarehouseName
                FROM Inventory i
                INNER JOIN Products p ON i.ProductID = p.ProductID
                INNER JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
                WHERE i.Quantity <= p.MinStockLevel
                ORDER BY p.ProductName";

            return DatabaseHelper.ExecuteQuery(query);
        }

        public static bool UpdateStock(int productId, int warehouseId, decimal quantity)
        {
            string query = @"
                IF EXISTS (SELECT 1 FROM Inventory WHERE ProductID = @ProductID AND WarehouseID = @WarehouseID)
                    UPDATE Inventory SET Quantity = Quantity + @Quantity, LastUpdated = GETDATE()
                    WHERE ProductID = @ProductID AND WarehouseID = @WarehouseID
                ELSE
                    INSERT INTO Inventory (ProductID, WarehouseID, Quantity)
                    VALUES (@ProductID, @WarehouseID, @Quantity)";

            SqlParameter[] parameters = {
                new SqlParameter("@ProductID", productId),
                new SqlParameter("@WarehouseID", warehouseId),
                new SqlParameter("@Quantity", quantity)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public static decimal GetStock(int productId, int warehouseId)
        {
            string query = @"
                SELECT ISNULL(Quantity, 0)
                FROM Inventory
                WHERE ProductID = @ProductID AND WarehouseID = @WarehouseID";

            SqlParameter[] parameters = {
                new SqlParameter("@ProductID", productId),
                new SqlParameter("@WarehouseID", warehouseId)
            };

            object result = DatabaseHelper.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToDecimal(result) : 0;
        }
    }
}
