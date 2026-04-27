using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos;
using api.Dtos.Product;
using api.Dtos.Category;
using api.models;

namespace api.Mapper
{
    public static class ProductMapper
    {
        public static ProductDto ToProductDto(this Product productModel)
        {
            return new ProductDto
            {
                Id = productModel.Id,
                Name = productModel.Name,
                Description = productModel.Description,
                Price = productModel.Price,
                Product_Image_URl = productModel.Product_Image_URl,
                Categories = productModel.Categories
                    .Select(c => new CategoryProductDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToList()
            };
        }

        public static Product ToProductCreateDto(this CreateProductRequestDto createProductRequestDto, string imagePath)
        {
            return new Product
            {
                Name = createProductRequestDto.Name,
                Description = createProductRequestDto.Description,
                Price = createProductRequestDto.Price,
                Product_Image_URl = imagePath
            };
        }
        
        public static void ToProductUpdateDto(this Product productModel, UpdateProductRequestDto updateProductRequestDto, string imagePath)
        {
            productModel.Name = updateProductRequestDto.Name;
            productModel.Description = updateProductRequestDto.Description;
            productModel.Price = updateProductRequestDto.Price;
            productModel.Product_Image_URl = imagePath;
        }
    }
}