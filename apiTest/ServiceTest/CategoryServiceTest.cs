using api.Dtos.Category;
using api.Exceptions;
using api.Interfaces;
using api.Services;
using FluentAssertions;
using api.models;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace apiTest.ServiceTest
{
    public class CategoryServiceTest
    {
        private readonly Mock<ICategoryRepository> _mockRepo;
        public CategoryServiceTest()
        {
            _mockRepo = new Mock<ICategoryRepository>();
        }

        [Fact]
        public async Task CategoryService_GetCategories_returnListCategoryDto()
        {
            //arrange
            var categoryList = new List<CategoryDto>
            {
                new CategoryDto { Id = 1, Name = "Category 1" },
                new CategoryDto { Id = 2, Name = "Category 2" }
            };

            _mockRepo.Setup(c => c.GetAllCategories())
                .ReturnsAsync(categoryList);

            var service = new CategoryService(_mockRepo.Object);

            //act
            var result = await service.GetCategories();

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<CategoryDto>>();
            result[0].Name.Should().Be("Category 1");
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task CategoryService_GetCategories_ThrowException()
        {
            //arrange
            _mockRepo.Setup(s => s.GetAllCategories())
                .ThrowsAsync(new NotFoundException("Categories not found"));

            var service = new CategoryService(_mockRepo.Object);

            //act
            Func<Task> result = () => service.GetCategories();

            //assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Categories not found");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task CategoryService_GetUniqueCategoryById_ReturnCategoryDto(int id)
        {
            //arrange
            var category = new CategoryDto {
                Id = id,
                Name = $"Category {id}"
            };

            _mockRepo.Setup(s => s.GetCategoryById(id))
                .ReturnsAsync(category);

            var service = new CategoryService(_mockRepo.Object);

            //act
            var result = await service.GetUniqueCategoryById(id);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CategoryDto>();
            result.Id.Should().Be(id);
            result.Name.Should().Be($"Category {id}");  
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task CategoryService_GetUniqueCategoryById_ThrowException(int id)
        {
            //arrange
            _mockRepo.Setup(s => s.CategoryExists(id))
                .ThrowsAsync(new NotFoundException("Category not Found"));

            var service = new CategoryService(_mockRepo.Object);

            //act
            Func<Task> result = () => service.GetUniqueCategoryById(id);

            //assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Category not Found");
        }

        [Fact]
        public async Task CategoryService_CreateNewCategory_ReturnCategoryDto()
        {
            //arrange
            var entry = new CreateCategoryRequestDto
            {
                Name = "New Category"
            };

            _mockRepo.Setup(c => c.CreateCategory(entry))
                .ReturnsAsync(new CategoryDto
                {
                    Id = 1,
                    Name = "New Category"
                });

            var service = new CategoryService(_mockRepo.Object);

            //act
            var result = await service.CreateNewCategory(entry);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CategoryDto>();
            result.Id.Should().Be(1);
            result.Name.Should().Be(entry.Name);
        }

        [Fact]
        public async Task CategoryService_CreateNewCategory_ThrowException()
        {
            //arrange
            var entry = new CreateCategoryRequestDto
            {
                Name = "New Category"
            };

            _mockRepo.Setup(c => c.CreateCategory(entry))
                .ThrowsAsync(new NotFoundException("Category not Created"));

            var services = new CategoryService(_mockRepo.Object);

            //act
            Func<Task> result = () => services.CreateNewCategory(entry);

            //assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Category not created");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task CategoryService_UpdateExistingCategory_ReturnCategory(int id)
        {
            //arrange
            var entry = new UpdateCategoryRequestDto
            {
                Name = $"Updated Category {id}"
            };

            _mockRepo.Setup(c => c.UpdateCategory(id, entry))
                .ReturnsAsync(new Category
                {
                    Id = id,
                    Name = $"Updated Category {id}"
                });

            var service = new CategoryService(_mockRepo.Object);

            //act
            var result = await service.UpdateExistingCategory(id, entry);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Category>();
            result.Id.Should().Be(id);
            result.Name.Should().Be($"Updated Category {id}");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task CategoryService_UpdateExistingCategory_ThrowException(int id)
        {
            //arrange
            var entry = new UpdateCategoryRequestDto
            {
                Name = $"Updated Category {id}"
            };

            _mockRepo.Setup(s => s.UpdateCategory(id, entry))
                .ThrowsAsync(new NotFoundException("Category not found"));

            var service = new CategoryService(_mockRepo.Object);

            //act
            Func<Task> result = () => service.UpdateExistingCategory(id, entry);

            //assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Category not found");
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task CategoryService_DeleteExistingCategory_ReturnVoid(int id)
        {
            //arrange
            _mockRepo.Setup(s => s.DeleteCategory(id));

            var service = new CategoryService(_mockRepo.Object);

            //act
            var result =  service.DeleteExistingCategory(id);

            //assert
            result.Should().NotBeNull();
        }

    }
}
