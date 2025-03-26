using Business;
using Business.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
    public class NotificationLogRepo : INotificationLogRepo
    {
        private readonly AppDBContext _context;

        public NotificationLogRepo(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationLog>> GetRecentLogsAsync(int count = 100)
        {
            return await _context.NotificationLog
                .Include(nl => nl.User)
                .Include(nl => nl.NotificationTemplate)
                .Include(nl => nl.BorrowRecord)
                    .ThenInclude(br => br.Book)
                .OrderByDescending(nl => nl.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<NotificationLog> GetLogByIdAsync(int id)
        {
            return await _context.NotificationLog
                .Include(nl => nl.User)
                .Include(nl => nl.NotificationTemplate)
                .Include(nl => nl.BorrowRecord)
                    .ThenInclude(br => br.Book)
                .FirstOrDefaultAsync(nl => nl.ID == id);
        }

        public async Task<List<NotificationLog>> GetLogsByUserAsync(int userId)
        {
            return await _context.NotificationLog
                .Include(nl => nl.NotificationTemplate)
                .Include(nl => nl.BorrowRecord)
                    .ThenInclude(br => br.Book)
                .Where(nl => nl.UserId == userId)
                .OrderByDescending(nl => nl.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<NotificationLog>> GetLogsByBorrowRecordAsync(int borrowRecordId)
        {
            return await _context.NotificationLog
                .Include(nl => nl.NotificationTemplate)
                .Where(nl => nl.BorrowRecordId == borrowRecordId)
                .OrderByDescending(nl => nl.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateLogAsync(NotificationLog log)
        {
            await _context.NotificationLog.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLogAsync(NotificationLog log)
        {
            _context.Entry(log).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetPendingNotificationsCountAsync()
        {
            return await _context.NotificationLog
                .CountAsync(nl => !nl.Sent && string.IsNullOrEmpty(nl.ErrorMessage));
        }

        public async Task<List<NotificationLog>> GetFailedNotificationsAsync()
        {
            return await _context.NotificationLog
                .Include(nl => nl.User)
                .Include(nl => nl.NotificationTemplate)
                .Where(nl => !nl.Sent && !string.IsNullOrEmpty(nl.ErrorMessage))
                .OrderByDescending(nl => nl.CreatedAt)
                .ToListAsync();
        }

        public async Task<DateTime?> GetLastNotificationDateForUserAsync(int userId)
        {
            return await _context.NotificationLog
                .Where(nl => nl.UserId == userId && nl.Sent)
                .OrderByDescending(nl => nl.SentAt)
                .Select(nl => nl.SentAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<NotificationLog>> GetNotificationsByTypeAsync(string type, DateTime fromDate)
        {
            return await _context.NotificationLog
                .Include(nl => nl.User)
                .Include(nl => nl.NotificationTemplate)
                .Where(nl => nl.Type == type && nl.CreatedAt >= fromDate)
                .OrderByDescending(nl => nl.CreatedAt)
                .ToListAsync();
        }
    }
}