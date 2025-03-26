using Business;
using Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class BorrowRecordDAO
    {
        private readonly AppDBContext _context;

        public BorrowRecordDAO(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BorrowRecords>> GetBorrowRecordsAsync()
        {
            return await _context.BorrowRecord.ToListAsync();
        }

        public async Task<BorrowRecords> GetBorrowRecordByIdAsync(int id)
        {
            return await _context.BorrowRecord
                .Include(br => br.User)
                .Include(br => br.Book)
                .FirstOrDefaultAsync(br => br.ID == id);
        }

        public async Task<IEnumerable<BorrowRecords>> GetBorrowRecordsByUserIdAsync(int userId)
        {
            return await _context.BorrowRecord
                .Include(br => br.Book)
                .Where(br => br.UserID == userId)
                .OrderByDescending(br => br.BorrowDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowRecords>> GetActiveBorrowRecordsAsync()
        {
            return await _context.BorrowRecord
                .Include(br => br.User)
                .Include(br => br.Book)
                .Where(br => !br.Returned)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BorrowRecords>> GetOverdueBorrowRecordsAsync()
        {
            var currentDate = DateTime.Now;
            return await _context.BorrowRecord
                .Include(br => br.User)
                .Include(br => br.Book)
                .Where(br => !br.Returned && br.DueDate < currentDate)
                .OrderBy(br => br.DueDate)
                .ToListAsync();
        }

        public async Task<BorrowRecords> AddBorrowRecordAsync(BorrowRecords borrowRecord)
        {
            _context.BorrowRecord.Add(borrowRecord);
            await _context.SaveChangesAsync();
            return borrowRecord;
        }

        public async Task<bool> UpdateBorrowRecordAsync(BorrowRecords borrowRecord)
        {
            _context.Entry(borrowRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BorrowRecordExistsAsync(borrowRecord.ID))
                {
                    return false;
                }
                throw;
            }
        }

        public async Task<bool> ReturnBookAsync(int borrowRecordId)
        {
            var borrowRecord = await _context.BorrowRecord.FindAsync(borrowRecordId);

            if (borrowRecord == null)
            {
                return false;
            }

            borrowRecord.Returned = true;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBorrowRecordAsync(int id)
        {
            var borrowRecord = await _context.BorrowRecord.FindAsync(id);

            if (borrowRecord == null)
            {
                return false;
            }

            _context.BorrowRecord.Remove(borrowRecord);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<bool> BorrowRecordExistsAsync(int id)
        {
            return await _context.BorrowRecord.AnyAsync(br => br.ID == id);
        }
    }
}
