using api.Dtos;
using api.Dtos.Product;
using api.Interfaces;
using api.models;

namespace api.Interfaces
{
    public interface IProductService
    {
        public Task<List<ProductDto>> GetProducts();
        public Task<ProductDto> GetUniqueProductById(int id);
        public Task<ProductDto> CreateNewProduct(CreateProductRequestDto productDto);
        public Task<Product> UpdateExistingProduct(int id, UpdateProductRequestDto productDto);
        public Task DeleteExistingProduct(int id);
    }
}
