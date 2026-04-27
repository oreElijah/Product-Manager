using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using api.Controllers;
using api.Interfaces;
using api.Dtos.Product;
using api.Dtos;
using api.models;

namespace apiTest.ControllerTest
{
    public class ProductControllerTest
    {
        private readonly Mock<IProductService> _mockService;

        public ProductControllerTest()
        {
            _mockService = new Mock<IProductService>();
        }

        [Fact]
        public async Task ProductController_GetProducts_ReturnProducts()
        {
            //arrange
            var productList = new List<ProductDto>
            {
                new ProductDto {
                    Id = 1,
                    Name = "Product 1" 
                },
                new ProductDto {
                    Id = 2,
                    Name = "Product 2"
                }
            };

            _mockService.Setup(s => s.GetProducts())
                .ReturnsAsync(productList);

            var controller = new ProductController(_mockService.Object);

            //act
            var result = await controller.GetProducts();
            var okResult = result as OkObjectResult;
            var returnedProducts = okResult.Value as List<ProductDto>;

            //assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedProducts.Should().NotBeNull();
            returnedProducts.Count.Should().Be(2);
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        public async Task ProductController_GetProductById_ReturnProduct(int productId)
        {
            //arrange
            var product = new ProductDto {
                Id = productId, 
                Name = $"Product {productId}"
            };

            _mockService.Setup(s => s.GetUniqueProductById(productId))
                .ReturnsAsync(product);

            var controller = new ProductController(_mockService.Object);

            //act
            var result = await controller.GetProductById(productId);
            var okResult = result as OkObjectResult;
            var returnedProduct = okResult.Value as ProductDto;

            //assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedProduct.Should().NotBeNull();
            returnedProduct.Id.Should().Be(productId);
            returnedProduct.Name.Should().Be($"Product {productId}");
        }

        [Fact]
        public async Task ProductController_CreateProduct_ReturnCreatedAtAction()
        {
            //arrange
            var entry = new CreateProductRequestDto { Name = "New Product" };

            _mockService.Setup(s => s.CreateNewProduct(entry))
                .ReturnsAsync(new ProductDto
                {
                    Id = 1,
                    Name = "New Product"
                });

            var controller = new ProductController(_mockService.Object);

            //act
            var result = await controller.CreateProduct(entry);
            var createdAtActionResult = result as CreatedAtActionResult;
            var returnedProduct = createdAtActionResult.Value as ProductDto;

            //assert
            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.StatusCode.Should().Be(201);
            returnedProduct.Should().NotBeNull();
            returnedProduct.Id.Should().Be(1);
            returnedProduct.Name.Should().Be("New Product");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task ProductController_UpdateProduct_ReturnProduct(int productId) 
        {
            //arrange
            var entry = new UpdateProductRequestDto
            {
                Name = $"Updated Product {productId}"
            };

            _mockService.Setup(s => s.UpdateExistingProduct(productId, entry))
                .ReturnsAsync(new Product
                {
                    Id = productId,
                    Name = $"Updated Product {productId}"
                });

            var controller = new ProductController(_mockService.Object);

            //act
            var result = await controller.UpdateProduct(productId, entry);
            var okResult = result as OkObjectResult;
            var returnedProduct = okResult.Value as Product;

            //assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedProduct.Should().NotBeNull();
            returnedProduct.Id.Should().Be(productId);
            returnedProduct.Name.Should().Be($"Updated Product {productId}");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task ProductController_DeleteProduct_ReturnNoContent(int productId) 
        {
            //arrange
            _mockService.Setup(s => s.DeleteExistingProduct(productId))
                .Returns(Task.CompletedTask);

            var controller = new ProductController(_mockService.Object);

            //act
            var result = await controller.DeleteProduct(productId);
            var noContentResult = result as NoContentResult;

            //assert
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }
    }
}
