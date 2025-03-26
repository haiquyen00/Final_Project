using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Repositories;
using Microsoft.Extensions.Logging;

namespace FUView.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly IBookRepo _bookRepo;
        private readonly ICategoryRepo _categoryRepo;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IBookRepo bookRepo, ICategoryRepo categoryRepo, ILogger<BooksController> logger)
        {
            _bookRepo = bookRepo;
            _categoryRepo = categoryRepo;
            _logger = logger;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var books = await _bookRepo.GetAllBooksAsync();
            return View(User.IsInRole("Admin") ? "AdminIndex" : "Index", books);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookRepo.GetBookWithDetailsAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Author,CategoryId,ISBN,Quantity,Description")] Books book)
        {
            // Chuẩn hóa ISBN trước khi validate
            book.ISBN = book.ISBN?.Replace("-", "").Replace(" ", "").Trim();

            if (ModelState.IsValid)
            {
                if (await _bookRepo.CreateBookAsync(book))
                {
                    TempData["Success"] = "Thêm sách thành công";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("ISBN", "ISBN không hợp lệ hoặc đã tồn tại");
            }

            await PrepareViewBagForCreate();
            return View(book);
        }


        private async Task PrepareViewBagForCreate()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "ID", "Name");
        }

        // GET: Books/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookRepo.GetBookByIdAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepo.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "ID", "Name", book.CategoryId);
            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Author,CategoryId,ISBN,Quantity")] Books book)
        {
            if (id != book.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (await _bookRepo.IsbnExistsAsync(book.ISBN, book.ID))
                {
                    ModelState.AddModelError("ISBN", "A book with this ISBN already exists.");
                    var categories = await _categoryRepo.GetAllCategoriesAsync();
                    ViewBag.Categories = new SelectList(categories, "ID", "Name", book.CategoryId);
                    return View(book);
                }

                if (await _bookRepo.UpdateBookAsync(book))
                {
                    _logger.LogInformation("Book updated successfully: {ID}", book.ID);
                    return RedirectToAction(nameof(Index));
                }

                if (!await _bookRepo.BookExistsAsync(id))
                {
                    return NotFound();
                }

                ModelState.AddModelError("", "Failed to update book. Please try again.");
            }

            var categoriesList = await _categoryRepo.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categoriesList, "ID", "Name", book.CategoryId);
            return View(book);
        }

        // GET: Books/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _bookRepo.GetBookWithDetailsAsync(id.Value);
            if (book == null)
            {
                return NotFound();
            }

            // Check if book has active borrows
            bool hasActiveBorrows = book.BorrowRecords?.Any(br => !br.Returned) ?? false;
            if (hasActiveBorrows)
            {
                TempData["Error"] = "Cannot delete book with active borrows.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _bookRepo.DeleteBookAsync(id))
            {
                _logger.LogInformation("Book deleted successfully: {ID}", id);
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Failed to delete book. The book may have active borrows.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Books/Category/5
        public async Task<IActionResult> Category(int? id)
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
    }
}
