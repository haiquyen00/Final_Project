using Business.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IBorrowRecordRepo
    {
        Task<List<BorrowRecords>> GetAllBorrowRecordsAsync();
        Task<BorrowRecords> GetBorrowRecordByIdAsync(int id);
        Task<List<BorrowRecords>> GetUserBorrowRecordsAsync(int userId);
        Task<List<BorrowRecords>> GetOverdueBorrowRecordsAsync();
        Task<List<BorrowRecords>> GetActiveBorrowsAsync(int userId);
        Task<bool> CreateBorrowRecordAsync(BorrowRecords record);
        Task<bool> ReturnBookAsync(int id, DateTime returnDate);
        Task<bool> ExtendDueDateAsync(int id, DateTime newDueDate);
        Task<bool> CanBorrowMoreBooksAsync(int userId);
        Task<bool> IsBookAvailableAsync(int bookId);
        Task<List<BorrowRecords>> GetBorrowHistoryForBookAsync(int bookId);
        Task<int> GetBorrowedBooksCountAsync(int bookId);
        Task<bool> HasOverdueBooksAsync(int userId);
        Task<List<BorrowRecords>> GetDueThisWeekAsync();
    }
}