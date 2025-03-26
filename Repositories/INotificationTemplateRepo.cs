using Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public interface INotificationTemplateRepo
    {
        Task<List<NotificationTemplate>> GetAllTemplatesAsync();
        Task<NotificationTemplate> GetTemplateByIdAsync(int id);
        Task<List<NotificationTemplate>> GetActiveTemplatesByTypeAsync(string type);
        Task CreateTemplateAsync(NotificationTemplate template);
        Task UpdateTemplateAsync(NotificationTemplate template);
        Task DeleteTemplateAsync(int id);
        Task<bool> TemplateExistsAsync(int id);
        Task<bool> TemplateNameExistsAsync(string name, string type, int? excludeId = null);
        Task<NotificationTemplate> GetTemplateForEventAsync(string type, int? daysBeforeDue = null);
    }
}