using Business;
using Business.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
    public class NotificationTemplateRepo : INotificationTemplateRepo
    {
        private readonly AppDBContext _context;

        public NotificationTemplateRepo(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationTemplate>> GetAllTemplatesAsync()
        {
            return await _context.NotificationTemplate
                .OrderBy(t => t.Type)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<NotificationTemplate> GetTemplateByIdAsync(int id)
        {
            return await _context.NotificationTemplate.FindAsync(id);
        }

        public async Task<List<NotificationTemplate>> GetActiveTemplatesByTypeAsync(string type)
        {
            return await _context.NotificationTemplate
                .Where(t => t.Type == type && t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task CreateTemplateAsync(NotificationTemplate template)
        {
            await _context.NotificationTemplate.AddAsync(template);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTemplateAsync(NotificationTemplate template)
        {
            template.UpdatedAt = System.DateTime.Now;
            _context.Entry(template).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTemplateAsync(int id)
        {
            var template = await _context.NotificationTemplate.FindAsync(id);
            if (template != null)
            {
                _context.NotificationTemplate.Remove(template);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> TemplateExistsAsync(int id)
        {
            return await _context.NotificationTemplate.AnyAsync(t => t.ID == id);
        }

        public async Task<bool> TemplateNameExistsAsync(string name, string type, int? excludeId = null)
        {
            var query = _context.NotificationTemplate
                .Where(t => t.Name == name && t.Type == type);

            if (excludeId.HasValue)
            {
                query = query.Where(t => t.ID != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<NotificationTemplate> GetTemplateForEventAsync(string type, int? daysBeforeDue = null)
        {
            var query = _context.NotificationTemplate
                .Where(t => t.Type == type && t.IsActive);

            if (daysBeforeDue.HasValue)
            {
                query = query.Where(t => t.DaysBeforeDue == daysBeforeDue.Value);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}