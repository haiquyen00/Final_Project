using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Business.Models;
using Microsoft.AspNetCore.Authorization;
using Repositories;

namespace FUView.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NotificationController : Controller
    {
        private readonly INotificationTemplateRepo _templateRepo;
        private readonly INotificationLogRepo _logRepo;

        public NotificationController(
            INotificationTemplateRepo templateRepo,
            INotificationLogRepo logRepo)
        {
            _templateRepo = templateRepo;
            _logRepo = logRepo;
        }

        // GET: Notification/Templates
        public async Task<IActionResult> Templates()
        {
            var templates = await _templateRepo.GetAllTemplatesAsync();
            return View(templates);
        }

        // GET: Notification/CreateTemplate
        public IActionResult CreateTemplate()
        {
            return View();
        }

        // POST: Notification/CreateTemplate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTemplate([Bind("Name,Subject,Template,Description,Type,DaysBeforeDue")] NotificationTemplate template)
        {
            if (ModelState.IsValid)
            {
                // Check if template name already exists for this type
                if (await _templateRepo.TemplateNameExistsAsync(template.Name, template.Type))
                {
                    ModelState.AddModelError("Name", "A template with this name and type already exists");
                    return View(template);
                }

                template.IsActive = true;
                await _templateRepo.CreateTemplateAsync(template);
                return RedirectToAction(nameof(Templates));
            }
            return View(template);
        }

        // GET: Notification/EditTemplate/5
        public async Task<IActionResult> EditTemplate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var template = await _templateRepo.GetTemplateByIdAsync(id.Value);
            if (template == null)
            {
                return NotFound();
            }
            return View(template);
        }

        // POST: Notification/EditTemplate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTemplate(int id, [Bind("ID,Name,Subject,Template,Description,Type,DaysBeforeDue,IsActive")] NotificationTemplate template)
        {
            if (id != template.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if template name already exists (excluding current template)
                if (await _templateRepo.TemplateNameExistsAsync(template.Name, template.Type, template.ID))
                {
                    ModelState.AddModelError("Name", "A template with this name and type already exists");
                    return View(template);
                }

                await _templateRepo.UpdateTemplateAsync(template);
                return RedirectToAction(nameof(Templates));
            }
            return View(template);
        }

        // GET: Notification/Logs
        public async Task<IActionResult> Logs()
        {
            var logs = await _logRepo.GetRecentLogsAsync(100);
            return View(logs);
        }

        // GET: Notification/LogDetails/5
        public async Task<IActionResult> LogDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _logRepo.GetLogByIdAsync(id.Value);
            if (log == null)
            {
                return NotFound();
            }

            return View(log);
        }
    }
}