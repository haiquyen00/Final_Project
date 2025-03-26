using Business.Models;

namespace Repositories
{
    public interface IAuthenticationRepo
    {
        Task<User> CheckLogin(string email, string password);
        Task Register(User user);
        Task<bool> CheckEmail(string email);
    }
}
