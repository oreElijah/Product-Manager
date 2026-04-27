using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.models;
using api.Interfaces;
using api.Exceptions;
using Microsoft.EntityFrameworkCore;
using api.Dtos.Category;
using api.Mapper;

namespace api.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDBContext _context;

        public CategoryRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetAllCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();

            return categories.Select(c => c.ToCategoryDto()).ToList();
        }

        public async Task<CategoryDto> GetCategoryById(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }

            return category.ToCategoryDto();
        }

        public async Task<CategoryDto> CreateCategory(CreateCategoryRequestDto categoryDto)
        {
            var category = categoryDto.ToCreateCategoryDto();

            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return category.ToCategoryDto();
        }

        public async Task<Category> UpdateCategory(int id, UpdateCategoryRequestDto categoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }

            category.ToUpdateCategoryDto(categoryDto);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CategoryExists(int id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

    }
}