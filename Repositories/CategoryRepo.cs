using Business;
using Business.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    public class CategoryRepo : ICategoryRepo
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CategoryRepo> _logger;

        public CategoryRepo(AppDBContext context, ILogger<CategoryRepo> logger)
        {
            _context = context;
            _logger = logger;
        }


        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Category
                .Include(c => c.Books)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _context.Category.FindAsync(id);
        }

        public async Task<Category> GetCategoryWithBooksAsync(int id)
        {
            return await _context.Category
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.ID == id);
        }

        public async Task<(bool success, string message)> CreateCategoryAsync(Category category)
        {
            try
            {
                // Kiểm tra tên category đã tồn tại
                var exists = await _context.Category
                    .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
                if (exists)
                {
                    return (false, "Danh mục này đã tồn tại");
                }

                category.Books ??= new HashSet<Books>();
                await _context.Category.AddAsync(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created new category: {Name}", category.Name);
                return (true, "Tạo danh mục thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category {Name}", category.Name);
                return (false, "Có lỗi xảy ra khi tạo danh mục");
            }
        }

        public async Task<(bool success, string message)> UpdateCategoryAsync(Category category)
        {
            try
            {
                // Kiểm tra tên mới có bị trùng không
                var exists = await _context.Category
                    .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower()
                                  && c.ID != category.ID);
                if (exists)
                {
                    return (false, "Tên danh mục này đã tồn tại");
                }

                _context.Entry(category).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated category: {Name}", category.Name);
                return (true, "Cập nhật danh mục thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category {Name}", category.Name);
                return (false, "Có lỗi xảy ra khi cập nhật danh mục");
            }
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _context.Category.FindAsync(id);
            if (category != null)
            {
                _context.Category.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _context.Category.AnyAsync(c => c.ID == id);
        }

        public async Task<bool> HasBooksAsync(int id)
        {
            return await _context.Book.AnyAsync(b => b.CategoryId == id);
        }
    }
}