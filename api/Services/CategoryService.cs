using api.Dtos.Category;
using api.Exceptions;
using api.Interfaces;
using api.models;

namespace api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CategoryDto>> GetCategories() 
        {
            var categories = await _repo.GetAllCategories();
            if (categories == null)
            {
                throw new NotFoundException("Categories not found");
            }
            return categories;
        }

        public async Task<CategoryDto> GetUniqueCategoryById(int id)
        {
            var category = await _repo.GetCategoryById(id);
            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }
            return category;
        }

        public async Task<CategoryDto> CreateNewCategory(CreateCategoryRequestDto categoryDto)
        {
            var category = await _repo.CreateCategory(categoryDto);
            if (category == null)
            {
                throw new NotFoundException("Category not created");
            }
            return category;
        }

        public async Task<Category> UpdateExistingCategory(int id, UpdateCategoryRequestDto categoryDto)
        {
            var category = await _repo.UpdateCategory(id, categoryDto);
            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }
            return category;
        }

        public async Task DeleteExistingCategory(int id)
        {
                await _repo.DeleteCategory(id);
        }
    }
}
