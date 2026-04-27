using api.Dtos;
using api.Dtos.Product;
using api.Exceptions;
using api.Interfaces;
using api.models;
using Microsoft.CodeAnalysis;

namespace api.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ICategoryRepository _Categoryrepo;
        public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _repo = productRepository;
            _Categoryrepo = categoryRepository;
        }

        public async Task<List<ProductDto>> GetProducts()
        {
            var products = await _repo.GetAllProducts();
            if (products == null)
            {
                throw new Exception("Products not found");
            }
            return products;
        }

        public async Task<ProductDto> GetUniqueProductById(int id)
        {
            var product = await _repo.GetProductById(id);
            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }
            return product;
        }

        public async Task<ProductDto> CreateNewProduct(CreateProductRequestDto productDto)
        {
            if (!await _Categoryrepo.CategoryExists(productDto.CategoryIds.FirstOrDefault()))
            {
                throw new Exception("One or more category IDs are invalid.");
            }
            var product = await _repo.CreateProduct(productDto);
            if (product == null)
            {
                throw new NotFoundException("Product not Created");
            }
            return product;
        }

        public async Task<Product> UpdateExistingProduct(int id, UpdateProductRequestDto productDto)
        {
            if (!await _Categoryrepo.CategoryExists(productDto.CategoryIds.FirstOrDefault()))
            {
                throw new Exception("One or more category IDs are invalid.");
            }
            var product = await _repo.UpdateProduct(id, productDto);
            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }
            return product;
        }

        public async Task DeleteExistingProduct(int id)
        {
            var product = await _repo.GetProductById(id);
            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }
            await _repo.DeleteProduct(id);
        }
    }
}
