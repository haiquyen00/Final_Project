using BCrypt.Net;
using Business;
using Business.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess
{
    public class AuthenticationDAO
    {
        private readonly AppDBContext _context;

        public AuthenticationDAO(AppDBContext context)
        {
            _context = context;
        }

        public async Task<User>  CheckLogin(string email, string password)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }
            return null;
        }

        public async Task Register(User user)
        {
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();
        }


        public async Task<bool> CheckEmail(string email)
        {
            return await _context.User.AnyAsync(u => u.Email == email);
        }
    }
}
