using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.models;
using api.Dtos.Category;
namespace api.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<List<CategoryDto>> GetAllCategories();
        public Task<CategoryDto> GetCategoryById(int id);
        public Task<CategoryDto> CreateCategory(CreateCategoryRequestDto category);
        public Task<Category> UpdateCategory(int id, UpdateCategoryRequestDto category);
        public Task DeleteCategory(int id); 
        public Task<bool> CategoryExists(int id);
    }
}