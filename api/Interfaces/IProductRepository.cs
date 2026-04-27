using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos;
using api.Dtos.Product;
using api.models;

namespace api.Interfaces
{
    public interface IProductRepository
    {
        Task<List<ProductDto>> GetAllProducts();
        Task<ProductDto> GetProductById(int id);
        Task<ProductDto> CreateProduct(CreateProductRequestDto productDto);
        Task<Product> UpdateProduct(int id, UpdateProductRequestDto productDto);
        Task DeleteProduct(int id);
    }
}