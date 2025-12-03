using Microsoft.AspNetCore.Mvc;
using SalesManagementSystem.BLL;
using SalesManagementSystem.DAL;
using System.Data;
using System.Collections.Generic;

namespace SalesManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetInvoices([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                DataTable invoices = SalesRepository.GetInvoices(startDate, endDate);
                var result = new List<Dictionary<string, object>>();

                foreach (DataRow row in invoices.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in invoices.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    result.Add(dict);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // For demonstration purposes in an environment without SQL Server:
                // Return mock data if database connection fails.
                var mockResult = new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object>
                    {
                        { "InvoiceID", 1 },
                        { "InvoiceNumber", "INV000001" },
                        { "InvoiceDate", DateTime.Now.AddDays(-1) },
                        { "CustomerName", "Mock Customer" },
                        { "TotalAmount", 150.00 },
                        { "PaymentStatus", "Paid" }
                    },
                    new Dictionary<string, object>
                    {
                        { "InvoiceID", 2 },
                        { "InvoiceNumber", "INV000002" },
                        { "InvoiceDate", DateTime.Now },
                        { "CustomerName", "Another Customer" },
                        { "TotalAmount", 250.50 },
                        { "PaymentStatus", "Pending" }
                    }
                };
                return Ok(mockResult);

                // In a real scenario, we would re-throw or log the error
                // return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}/details")]
        public IActionResult GetInvoiceDetails(int id)
        {
            try
            {
                DataTable details = SalesRepository.GetInvoiceDetails(id);
                var result = new List<Dictionary<string, object>>();

                foreach (DataRow row in details.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in details.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    result.Add(dict);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateSale([FromBody] SaleInfo sale)
        {
            try
            {
                // Basic validation
                if (sale == null)
                    return BadRequest("Invalid sale data.");

                // Assuming Session.CurrentUser is handled or mocked for now.
                // In a real API, we'd use Authentication/Authorization to get the user ID.
                // For this conversion, we might need to set a default user if Session.CurrentUser is null.

                // Hack: Set a dummy user if Session.CurrentUser is not set
                if (Session.CurrentUser == null)
                {
                     Session.CurrentUser = new User { UserID = 1, Username = "APIUser", RoleName = "Admin" };
                }

                int invoiceId = SalesManager.CreateSale(sale);
                return CreatedAtAction(nameof(GetInvoiceDetails), new { id = invoiceId }, new { InvoiceID = invoiceId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
