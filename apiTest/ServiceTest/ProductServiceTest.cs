using api.Dtos;
using api.Dtos.Category;
using api.Dtos.Product;
using api.Exceptions;
using api.Interfaces;
using api.models;
using api.Services;
using Castle.Components.DictionaryAdapter.Xml;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace apiTest.ServiceTest
{
    public class ProductServiceTest
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly Mock<ICategoryRepository> _mockCategoryRepo;

        public ProductServiceTest()
        {
            _mockRepo = new Mock<IProductRepository>();
            _mockCategoryRepo = new Mock<ICategoryRepository>();
        }

        [Fact]
        public async Task ProductService_GetProducts_ReturnProducts()
        {
            //Arrange
            _mockRepo.Setup(s => s.GetAllProducts())
                .ReturnsAsync(new List<ProductDto>
                {
                    new ProductDto {
                        Id = 1,
                        Name = "Product 1",
                        Description = "Description 1",
                        Price = 10.0m,
                        Product_Image_URl = "http://example.com/product1.jpg",
                        Categories = new List<CategoryProductDto>
                        {
                            new CategoryProductDto { Id = 1, Name = "Category 1" },
                            new CategoryProductDto { Id = 2, Name = "Category 2" }
                        }
                    }});
            var services = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //Act
            var result = await services.GetProducts();

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(1);
            result[0].Name.Should().Be("Product 1");
            result.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ProductService_GetProducts_ThrowException()
        {
            //Arrange
            _mockRepo.Setup(s => s.GetAllProducts())
                .ThrowsAsync(new NotFoundException("Products not found"));

            var service = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //Act
            Func<Task> result = () => service.GetProducts();

            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Products not found");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ProductService_GetUniqueProductById_ReturnProduct(int id)
        {
            //arrange
            _mockRepo.Setup(s => s.GetProductById(id))
                .ReturnsAsync(new ProductDto
                {
                    Id = 1,
                    Name = "Product 1",
                    Description = "Description 1",
                    Price = 10.0m,
                    Product_Image_URl = "http://example.com/product1.jpg",
                    Categories = new List<CategoryProductDto>
                        {
                            new CategoryProductDto { Id = 1, Name = "Category 1" },
                            new CategoryProductDto { Id = 2, Name = "Category 2" }
                        }
                });
            var service = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //act
            var result = await service.GetUniqueProductById(id);

            //assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Product 1");
            result.Description.Should().Be("Description 1");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ProductService_GetUniqueProductById_ThrowException(int id)
        {
            //arrange
            _mockRepo.Setup(p => p.GetProductById(id))
                .Throws(new NotFoundException("Product Not Found"));

            var service = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //act
            Func<Task> result = () => service.GetUniqueProductById(id);

            //assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Product Not Found");
        }

        [Fact]
        public async Task ProductService_CreateNewProduct_ReturnProductDto()
        {
            //arrange
            var entry = new CreateProductRequestDto
            {
                Name = "New_Product",
                Description = "Description of New Product",
                Price = 100,
                file = null,
                CategoryIds = new List<int> { 1 }
            };

            _mockCategoryRepo.Setup(c => c.GetCategoryById(1))
                .ReturnsAsync(new CategoryDto {
                    Id = 1,
                    Name = "Category 1",
                    Products = new List<ProductDto> { }
                });

            _mockCategoryRepo.Setup(c => c.CategoryExists(1))
                .ReturnsAsync(true);

            _mockRepo.Setup(p => p.CreateProduct(entry))
                .ReturnsAsync(new ProductDto
                {
                    Id = 3,
                    Name = "New_Product",
                    Description = "Description of New Product",
                    Price = 100,
                    Product_Image_URl = "",
                    Categories = new List<CategoryProductDto>
                    {
                        new CategoryProductDto { Id = 1, Name = "Category 1" }
                    }
                });

            var service = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //act 
            var result = await service.CreateNewProduct(entry);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ProductDto>();
            result.Name.Should().Match(entry.Name);
            result.Description.Should().Match(entry.Description);
        }

        [Fact]
        public async Task ProductService_CreateNewProduct_ThrowInvalidCategoryId()
        {
            //arrange
            var entry = new CreateProductRequestDto
            {
                Name = "New_Product",
                Description = "Description of New Product",
                Price = 100,
                file = null,
                CategoryIds = new List<int> { 1 }
            };

            _mockRepo.Setup(p => p.CreateProduct(entry))
                .ThrowsAsync(new Exception("One or more category IDs are invalid"));

            var service = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //act 
            Func<Task> result = () => service.CreateNewProduct(entry);

            //assert
            await result.Should().ThrowAsync<Exception>()
                .WithMessage("*One or more category IDs are invalid*");
        }

        [Fact]
        public async Task ProductService_CreateNewProduct_ThrowProductNotFound()
        {
            //arrange
            var entry = new CreateProductRequestDto
            {
                Name = "New_Product",
                Description = "Description of New Product",
                Price = 100,
                file = null,
                CategoryIds = new List<int> { 1 }
            };

            _mockCategoryRepo.Setup(c => c.GetCategoryById(1))
                .ReturnsAsync(new CategoryDto
                {
                    Id = 1,
                    Name = "Category 1",
                    Products = new List<ProductDto> { }
                });

            _mockCategoryRepo.Setup(c => c.CategoryExists(1))
                .ReturnsAsync(true);

            _mockRepo.Setup(p => p.CreateProduct(entry))
                 .ThrowsAsync(new NotFoundException("Product not found"));

            var service = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //act
            Func<Task> result = () => service.CreateNewProduct(entry);

            //assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Product Not Found");

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ProductService_UpdateExistingProduct_ReturnProduct(int id)
        {
            //Arrange
            var entry = new UpdateProductRequestDto
            {
                Name = "New_Product",
                Description = "Description of New Product",
                Price = 100,
                file = null,
                CategoryIds = new List<int> { 1 }
            };

            _mockCategoryRepo.Setup(c => c.GetCategoryById(1))
               .ReturnsAsync(new CategoryDto
               {
                   Id = 1,
                   Name = "Category 1",
                   Products = new List<ProductDto> { }
               });

            _mockCategoryRepo.Setup(c => c.CategoryExists(1))
                .ReturnsAsync(true);

            _mockRepo.Setup(p => p.UpdateProduct(id, entry))
                .ReturnsAsync(new Product
                {
                    Id = 3,
                    Name = "New_Product",
                    Description = "Description of New Product",
                    Price = 100,
                    Product_Image_URl = "",
                    Categories = new List<Category>
                    {
                        new Category { Id = 1, Name = "Category 1" }
                    }
                });

            var service = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //act
            var result = await service.UpdateExistingProduct(id, entry);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Product>();
            result.Name.Should().Be(entry.Name);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ProductService_UpdateExistingProduct_ThrowInvalidCategoryId(int id) 
        {
            //arrange
            var entry = new UpdateProductRequestDto
            {
                Name = "New_Product",
                Description = "Description of New Product",
                Price = 100,
                file = null,
                CategoryIds = new List<int> { 1 }
            };

            _mockRepo.Setup(p => p.UpdateProduct(id, entry))
                .ThrowsAsync(new Exception("One or more category IDs are invalid"));

            var service = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //act
            Func<Task> result = () => service.UpdateExistingProduct(id, entry);

            //assert
            await result.Should().ThrowAsync<Exception>()
                .WithMessage("*One or more category IDs are invalid*");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ProductService_UpdateExistingProduct_ThrowProductNotFound(int id)
        {
            //arrange
            var entry = new UpdateProductRequestDto
            {
                Name = "New_Product",
                Description = "Description of New Product",
                Price = 100,
                file = null,
                CategoryIds = new List<int> { 1 }
            };

            _mockCategoryRepo.Setup(c => c.GetCategoryById(1))
                .ReturnsAsync(new CategoryDto
                {
                Id = 1,
                Name = "Category 1",
                Products = new List<ProductDto> { }
                });

            _mockCategoryRepo.Setup(c => c.CategoryExists(1))
                .ReturnsAsync(true);

            _mockRepo.Setup(p => p.UpdateProduct(id, entry))
                 .ThrowsAsync(new NotFoundException("Product not found"));

            var service = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //act
            Func<Task> result = () => service.UpdateExistingProduct(id, entry);

            //assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Product Not Found");

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ProductService_DeleteExistingProduct(int id)
        {
            //arrange
            _mockRepo.Setup(s => s.DeleteProduct(id));

            var services = new ProductService(_mockRepo.Object, _mockCategoryRepo.Object);

            //act
            var result = services.DeleteExistingProduct(id);

            //assert
            result.Should().NotBeNull();
        }

    }
}
