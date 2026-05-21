// ============================================================
// CustomersController.cs
// Full CRUD for Customers.
// Delete cascades manually: removes OrderItems → Orders → Customer.
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StoreApp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly StoreContext _context;

        public CustomersController(StoreContext context)
        {
            _context = context;
        }

        // GET: /Customers
        /// <summary>
        /// Lists all customers with optional search by name or email.
        /// Uses LINQ OrderBy + Where.
        /// </summary>
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Customers
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .AsQueryable();

            // LINQ: filter by name or email
            if (!string.IsNullOrWhiteSpace(search))
            {
                string lower = search.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(lower) ||
                    c.LastName.ToLower().Contains(lower) ||
                    c.Email.ToLower().Contains(lower));
            }

            ViewBag.Search = search;
            return View(await query.ToListAsync());
        }

        // GET: /Customers/Details/5
        /// <summary>Shows customer profile with their order history.</summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // GET: /Customers/Create
        public IActionResult Create() => View();

        // POST: /Customers/Create
        /// <summary>Saves a new customer. Checks for duplicate email first.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Phone,Address")] Customer customer)
        {
            // LINQ: duplicate email check
            bool emailTaken = await _context.Customers
                .AnyAsync(c => c.Email == customer.Email);

            if (emailTaken)
                ModelState.AddModelError("Email", "This email address is already registered.");

            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: /Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: /Customers/Edit/5
        /// <summary>Updates an existing customer record.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerID,FirstName,LastName,Email,Phone,Address,CreatedDate")] Customer customer)
        {
            if (id != customer.CustomerID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: /Customers/Delete/5
        /// <summary>Shows delete confirmation with customer details.</summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null) return NotFound();

            return View(customer);
        }

        // POST: /Customers/Delete/5
        /// <summary>
        /// Deletes a customer and all their data in order:
        /// 1. Delete all OrderItems for each order
        /// 2. Delete all Orders for the customer
        /// 3. Delete the Customer
        /// This avoids FK constraint errors on cascade.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.CustomerID == id);

            if (customer == null) return RedirectToAction(nameof(Index));

            // LINQ: get all order IDs for this customer
            var orderIds = customer.Orders.Select(o => o.OrderID).ToList();

            if (orderIds.Any())
            {
                // Step 1: delete all order items across all customer orders
                var orderItems = await _context.OrderItems
                    .Where(oi => orderIds.Contains(oi.OrderID))
                    .ToListAsync();

                _context.OrderItems.RemoveRange(orderItems);

                // Step 2: delete all orders for this customer
                _context.Orders.RemoveRange(customer.Orders);
            }

            // Step 3: delete the customer
            _context.Customers.Remove(customer);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}