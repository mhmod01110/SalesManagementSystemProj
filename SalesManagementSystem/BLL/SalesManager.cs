using System;
using SalesManagementSystem.DAL;

namespace SalesManagementSystem.BLL
{
    public class SalesManager
    {
        public static int CreateSale(SaleInfo sale)
        {
            // Validate stock availability
            foreach (var item in sale.Items)
            {
                decimal availableStock = InventoryRepository.GetStock(item.ProductID, sale.WarehouseID);
                if (availableStock < item.Quantity)
                {
                    throw new Exception($"Insufficient stock for product ID {item.ProductID}");
                }
            }

            // Create invoice
            int invoiceId = SalesRepository.CreateSalesInvoice(
                sale.CustomerID, sale.WarehouseID, sale.SalesRepID, sale.CurrencyID,
                sale.SubTotal, sale.DiscountAmount, sale.TaxAmount, sale.TotalAmount,
                sale.PaidAmount, sale.PaymentMethod, sale.IsInstallment,
                Session.CurrentUser.UserID, sale.Notes);

            // Add invoice details and update inventory
            foreach (var item in sale.Items)
            {
                SalesRepository.AddInvoiceDetail(invoiceId, item.ProductID, item.UnitID,
                    item.Quantity, item.UnitPrice, item.CostPrice, item.DiscountPercentage,
                    item.TaxPercentage, item.TotalPrice);

                // Reduce stock
                InventoryRepository.UpdateStock(item.ProductID, sale.WarehouseID, -item.Quantity);
            }

            // Create installment plan if needed
            if (sale.IsInstallment && sale.NumberOfInstallments > 0)
            {
                InstallmentRepository.CreateInstallmentPlan(invoiceId, sale.TotalAmount,
                    sale.NumberOfInstallments, sale.InstallmentStartDate);
            }

            // Update customer balance
            CustomerRepository.UpdateCustomerBalance(sale.CustomerID, sale.TotalAmount - sale.PaidAmount);

            // Create accounting entry
            CreateSalesJournalEntry(invoiceId, sale);

            // Send notifications
            if (!string.IsNullOrEmpty(sale.CustomerEmail))
            {
                EmailService.SendInvoiceEmail(invoiceId, sale.CustomerEmail);
            }

            if (!string.IsNullOrEmpty(sale.CustomerPhone))
            {
                WhatsAppService.SendInvoice(invoiceId);
            }

            return invoiceId;
        }

        private static void CreateSalesJournalEntry(int invoiceId, SaleInfo sale)
        {
            int entryId = AccountingRepository.CreateJournalEntry(
                $"Sales Invoice - {invoiceId}", "Sales", invoiceId, Session.CurrentUser.UserID);

            // Debit: Cash/Accounts Receivable
            AccountingRepository.AddJournalEntryDetail(entryId, 1, sale.PaidAmount, 0, "Cash received");
            if (sale.TotalAmount > sale.PaidAmount)
            {
                AccountingRepository.AddJournalEntryDetail(entryId, 2,
                    sale.TotalAmount - sale.PaidAmount, 0, "Accounts receivable");
            }

            // Credit: Sales Revenue
            AccountingRepository.AddJournalEntryDetail(entryId, 10, 0, sale.SubTotal, "Sales revenue");

            // Credit: Tax Payable (if any)
            if (sale.TaxAmount > 0)
            {
                AccountingRepository.AddJournalEntryDetail(entryId, 15, 0, sale.TaxAmount, "Tax payable");
            }
        }
    }
}
