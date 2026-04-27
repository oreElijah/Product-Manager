using api.Data;
using api.Dtos.Product;
using api.Exceptions;
using api.Interfaces;
using api.models;
using api.Repository;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using api.Dtos; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace apiTest.RepositoryTest
{
    public class ProductRepositoryTest
    {
        private readonly ApplicationDBContext _context;
        private readonly Mock<IDistributedCache> _cacheMock;
        private readonly Mock<IEmailService> _emailMock;
        private readonly Mock<ILogger<ProductRepository>> _loggerMock;
        private readonly ProductRepository _repository;

        public ProductRepositoryTest()
        {
            _context = GetDbContext().GetAwaiter().GetResult();

            _cacheMock = new Mock<IDistributedCache>();
            _emailMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<ProductRepository>>();

            // Default setup for cachemiss
            byte[] nullCache = null;
            _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(nullCache);

            _repository = new ProductRepository(_context, _cacheMock.Object, _emailMock.Object, _loggerMock.Object);
        }

        private async Task<ApplicationDBContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDBContext(options);
            context.Database.EnsureCreated();

            if (await context.Products.CountAsync() <= 0)
            {
                var category = new Category { Name = "Test Category" };
                context.Categories.Add(category);

                context.Products.Add(new Product 
                { 
                    Name = "Test Product 1", 
                    Description = "Desc 1", 
                    Price = 10,
                    Product_Image_URl = "uploads/test1.jpg",
                    Categories = new List<Category> { category }
                });

                context.Products.Add(new Product 
                { 
                    Name = "Test Product 2", 
                    Description = "Desc 2", 
                    Price = 20,
                    Product_Image_URl = "uploads/test2.jpg",
                    Categories = new List<Category> { category }
                });

                await context.SaveChangesAsync();
            }

            return context;
        }

        private IFormFile GetMockFormFile(string fileName, string content = "dummy file content")
        {
            var fileMock = new Mock<IFormFile>();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));

            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            fileMock.Setup(_ => _.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((stream, token) => ms.CopyTo(stream))
                .Returns(Task.CompletedTask);

            return fileMock.Object;
        }

        [Fact]
        public async Task ProductRepository_GetAllProducts_ReturnsList()
        {
            // Act
            var result = await _repository.GetAllProducts();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<ProductDto>>();
            result.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task ProductRepository_GetProductById_ReturnsProduct_WhenInDb()
        {
            // Arrange
            var product = await _context.Products.FirstAsync();

            // Act
            var result = await _repository.GetProductById(product.Id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(product.Id);
            result.Name.Should().Be(product.Name);
        }

        [Fact]
        public async Task ProductRepository_GetProductById_ThrowsNotFoundException_WhenMissing()
        {
            // Act
            Func<Task> act = async () => await _repository.GetProductById(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found");
        }

        [Fact]
        public async Task ProductRepository_GetProductById_ReturnsFromCache_WhenCached()
        {
            // Arrange
            var cachedDto = new ProductDto { Id = 100, Name = "Cached Product" };
            var cachedBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cachedDto));
            _cacheMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(cachedBytes);

            // Act
            var result = await _repository.GetProductById(100);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(100);
            result.Name.Should().Be("Cached Product");
        }

        [Fact]
        public async Task ProductRepository_CreateProduct_ReturnsCreatedProductDto()
        {
            // Arrange
            var category = await _context.Categories.FirstAsync();

            var reqDto = new CreateProductRequestDto
            {
                Name = "New Creation",
                Description = "Created desc",
                Price = 50,
                CategoryIds = new List<int> { category.Id },
                file = GetMockFormFile("newimage.jpg")
            };

            // Act
            var result = await _repository.CreateProduct(reqDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Creation");
            result.Product_Image_URl.Should().NotBeNullOrEmpty();
            result.Product_Image_URl.Should().Contain(".jpg");
            result.Product_Image_URl.Should().StartWith("uploads/");
        }

        [Fact]
        public async Task ProductRepository_CreateProduct_ThrowsNotFoundException_WhenCategoryMissing()
        {
            // Arrange
            var reqDto = new CreateProductRequestDto
            {
                Name = "Invalid categories",
                CategoryIds = new List<int> { 999 },
                file = GetMockFormFile("newimage.jpg")
            };

            // Act
            Func<Task> act = async () => await _repository.CreateProduct(reqDto);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("One or more category IDs are invalid.");
        }

        [Fact]
        public async Task ProductRepository_CreateProduct_ThrowsNotFoundException_WhenFileInvalid()
        {
            // Arrange
            var category = await _context.Categories.FirstAsync();

            var reqDto = new CreateProductRequestDto
            {
                Name = "Invalid file",
                CategoryIds = new List<int> { category.Id },
                file = GetMockFormFile("badfile.txt") // .txt is not allowed
            };

            // Act
            Func<Task> act = async () => await _repository.CreateProduct(reqDto);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Invalid File Type");
        }

        [Fact]
        public async Task ProductRepository_UpdateProduct_ReturnsUpdatedProduct()
        {
            // Arrange
            var existingProduct = await _context.Products.FirstAsync();

            var upDto = new UpdateProductRequestDto
            {
                Name = "Updated Name",
                Description = "Updated desc",
                Price = 199,
                file = GetMockFormFile("updatedimage.png")
            };

            // Act
            var result = await _repository.UpdateProduct(existingProduct.Id, upDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(existingProduct.Id);
            result.Name.Should().Be("Updated Name");
            result.Product_Image_URl.Should().Contain(".png");
        }

        [Fact]
        public async Task ProductRepository_UpdateProduct_ThrowsNotFoundException_WhenProductMissing()
        {
            // Arrange
            var upDto = new UpdateProductRequestDto
            {
                Name = "Missing",
                file = GetMockFormFile("updatedimage.png")
            };

            // Act
            Func<Task> act = async () => await _repository.UpdateProduct(999, upDto);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found");
        }

        [Fact]
        public async Task ProductRepository_DeleteProduct_RemovesProduct()
        {
            // Arrange
            var product = await _context.Products.FirstAsync();

            // Act
            await _repository.DeleteProduct(product.Id);

            // Assert
            var deletedProduct = await _context.Products.FindAsync(product.Id);
            deletedProduct.Should().BeNull();
        }

        [Fact]
        public async Task ProductRepository_DeleteProduct_ThrowsNotFoundException_WhenMissing()
        {
            // Act
            Func<Task> act = async () => await _repository.DeleteProduct(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Product not found");
        }
    }
}
