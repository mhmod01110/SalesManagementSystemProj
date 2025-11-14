using System;
using System.Collections.Generic;

namespace SalesManagementSystem.BLL
{
    public class SaleInfo
    {
        public int CustomerID { get; set; }
        public int WarehouseID { get; set; }
        public int? SalesRepID { get; set; }
        public int CurrencyID { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string PaymentMethod { get; set; }
        public bool IsInstallment { get; set; }
        public int NumberOfInstallments { get; set; }
        public DateTime InstallmentStartDate { get; set; }
        public string Notes { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public List<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}
