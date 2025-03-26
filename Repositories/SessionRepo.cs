using Business.Models;
using Business;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class SessionRepo : ISessionRepo
    {
        private readonly AppDBContext _context;
        private readonly IConfiguration _configuration;

        public SessionRepo(AppDBContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<Sessions> CreateSessionAsync(int userId, string role)
        {
            var session = new Sessions
            {
                SessionID = Guid.NewGuid().ToString(),
                UserID = userId,
                Role = role,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("AppSettings:SessionTimeout", 720))
            };

            await _context.Session.AddAsync(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<Sessions> GetSessionAsync(string sessionId)
        {
            return await _context.Session
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionID == sessionId);
        }

        public async Task<bool> ValidateSessionAsync(string sessionId)
        {
            var session = await _context.Session
                .FirstOrDefaultAsync(s => s.SessionID == sessionId);

            if (session == null || session.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }

            // Cập nhật thời gian hết hạn
            session.ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("AppSettings:SessionTimeout", 720));
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            var session = await _context.Session.FindAsync(sessionId);
            if (session != null)
            {
                _context.Session.Remove(session);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task DeleteExpiredSessionsAsync()
        {
            var expiredSessions = await _context.Session
                .Where(s => s.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (expiredSessions.Any())
            {
                _context.Session.RemoveRange(expiredSessions);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Sessions>> GetUserSessionsAsync(int userId)
        {
            return await _context.Session
                .Where(s => s.UserID == userId)
                .ToListAsync();
        }

        public async Task<bool> UpdateSessionExpiryAsync(string sessionId)
        {
            var session = await _context.Session.FindAsync(sessionId);
            if (session != null)
            {
                session.ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("AppSettings:SessionTimeout", 720));
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

}
