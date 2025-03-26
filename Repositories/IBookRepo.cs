using Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public interface IBookRepo
    {
        Task<List<Books>> GetAllBooksAsync();
        Task<Books> GetBookByIdAsync(int id);
        Task<Books> GetBookWithDetailsAsync(int id);
        Task<List<Books>> GetBooksByCategoryAsync(int categoryId);
        Task<bool> CreateBookAsync(Books book);
        Task<bool> UpdateBookAsync(Books book);
        Task<bool> DeleteBookAsync(int id);
        Task<bool> BookExistsAsync(int id);
        Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null);
        Task<int> GetAvailableQuantityAsync(int id);
        Task<bool> UpdateQuantityAsync(int id, int newQuantity);
        Task<bool> IsBookAvailableAsync(int id);
    }
}