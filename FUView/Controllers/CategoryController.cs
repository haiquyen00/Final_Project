using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Repositories;

namespace FUView.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryRepo _categoryRepo;
        private readonly ILogger<CategoryController> _logger;


        public CategoryController(ICategoryRepo categoryRepo, ILogger<CategoryController> logger)
        {
            _categoryRepo = categoryRepo;
            _logger = logger;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoryRepo.GetCategoryWithBooksAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var (success, message) = await _categoryRepo.CreateCategoryAsync(category);
            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", message);
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoryRepo.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Name,Description")] Category category)
        {
            if (id != category.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var (success, message) = await _categoryRepo.UpdateCategoryAsync(category);
            if (success)
            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", message);
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _categoryRepo.GetCategoryWithBooksAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasBooks = await _categoryRepo.HasBooksAsync(id);
            if (hasBooks)
            {
                TempData["Error"] = "Không thể xóa danh mục đang có sách";
                return RedirectToAction(nameof(Index));
            }

            await _categoryRepo.DeleteCategoryAsync(id);
            TempData["Success"] = "Xóa danh mục thành công";
            return RedirectToAction(nameof(Index));
        }
    }
}