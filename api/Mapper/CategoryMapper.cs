using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Category;
using api.models;

namespace api.Mapper
{
    public static class CategoryMapper
    {
        public static CategoryDto ToCategoryDto(this models.Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Products = category.Products.Select(p => p.ToProductDto()).ToList()
            };
        }

        public static Category ToCreateCategoryDto(this CreateCategoryRequestDto createCategoryRequestDto)
        {
            return new Category
            {
                Name = createCategoryRequestDto.Name
            };
        }

        public static void ToUpdateCategoryDto(this Category categoryModel, UpdateCategoryRequestDto updateCategoryRequestDto)
        {
            categoryModel.Name = updateCategoryRequestDto.Name;
        }
    }
}