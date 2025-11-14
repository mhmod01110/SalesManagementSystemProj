using System;
using System.Data;
using System.Data.SqlClient;

namespace SalesManagementSystem.DAL
{
    public class ProductRepository
    {
        public static DataTable GetAllProducts()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT p.*, c.CategoryName, u.UnitName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                INNER JOIN Units u ON p.BaseUnitID = u.UnitID
                WHERE p.IsActive = 1
                ORDER BY p.ProductName");
        }

        public static DataTable SearchProducts(string searchTerm)
        {
            string query = @"
                SELECT p.*, c.CategoryName, u.UnitName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                INNER JOIN Units u ON p.BaseUnitID = u.UnitID
                WHERE p.IsActive = 1 
                AND (p.ProductCode LIKE @Search OR p.ProductName LIKE @Search OR p.Barcode LIKE @Search)
                ORDER BY p.ProductName";

            SqlParameter[] parameters = {
                new SqlParameter("@Search", "%" + searchTerm + "%")
            };

            return DatabaseHelper.ExecuteQuery(query, parameters);
        }

        public static int CreateProduct(string code, string name, int categoryId, string barcode,
            int baseUnitId, decimal costPrice, decimal retailPrice, decimal wholesalePrice, int minStock)
        {
            string query = @"
                INSERT INTO Products (ProductCode, ProductName, CategoryID, Barcode, BaseUnitID, 
                    CostPrice, RetailPrice, WholesalePrice, MinStockLevel, CreatedDate)
                VALUES (@Code, @Name, @CategoryID, @Barcode, @BaseUnitID, 
                    @CostPrice, @RetailPrice, @WholesalePrice, @MinStock, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            SqlParameter[] parameters = {
                new SqlParameter("@Code", code),
                new SqlParameter("@Name", name),
                new SqlParameter("@CategoryID", categoryId),
                new SqlParameter("@Barcode", barcode ?? (object)DBNull.Value),
                new SqlParameter("@BaseUnitID", baseUnitId),
                new SqlParameter("@CostPrice", costPrice),
                new SqlParameter("@RetailPrice", retailPrice),
                new SqlParameter("@WholesalePrice", wholesalePrice),
                new SqlParameter("@MinStock", minStock)
            };

            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
        }

        public static bool UpdateProduct(int productId, string name, int categoryId,
            decimal costPrice, decimal retailPrice, decimal wholesalePrice, int minStock)
        {
            string query = @"
                UPDATE Products 
                SET ProductName = @Name, CategoryID = @CategoryID, 
                    CostPrice = @CostPrice, RetailPrice = @RetailPrice, 
                    WholesalePrice = @WholesalePrice, MinStockLevel = @MinStock
                WHERE ProductID = @ProductID";

            SqlParameter[] parameters = {
                new SqlParameter("@ProductID", productId),
                new SqlParameter("@Name", name),
                new SqlParameter("@CategoryID", categoryId),
                new SqlParameter("@CostPrice", costPrice),
                new SqlParameter("@RetailPrice", retailPrice),
                new SqlParameter("@WholesalePrice", wholesalePrice),
                new SqlParameter("@MinStock", minStock)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }

        public static DataTable GetProductUnits(int productId)
        {
            string query = @"
                SELECT pu.*, u.UnitName, u.ShortName
                FROM ProductUnits pu
                INNER JOIN Units u ON pu.UnitID = u.UnitID
                WHERE pu.ProductID = @ProductID";

            return DatabaseHelper.ExecuteQuery(query,
                new[] { new SqlParameter("@ProductID", productId) });
        }

        public static bool AddProductUnit(int productId, int unitId, decimal conversionFactor, decimal price)
        {
            string query = @"
                INSERT INTO ProductUnits (ProductID, UnitID, ConversionFactor, Price)
                VALUES (@ProductID, @UnitID, @ConversionFactor, @Price)";

            SqlParameter[] parameters = {
                new SqlParameter("@ProductID", productId),
                new SqlParameter("@UnitID", unitId),
                new SqlParameter("@ConversionFactor", conversionFactor),
                new SqlParameter("@Price", price)
            };

            return DatabaseHelper.ExecuteNonQuery(query, parameters) > 0;
        }
    }
}
