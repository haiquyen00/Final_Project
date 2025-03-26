using Business.Models;
using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class AuthenticationRepo : IAuthenticationRepo
    {
        private readonly AuthenticationDAO _dao;


        public AuthenticationRepo(AuthenticationDAO dao)
        {
            _dao = dao;
        }

        public async Task<User> CheckLogin(string email, string password)
        {
            var user = await _dao.CheckLogin(email, password);
            return user;
        }

        public async Task Register(User user)
        {
            await _dao.Register(user);
        }

        public async Task<bool> CheckEmail(string email)
        {
            return await _dao.CheckEmail(email);
        }

    }
}
