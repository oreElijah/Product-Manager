using Moq;
using api.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using api.models;
using api.Dtos.Category;
using api.Controllers;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;


namespace apiTest.ControllerTest
{
    public class CategoryControllerTest
    {
        private readonly Mock<ICategoryService> _mockService;
        public CategoryControllerTest()
        {
            _mockService = new Mock<ICategoryService>();
        }

        [Fact]
        public async Task CategoryController_GetAllCategories_ReturnCategoies()
        {
            //arrange
            var categoryList = new List<CategoryDto>
            {
                new CategoryDto { Id = 1, Name = "Category 1" },
                new CategoryDto { Id = 2, Name = "Category 2" }
            };

            _mockService.Setup(s => s.GetCategories())
                .ReturnsAsync(categoryList);

            var controller = new CategoryController(_mockService.Object);

            //act
            var result = await controller.GetAllCategories();
            var okResult = result as OkObjectResult;
            var returnedCategories = okResult.Value as List<CategoryDto>;

            //assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedCategories.Should().NotBeNull();
            returnedCategories.Count.Should().Be(2);
        }

        [Fact]
        public async Task CategoryController_GetAllCategories_ReturnNotFound()
        {
            //arrange
            _mockService.Setup(s => s.GetCategories())
                .ReturnsAsync(new List<CategoryDto>());

            var controller = new CategoryController(_mockService.Object);
            //act

            var result = await controller.GetAllCategories();
            var notFoundResult = result as NotFoundObjectResult;
            //var returnedCategories = notFoundResult.Value as List<CategoryDto>;

            //assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("No categories found.");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        public async Task CategoryController_GetCategoryById_ReturnCategory(int categoryId)
        {
            //arrange
            var category = new CategoryDto {
                Id = categoryId, 
                Name = $"Category {categoryId}"
            };

            _mockService.Setup(s => s.GetUniqueCategoryById(categoryId))
                .ReturnsAsync(category);

            var controller = new CategoryController(_mockService.Object);

            //act
            var result = await controller.GetCategoryById(categoryId);
            var okResult = result as OkObjectResult;
            var returnedCategory = okResult.Value as CategoryDto;

            //assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedCategory.Should().NotBeNull();
            returnedCategory.Id.Should().Be(categoryId);
            returnedCategory.Name.Should().Be($"Category {categoryId}");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        [InlineData(13)]
        public async Task CategoryController_GetCategoryById_ReturnNotFound(int categoryId)
        {
            //arrange
            _mockService.Setup(s => s.GetUniqueCategoryById(categoryId))
                .ReturnsAsync((CategoryDto)null);

            var controller = new CategoryController(_mockService.Object);

            //act
            var result = await controller.GetCategoryById(categoryId);
            var notFoundResult = result as NotFoundObjectResult;

            //assert         
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("No category found.");
        }

        [Fact]
        public async Task CategoryController_CreateCategory_ReturnCreatedAtAction()
        {
            //arrange
            var entry = new CreateCategoryRequestDto { Name = "New Category" };

            _mockService.Setup(c => c.CreateNewCategory(entry))
                .ReturnsAsync(new CategoryDto
                {
                    Id = 1,
                    Name = "New Category"
                });

            var controller = new CategoryController(_mockService.Object);

            //act
            var result = await controller.CreateCategory(entry);
            var createdAtActionResult = result as CreatedAtActionResult;
            var returnedCategory = createdAtActionResult.Value as CategoryDto;

            //assert
            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.StatusCode.Should().Be(201);
            returnedCategory.Should().NotBeNull();
            returnedCategory.Id.Should().Be(1);
            returnedCategory.Name.Should().Be("New Category");
        }

        [Fact]
        public async Task CategoryController_CreateCategory_ReturnNotFound()
        {
            //arrange
            var entry = new CreateCategoryRequestDto { 
                Name = "New Category"
            };

            _mockService.Setup(s => s.CreateNewCategory(entry))
                .ReturnsAsync((CategoryDto)null);

            var controller = new CategoryController(_mockService.Object);

            //act
            var result = await controller.CreateCategory(entry);
            var notFoundResult = result as NotFoundObjectResult;

            //assert
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be($"category not created.");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task CategoryController_UpdateCategory_ReturnCategory(int categoryId) 
        {
            var entry = new UpdateCategoryRequestDto
            {
                Name = $"Updated Category {categoryId}"
            };

            _mockService.Setup(c => c.UpdateExistingCategory(categoryId, entry))
                .ReturnsAsync(new Category
                {
                    Id = categoryId,
                    Name = $"Updated Category {categoryId}"
                });

            var controller = new CategoryController(_mockService.Object);

            //act
            var result = await controller.UpdateCategory(categoryId, entry);
            var okResult = result as OkObjectResult;
            var returnedCategory = okResult.Value as Category;

            //assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedCategory.Should().NotBeNull();
            returnedCategory.Id.Should().Be(categoryId);
            returnedCategory.Name.Should().Be($"Updated Category {categoryId}");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task CategoryController_UpdateCategory_ReturnNotFound(int id)
        {
            //arrange
            var entry = new UpdateCategoryRequestDto
            {
                Name = "New Category"
            };

            _mockService.Setup(s => s.UpdateExistingCategory(id, entry))
                .ReturnsAsync((Category)null);

            var controller = new CategoryController(_mockService.Object);

            //act
            var result = await controller.UpdateCategory(id, entry);
            var notFoundResult = result as NotFoundObjectResult;

            //assert
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be($"No category found.");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task CategoryController_DeleteCategory_ReturnVoid(int categoryId) 
        {
            //arrange
            _mockService.Setup(c => c.DeleteExistingCategory(categoryId));

            var controller = new CategoryController(_mockService.Object);

            //act
            var result = await controller.DeleteCategory(categoryId);
            var NoContentResult = result as NoContentResult;

            //assert
            NoContentResult.Should().NotBeNull();
            NoContentResult.StatusCode.Should().Be(204);
        }
    }
}
