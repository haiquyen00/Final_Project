using Microsoft.AspNetCore.Mvc;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FUView.Controllers
{
    [Authorize]
    public class BorrowRecordsController : Controller
    {
        private readonly IBorrowRecordRepo _borrowRepo;
        private readonly IBookRepo _bookRepo;
        private readonly INotificationTemplateRepo _templateRepo;
        private readonly INotificationLogRepo _notificationRepo;
        private readonly ILogger<BorrowRecordsController> _logger;

        public BorrowRecordsController(
            IBorrowRecordRepo borrowRepo,
            IBookRepo bookRepo,
            INotificationTemplateRepo templateRepo,
            INotificationLogRepo notificationRepo,
            ILogger<BorrowRecordsController> logger)
        {
            _borrowRepo = borrowRepo;
            _bookRepo = bookRepo;
            _templateRepo = templateRepo;
            _notificationRepo = notificationRepo;
            _logger = logger;
        }

        // GET: BorrowRecords
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
            {
                var allRecords = await _borrowRepo.GetAllBorrowRecordsAsync();
                return View("AdminIndex", allRecords);
            }

            var userId = int.Parse(User.FindFirstValue("UserId"));
            var userRecords = await _borrowRepo.GetUserBorrowRecordsAsync(userId);
            return View(userRecords);
        }

        // GET: BorrowRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var record = await _borrowRepo.GetBorrowRecordByIdAsync(id.Value);
            if (record == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue("UserId"));
            if (!User.IsInRole("Admin") && record.UserID != userId)
            {
                return Forbid();
            }

            return View(record);
        }

        // GET: BorrowRecords/Create
        public async Task<IActionResult> Create(int? bookId)
        {
            if (bookId == null)
            {
                return NotFound();
            }

            var book = await _bookRepo.GetBookByIdAsync(bookId.Value);
            if (book == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue("UserId"));
            
            if (await _borrowRepo.HasOverdueBooksAsync(userId))
            {
                TempData["Error"] = "You have overdue books. Please return them before borrowing more.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            if (!await _borrowRepo.CanBorrowMoreBooksAsync(userId))
            {
                TempData["Error"] = "You have reached the maximum number of books you can borrow.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            if (!await _bookRepo.IsBookAvailableAsync(bookId.Value))
            {
                TempData["Error"] = "This book is currently not available.";
                return RedirectToAction("Details", "Books", new { id = bookId });
            }

            ViewBag.Book = book;
            return View(new BorrowRecords { BookID = bookId.Value, UserID = userId });
        }

        // POST: BorrowRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookID")] BorrowRecords record)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            record.UserID = userId;
            record.BorrowDate = DateTime.Now;
            record.DueDate = DateTime.Now.AddDays(14);
            record.Returned = false;

            if (ModelState.IsValid)
            {
                if (await _borrowRepo.CreateBorrowRecordAsync(record))
                {
                    // Send borrow confirmation notification
                    var borrowTemplate = await _templateRepo.GetTemplateForEventAsync("BORROW_CONFIRMATION");
                    if (borrowTemplate != null)
                    {
                        var borrowedBook = await _bookRepo.GetBookByIdAsync(record.BookID);
                        var borrowNotification = new NotificationLog
                        {
                            UserId = userId,
                            NotificationTemplateId = borrowTemplate.ID,
                            Type = "BORROW_CONFIRMATION",
                            Subject = $"Book Borrowed: {borrowedBook.Title}",
                            Content = $"You have borrowed {borrowedBook.Title}. Due date: {record.DueDate:d}",
                            BorrowRecordId = record.ID
                        };
                        await _notificationRepo.CreateLogAsync(borrowNotification);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            var book = await _bookRepo.GetBookByIdAsync(record.BookID);
            ViewBag.Book = book;
            return View(record);
        }

        // POST: BorrowRecords/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var record = await _borrowRepo.GetBorrowRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue("UserId"));
            if (!User.IsInRole("Admin") && record.UserID != userId)
            {
                return Forbid();
            }

            if (await _borrowRepo.ReturnBookAsync(id, DateTime.Now))
            {
                // Send return confirmation notification
                var returnTemplate = await _templateRepo.GetTemplateForEventAsync("RETURN_CONFIRMATION");
                if (returnTemplate != null)
                {
                    var returnedBook = await _bookRepo.GetBookByIdAsync(record.BookID);
                    var returnNotification = new NotificationLog
                    {
                        UserId = record.UserID,
                        NotificationTemplateId = returnTemplate.ID,
                        Type = "RETURN_CONFIRMATION",
                        Subject = $"Book Returned: {returnedBook.Title}",
                        Content = $"You have successfully returned {returnedBook.Title}.",
                        BorrowRecordId = record.ID
                    };
                    await _notificationRepo.CreateLogAsync(returnNotification);
                }

                TempData["Success"] = "Book returned successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to return book.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: BorrowRecords/Extend/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Extend(int id)
        {
            var record = await _borrowRepo.GetBorrowRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirstValue("UserId"));
            if (!User.IsInRole("Admin") && record.UserID != userId)
            {
                return Forbid();
            }

            var newDueDate = record.DueDate.AddDays(7);
            if (await _borrowRepo.ExtendDueDateAsync(id, newDueDate))
            {
                // Send extension confirmation notification
                var extensionTemplate = await _templateRepo.GetTemplateForEventAsync("EXTENSION_CONFIRMATION");
                if (extensionTemplate != null)
                {
                    var extendedBook = await _bookRepo.GetBookByIdAsync(record.BookID);
                    var extensionNotification = new NotificationLog
                    {
                        UserId = record.UserID,
                        NotificationTemplateId = extensionTemplate.ID,
                        Type = "EXTENSION_CONFIRMATION",
                        Subject = $"Due Date Extended: {extendedBook.Title}",
                        Content = $"The due date for {extendedBook.Title} has been extended to {newDueDate:d}.",
                        BorrowRecordId = record.ID
                    };
                    await _notificationRepo.CreateLogAsync(extensionNotification);
                }

                TempData["Success"] = "Due date extended successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to extend due date.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: BorrowRecords/Overdue (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Overdue()
        {
            var records = await _borrowRepo.GetOverdueBorrowRecordsAsync();
            return View(records);
        }
    }
}
