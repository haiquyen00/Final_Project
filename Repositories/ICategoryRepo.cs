using Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public interface ICategoryRepo
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<Category> GetCategoryWithBooksAsync(int id);
        Task<(bool success, string message)> CreateCategoryAsync(Category category);
        Task<(bool success, string message)> UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
        Task<bool> HasBooksAsync(int id);
    }
}