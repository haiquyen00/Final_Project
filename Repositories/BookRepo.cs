using Business;
using Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Repositories
{
    public class BookRepo : IBookRepo
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BookRepo> _logger;

        public BookRepo(AppDBContext context, ILogger<BookRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Books>> GetAllBooksAsync()
        {
            return await _context.Book
                .Include(b => b.Category)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        public async Task<Books> GetBookByIdAsync(int id)
        {
            return await _context.Book
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.ID == id);
        }

        public async Task<Books> GetBookWithDetailsAsync(int id)
        {
            return await _context.Book
                .Include(b => b.Category)
                .Include(b => b.BorrowRecords)
                    .ThenInclude(br => br.User)
                .FirstOrDefaultAsync(b => b.ID == id);
        }

        public async Task<List<Books>> GetBooksByCategoryAsync(int categoryId)
        {
            return await _context.Book
                .Include(b => b.Category)
                .Where(b => b.CategoryId == categoryId)
                .OrderBy(b => b.Title)
                .ToListAsync();
        }

        private bool IsValidIsbn(string isbn)
        {
            // Lo?i b? d?u g?ch ngang và kho?ng tr?ng
            isbn = isbn?.Replace("-", "").Replace(" ", "").Trim();

            if (string.IsNullOrEmpty(isbn))
                return false;

            // Ki?m tra ?? dài
            if (isbn.Length != 10 && isbn.Length != 13)
                return false;

            // Ki?m tra ch? ch?a s? (và 'X' ? cu?i ??i v?i ISBN-10)
            if (isbn.Length == 10)
            {
                return isbn.Substring(0, 9).All(char.IsDigit) &&
                       (char.IsDigit(isbn[9]) || isbn[9] == 'X' || isbn[9] == 'x');
            }

            // ISBN-13
            return isbn.All(char.IsDigit);
        }

        public async Task<bool> CreateBookAsync(Books book)
        {
            try
            {
                // Chu?n hóa ISBN
                book.ISBN = book.ISBN?.Replace("-", "").Replace(" ", "").Trim();

                if (!IsValidIsbn(book.ISBN))
                {
                    _logger.LogWarning("Invalid ISBN format: {ISBN}", book.ISBN);
                    return false;
                }

                // Ki?m tra trùng l?p
                if (await IsbnExistsAsync(book.ISBN))
                {
                    _logger.LogWarning("Duplicate ISBN: {ISBN}", book.ISBN);
                    return false;
                }

                await _context.Book.AddAsync(book);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book: {Title}", book.Title);
                return false;
            }
        }


        private string NormalizeIsbn(string isbn)
        {
            return isbn?.Replace("-", "").Replace(" ", "").Trim();
        }
        public async Task<bool> UpdateBookAsync(Books book)
        {
            try
            {
                if (await IsbnExistsAsync(book.ISBN, book.ID))
                {
                    _logger.LogWarning("Attempted to update book with duplicate ISBN: {ISBN}", book.ISBN);
                    return false;
                }

                _context.Entry(book).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating book: {ID}", book.ID);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book: {ID}", book.ID);
                return false;
            }
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            try
            {
                var book = await _context.Book
                    .Include(b => b.BorrowRecords)
                    .FirstOrDefaultAsync(b => b.ID == id);

                if (book == null)
                {
                    return false;
                }

                if (book.BorrowRecords?.Any(br => !br.Returned) == true)
                {
                    _logger.LogWarning("Attempted to delete book with active borrows: {ID}", id);
                    return false;
                }

                _context.Book.Remove(book);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book: {ID}", id);
                return false;
            }
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _context.Book.AnyAsync(b => b.ID == id);
        }

        public async Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null)
        {
            var query = _context.Book.Where(b => b.ISBN == isbn);
            if (excludeId.HasValue)
            {
                query = query.Where(b => b.ID != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<int> GetAvailableQuantityAsync(int id)
        {
            var book = await _context.Book
                .Include(b => b.BorrowRecords)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (book == null)
            {
                return 0;
            }

            int borrowedCount = book.BorrowRecords?.Count(br => !br.Returned) ?? 0;
            return book.Quantity - borrowedCount;
        }

        public async Task<bool> UpdateQuantityAsync(int id, int newQuantity)
        {
            try
            {
                var book = await _context.Book.FindAsync(id);
                if (book == null)
                {
                    return false;
                }

                int borrowedCount = await _context.BorrowRecord
                    .CountAsync(br => br.BookID == id && !br.Returned);

                if (newQuantity < borrowedCount)
                {
                    _logger.LogWarning("Attempted to set quantity below borrowed count for book: {ID}", id);
                    return false;
                }

                book.Quantity = newQuantity;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book quantity: {ID}", id);
                return false;
            }
        }

        public async Task<bool> IsBookAvailableAsync(int id)
        {
            var bookWithBorrows = await _context.Book
                .Include(b => b.BorrowRecords)
                .FirstOrDefaultAsync(b => b.ID == id);

            if (bookWithBorrows == null)
            {
                _logger.LogWarning("Attempted to check availability for non-existent book: {ID}", id);
                return false;
            }

            var borrowedCount = bookWithBorrows.BorrowRecords?.Count(br => !br.Returned) ?? 0;
            var isAvailable = borrowedCount < bookWithBorrows.Quantity;

            _logger.LogInformation("Book {ID} availability check - Total: {Total}, Borrowed: {Borrowed}, Available: {Available}",
                id, bookWithBorrows.Quantity, borrowedCount, isAvailable);

            return isAvailable;
        }
    }
}