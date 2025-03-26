using Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public interface ISessionRepo
    {
        Task<Sessions> CreateSessionAsync(int userId, string role);
        Task<Sessions> GetSessionAsync(string sessionId);
        Task<bool> ValidateSessionAsync(string sessionId);
        Task<bool> DeleteSessionAsync(string sessionId);
        Task DeleteExpiredSessionsAsync();
        Task<List<Sessions>> GetUserSessionsAsync(int userId);
        Task<bool> UpdateSessionExpiryAsync(string sessionId);
    }

}
