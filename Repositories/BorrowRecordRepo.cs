using Business;
using Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
    public class BorrowRecordRepo : IBorrowRecordRepo
    {
        private readonly AppDBContext _context;
        private readonly ILogger<BorrowRecordRepo> _logger;
        private const int MaxBooksPerUser = 5;
        private const int DefaultLoanDays = 14;

        public BorrowRecordRepo(AppDBContext context, ILogger<BorrowRecordRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BorrowRecords>> GetAllBorrowRecordsAsync()
        {
            return await _context.BorrowRecord
                .Include(br => br.User)
                .Include(br => br.Book)
                .OrderByDescending(br => br.BorrowDate)
                .ToListAsync();
        }

        public async Task<BorrowRecords> GetBorrowRecordByIdAsync(int id)
        {
            return await _context.BorrowRecord
                .Include(br => br.User)
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.ID == id);
        }

        public async Task<List<BorrowRecords>> GetUserBorrowRecordsAsync(int userId)
        {
            return await _context.BorrowRecord
                .Include(br => br.Book)
                .Where(br => br.UserID == userId)
                .OrderByDescending(br => br.BorrowDate)
                .ToListAsync();
        }

        public async Task<List<BorrowRecords>> GetOverdueBorrowRecordsAsync()
        {
            var today = DateTime.Now.Date;
            return await _context.BorrowRecord
                .Include(br => br.User)
                .Include(br => br.Book)
                .Where(br => !br.Returned && br.DueDate.Date < today)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public async Task<List<BorrowRecords>> GetActiveBorrowsAsync(int userId)
        {
            return await _context.BorrowRecord
                .Include(br => br.Book)
                .Where(br => br.UserID == userId && !br.Returned)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public async Task<bool> CreateBorrowRecordAsync(BorrowRecords record)
        {
            try
            {
                if (!await CanBorrowMoreBooksAsync(record.UserID))
                {
                    _logger.LogWarning("User {UserId} has reached maximum books limit", record.UserID);
                    return false;
                }

                if (!await IsBookAvailableAsync(record.BookID))
                {
                    _logger.LogWarning("Book {BookId} is not available for borrowing", record.BookID);
                    return false;
                }

                if (record.DueDate == default)
                {
                    record.DueDate = DateTime.Now.AddDays(DefaultLoanDays);
                }

                await _context.BorrowRecord.AddAsync(record);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created borrow record: User {UserId}, Book {BookId}", 
                    record.UserID, record.BookID);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating borrow record for User {UserId}, Book {BookId}", 
                    record.UserID, record.BookID);
                return false;
            }
        }

        public async Task<bool> ReturnBookAsync(int id, DateTime returnDate)
        {
            try
            {
                var record = await _context.BorrowRecord.FindAsync(id);
                if (record == null || record.Returned)
                {
                    return false;
                }

                record.Returned = true;
                record.ReturnDate = returnDate;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book returned: BorrowRecord {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning book: BorrowRecord {Id}", id);
                return false;
            }
        }

        public async Task<bool> ExtendDueDateAsync(int id, DateTime newDueDate)
        {
            try
            {
                var record = await _context.BorrowRecord.FindAsync(id);
                if (record == null || record.Returned || DateTime.Now > record.DueDate)
                {
                    return false;
                }

                record.DueDate = newDueDate;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Extended due date: BorrowRecord {Id}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending due date: BorrowRecord {Id}", id);
                return false;
            }
        }

        public async Task<bool> CanBorrowMoreBooksAsync(int userId)
        {
            var activeLoans = await _context.BorrowRecord
                .CountAsync(br => br.UserID == userId && !br.Returned);
            return activeLoans < MaxBooksPerUser;
        }

        public async Task<bool> IsBookAvailableAsync(int bookId)
        {
            var book = await _context.Book
                .Include(b => b.BorrowRecords)
                .FirstOrDefaultAsync(b => b.ID == bookId);

            if (book == null)
            {
                return false;
            }

            var borrowedCount = book.BorrowRecords?.Count(br => !br.Returned) ?? 0;
            return borrowedCount < book.Quantity;
        }

        public async Task<List<BorrowRecords>> GetBorrowHistoryForBookAsync(int bookId)
        {
            return await _context.BorrowRecord
                .Include(br => br.User)
                .Where(br => br.BookID == bookId)
                .OrderByDescending(br => br.BorrowDate)
                .ToListAsync();
        }

        public async Task<int> GetBorrowedBooksCountAsync(int bookId)
        {
            return await _context.BorrowRecord
                .CountAsync(br => br.BookID == bookId && !br.Returned);
        }

        public async Task<bool> HasOverdueBooksAsync(int userId)
        {
            var today = DateTime.Now.Date;
            return await _context.BorrowRecord
                .AnyAsync(br => br.UserID == userId && !br.Returned && br.DueDate.Date < today);
        }

        public async Task<List<BorrowRecords>> GetDueThisWeekAsync()
        {
            var today = DateTime.Now.Date;
            var weekFromNow = today.AddDays(7);

            return await _context.BorrowRecord
                .Include(br => br.User)
                .Include(br => br.Book)
                .Where(br => !br.Returned && 
                            br.DueDate.Date >= today && 
                            br.DueDate.Date <= weekFromNow)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }
    }
}