// ============================================================
// HomeController.cs
// Renders the dashboard / home page with store summary stats.
// Demonstrates LINQ Count, Sum and anonymous projections.
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StoreApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly StoreContext _context;

        public HomeController(StoreContext context)
        {
            _context = context;
        }

        // GET: /
        /// <summary>
        /// Dashboard: uses LINQ aggregates to compute summary statistics
        /// displayed on the home page (total products, customers, orders, revenue).
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // LINQ: count active products
            ViewBag.TotalProducts = await _context.Products
                .CountAsync(p => p.IsActive);

            // LINQ: count all customers
            ViewBag.TotalCustomers = await _context.Customers.CountAsync();

            // LINQ: count all orders
            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            // LINQ: sum all completed order totals as total revenue
            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // LINQ: get the 5 most recent orders with customer names
            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // LINQ: top 5 low-stock products (active, qty < 10)
            ViewBag.LowStock = await _context.Products
                .Where(p => p.IsActive && p.StockQty < 10)
                .OrderBy(p => p.StockQty)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}
