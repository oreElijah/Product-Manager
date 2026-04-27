using api.Dtos.Category;
using api.Interfaces;
using api.models;

namespace api.Interfaces
{
    public interface ICategoryService
    {
        public Task<List<CategoryDto>> GetCategories();
        public Task<CategoryDto> GetUniqueCategoryById(int id);
        public Task<CategoryDto> CreateNewCategory(CreateCategoryRequestDto categoryDto);
        public Task<Category> UpdateExistingCategory(int id, UpdateCategoryRequestDto categoryDto);
        public Task DeleteExistingCategory(int id);

    }
}
