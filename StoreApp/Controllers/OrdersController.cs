// ============================================================
// OrdersController.cs — Full CRUD for Orders and OrderItems
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StoreApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly StoreContext _context;

        public OrdersController(StoreContext context)
        {
            _context = context;
        }

        // GET: /Orders
        /// <summary>Lists all orders with optional status filter via LINQ Where.</summary>
        public async Task<IActionResult> Index(string status)
        {
            var query = _context.Orders
                .Include(o => o.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            ViewBag.Statuses = await _context.Orders
                .Select(o => o.Status).Distinct().ToListAsync();

            ViewBag.SelectedStatus = status;
            return View(orders);
        }

        // GET: /Orders/Details/5
        /// <summary>Shows order with all line items using nested ThenInclude.</summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // GET: /Orders/Create
        /// <summary>Returns Create form with customer dropdown.</summary>
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = new SelectList(
                await _context.Customers.OrderBy(c => c.LastName).ToListAsync(),
                "CustomerID", "FullName");
            return View();
        }

        // POST: /Orders/Create
        /// <summary>Saves new order then redirects to AddItem.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerID,Status,Notes")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AddItem), new { orderId = order.OrderID });
            }

            ViewBag.Customers = new SelectList(
                await _context.Customers.OrderBy(c => c.LastName).ToListAsync(),
                "CustomerID", "FullName", order.CustomerID);
            return View(order);
        }

        // GET: /Orders/AddItem?orderId=5
        /// <summary>
        /// Returns form to add a product line item.
        /// Passes full product list (not SelectList) so the view
        /// can read data-price attributes for auto-fill.
        /// </summary>
        public async Task<IActionResult> AddItem(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null) return NotFound();

            // Pass full product objects so view can read Price for auto-fill
            ViewBag.ProductList = await _context.Products
                .Where(p => p.IsActive && p.StockQty > 0)
                .OrderBy(p => p.ProductName)
                .ToListAsync();

            ViewBag.Order = order;
            return View(new OrderItem { OrderID = orderId });
        }

        // POST: /Orders/AddItem
        /// <summary>
        /// Saves OrderItem and recalculates order total via LINQ SumAsync.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem([Bind("OrderID,ProductID,Quantity,UnitPrice")] OrderItem item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();

                // LINQ: recalculate total from all line items
                var order = await _context.Orders.FindAsync(item.OrderID);
                order.TotalAmount = await _context.OrderItems
                    .Where(oi => oi.OrderID == item.OrderID)
                    .SumAsync(oi => oi.Subtotal);

                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = item.OrderID });
            }

            ViewBag.ProductList = await _context.Products
                .Where(p => p.IsActive).OrderBy(p => p.ProductName).ToListAsync();
            return View(item);
        }

        // GET: /Orders/Edit/5
        /// <summary>Loads order into Edit form with customer dropdown pre-selected.</summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            ViewBag.Customers = new SelectList(
                await _context.Customers.OrderBy(c => c.LastName).ToListAsync(),
                "CustomerID", "FullName", order.CustomerID);
            return View(order);
        }

        // POST: /Orders/Edit/5
        /// <summary>Updates order status, customer and notes.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderID,CustomerID,OrderDate,Status,TotalAmount,Notes")] Order order)
        {
            if (id != order.OrderID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Customers = new SelectList(
                await _context.Customers.OrderBy(c => c.LastName).ToListAsync(),
                "CustomerID", "FullName", order.CustomerID);
            return View(order);
        }

        // GET: /Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /Orders/Delete/5
        /// <summary>
        /// Deletes order safely:
        /// Step 1 — Remove all OrderItems (avoids FK constraint error)
        /// Step 2 — Remove the Order
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null) return RedirectToAction(nameof(Index));

            if (order.OrderItems.Any())
                _context.OrderItems.RemoveRange(order.OrderItems);

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}