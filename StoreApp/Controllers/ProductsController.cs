// ============================================================
// ProductsController.cs
// Full CRUD for Products with search and category filter.
// Delete removes product only if not referenced in any order,
// otherwise soft-deletes (IsActive = false).
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StoreApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly StoreContext _context;

        public ProductsController(StoreContext context)
        {
            _context = context;
        }

        // GET: /Products
        /// <summary>
        /// Lists all products with optional name search and category filter.
        /// Uses LINQ Where + Include + OrderBy.
        /// </summary>
        public async Task<IActionResult> Index(string searchName, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // LINQ: optional name search
            if (!string.IsNullOrWhiteSpace(searchName))
                query = query.Where(p => p.ProductName.Contains(searchName));

            // LINQ: optional category filter
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryID == categoryId.Value);

            ViewBag.Categories = new SelectList(
                await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryID", "CategoryName", categoryId);

            ViewBag.SearchName = searchName;
            ViewBag.CategoryId = categoryId;

            return View(await query.OrderBy(p => p.ProductName).ToListAsync());
        }

        // GET: /Products/Details/5
        /// <summary>Shows full product details including category.</summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: /Products/Create
        /// <summary>Returns Create form with categories dropdown.</summary>
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryID", "CategoryName");

            return View();
        }

        // POST: /Products/Create
        /// <summary>Saves a new product to the database.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryID,ProductName,Description,Price,StockQty,IsActive")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(
                await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryID", "CategoryName", product.CategoryID);

            return View(product);
        }

        // GET: /Products/Edit/5
        /// <summary>Loads a product into the Edit form.</summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(
                await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryID", "CategoryName", product.CategoryID);

            return View(product);
        }

        // POST: /Products/Edit/5
        /// <summary>Commits edits to an existing product.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,CategoryID,ProductName,Description,Price,StockQty,IsActive,CreatedDate")] Product product)
        {
            if (id != product.ProductID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = new SelectList(
                await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync(),
                "CategoryID", "CategoryName", product.CategoryID);

            return View(product);
        }

        // GET: /Products/Delete/5
        /// <summary>Shows delete confirmation with product details.</summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: /Products/Delete/5
        /// <summary>
        /// Deletes a product. If it exists in any order items,
        /// soft-deletes (IsActive = false) to preserve order history.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return RedirectToAction(nameof(Index));

            // LINQ: check if product is referenced in any order item
            bool isOrdered = await _context.OrderItems
                .AnyAsync(oi => oi.ProductID == id);

            if (isOrdered)
            {
                // Soft delete — preserve order history
                product.IsActive = false;
                _context.Update(product);
                TempData["Error"] = "Product exists in orders and was deactivated instead of deleted.";
            }
            else
            {
                // Safe to hard delete
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}