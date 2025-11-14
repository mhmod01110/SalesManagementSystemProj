using System;

namespace SalesManagementSystem.BLL
{
    public class SaleItem
    {
        public int ProductID { get; set; }
        public int UnitID { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
