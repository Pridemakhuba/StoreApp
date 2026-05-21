// ============================================================
// CategoriesController.cs
// Full CRUD for Categories.
// Delete removes category only if no products are linked,
// otherwise soft-deletes (IsActive = false).
// ============================================================

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace StoreApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly StoreContext _context;

        public CategoriesController(StoreContext context)
        {
            _context = context;
        }

        // GET: /Categories
        /// <summary>Lists all categories ordered alphabetically.</summary>
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        // GET: /Categories/Details/5
        /// <summary>Shows a category and its linked products.</summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // GET: /Categories/Create
        /// <summary>Returns blank Create form.</summary>
        public IActionResult Create() => View();

        // POST: /Categories/Create
        /// <summary>Saves a new Category to the database.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName,Description,IsActive")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: /Categories/Edit/5
        /// <summary>Loads existing category into Edit form.</summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /Categories/Edit/5
        /// <summary>Commits updates to an existing category.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryID,CategoryName,Description,IsActive,CreatedDate")] Category category)
        {
            if (id != category.CategoryID) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: /Categories/Delete/5
        /// <summary>Shows delete confirmation page.</summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /Categories/Delete/5
        /// <summary>
        /// Deletes a category. If it has linked products, soft-deletes
        /// (IsActive = false) to preserve referential integrity.
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null) return RedirectToAction(nameof(Index));

            // LINQ: check if any products reference this category
            bool hasProducts = await _context.Products
                .AnyAsync(p => p.CategoryID == id);

            if (hasProducts)
            {
                // Soft delete — cannot hard-delete due to FK constraint
                category.IsActive = false;
                _context.Update(category);
                TempData["Error"] = "Category has linked products and was deactivated instead of deleted.";
            }
            else
            {
                // Safe to hard delete
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}