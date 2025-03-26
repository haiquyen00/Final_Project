using Business;
using Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace DataAccess
{
    public class BookDAO
    {
        private readonly AppDBContext _appDBContext;

        public BookDAO(AppDBContext context)
        {
            _appDBContext = context;
        }

        public async Task<IEnumerable<Books>> GetBooksAsync()
        {
            return await _appDBContext.Book.ToListAsync();
        }
        





    }
}