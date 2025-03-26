using Business.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public interface INotificationLogRepo
    {
        Task<List<NotificationLog>> GetRecentLogsAsync(int count = 100);
        Task<NotificationLog> GetLogByIdAsync(int id);
        Task<List<NotificationLog>> GetLogsByUserAsync(int userId);
        Task<List<NotificationLog>> GetLogsByBorrowRecordAsync(int borrowRecordId);
        Task CreateLogAsync(NotificationLog log);
        Task UpdateLogAsync(NotificationLog log);
        Task<int> GetPendingNotificationsCountAsync();
        Task<List<NotificationLog>> GetFailedNotificationsAsync();
        Task<DateTime?> GetLastNotificationDateForUserAsync(int userId);
        Task<List<NotificationLog>> GetNotificationsByTypeAsync(string type, DateTime fromDate);
    }
}