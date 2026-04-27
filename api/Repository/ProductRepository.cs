using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using api.Data;
using api.Exceptions;
using api.Interfaces;
using api.models;
using api.Dtos.Product;
using api.Mapper;
using api.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Hangfire;

namespace api.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IDistributedCache _cache;
        private readonly IEmailService _emailService;
        private readonly ILogger<ProductRepository> _logger;
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];

        public ProductRepository(ApplicationDBContext context, IDistributedCache cache, IEmailService emailService, ILogger<ProductRepository> logger)
        {
            _context = context;
            _cache = cache;
            _emailService = emailService;
            _logger = logger;
        }

        private static async Task<string> SaveProductFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new NotFoundException("No file uploaded.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new NotFoundException("Invalid File Type");
            }

            var uploadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploadDirectory);

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var absolutePath = Path.Combine(uploadDirectory, uniqueFileName);

            await using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine("uploads", uniqueFileName).Replace("\\", "/");
        }

        public async Task<List<ProductDto>> GetAllProducts()
        {
            var products = await _context.Products
                .Include(p => p.Categories)
                .ToListAsync();

            return products.Select(p => p.ToProductDto()).ToList();
        }

        public async Task<ProductDto> GetProductById(int id)
        {
            var cacheKey = $"Product_{id}";
            var cached = await _cache.GetStringAsync(cacheKey);

            // BackgroundJob.Enqueue<IEmailService>(x =>
            //    x.SendEmailAsync(
            //        "admin@example.com",
            //        "Product Request",
            //        $"A new product request has been received for product ID: {id}"));
            if (!string.IsNullOrEmpty(cached))
            {
                _logger.LogInformation("Product {ProductId} retrieved from cache.", id);
                var serializedProduct = JsonSerializer.Deserialize<ProductDto>(cached);
                return serializedProduct;
            }
            
            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }
            var options = new DistributedCacheEntryOptions(){
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            var productDto = product.ToProductDto();
            var serialized = JsonSerializer.Serialize(productDto);

            await _cache.SetStringAsync(cacheKey, serialized, options);
            _logger.LogInformation("Product {ProductId} retrieved from database.", id);
            return productDto;
        
        }

        public async Task<ProductDto> CreateProduct(CreateProductRequestDto productDto)
        {
            _logger.LogInformation("Creating product with name: {ProductName}", productDto.Name);
            var categoryIds = productDto.CategoryIds.Distinct().ToList();

            var categories = await _context.Categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync();

            if (categories.Count != categoryIds.Count)
            {
                throw new NotFoundException("One or more category IDs are invalid.");
            }

            var imagePath = await SaveProductFileAsync(productDto.file);
            var product = productDto.ToProductCreateDto(imagePath);
            product.Categories = categories;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return product.ToProductDto();
        }

        public async Task<Product> UpdateProduct(int id, UpdateProductRequestDto productDto)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }

            var imagePath = await SaveProductFileAsync(productDto.file);

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.Product_Image_URl = imagePath;

            await _context.SaveChangesAsync();
            return product;
        }
        
        public async Task DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                throw new NotFoundException("Product not found");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}