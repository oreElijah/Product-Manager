using api.Data;
using api.Dtos.Category;
using api.Exceptions;
using api.models;
using api.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace apiTest.RepositoryTest
{
    public class CategoryRepositoryTest
    {
        private readonly ApplicationDBContext _context;

        public CategoryRepositoryTest()
        {
            _context = GetDbContext().GetAwaiter().GetResult();
        }
        private async Task<ApplicationDBContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "ProductDb_Test")
                .Options;

            var context = new ApplicationDBContext(options);
            context.Database.EnsureCreated();

            if( await context.Categories.CountAsync() <= 0)
            {
                context.Categories.Add(new Category { Name = "Test Category 1" });
                context.Categories.Add(new Category { Name = "Test Category 2" });
                context.Categories.Add(new Category { Name = "Test Category 3" });
                context.Categories.Add(new Category { Name = "Test Category 4" });
                context.Categories.Add(new Category { Name = "Test Category 5" });

                await context.SaveChangesAsync();
            }

            return context;
        }

        [Fact]
        public async Task CategoryRepository_GetAllCateGories_ReturnsListOfCategoryDto()
        {
            // Arrange
            var repository = new CategoryRepository(_context);

            // Act
            var result = await repository.GetAllCategories();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<CategoryDto>>();
            result.Count.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task CategoryRepository_GetCategoryById_ReturnCategoryDto(int id)
        {
            //arrange
            var repo = new CategoryRepository(_context);

            //act
            var result = await repo.GetCategoryById(id);

            //act
            result.Should().NotBeNull();
            result.Should().BeOfType<CategoryDto>();
            result.Id.Should().Be(id);
            result.Name.Should().Be($"Test Category {id}");
        }

        [Theory]
        [InlineData(11)]
        [InlineData(12)]
        public async Task CategoryRepository_GetCategoryById_ThrowException(int Id)
        {
            // Arrange
            var repository = new CategoryRepository(_context);
            
            // Act
            Func<Task> result = () => repository.GetCategoryById(Id);

            // Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Category not found");
        }    
        
        [Fact]
        public async Task CategoryRepository_CreateCategory_ReturnCategory()
        {
            //arrange
            var entry = new CreateCategoryRequestDto
            {
                Name = "New Test Category"
            };

            var repo = new CategoryRepository(_context);

            //act
            var result = await repo.CreateCategory(entry);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType<CategoryDto>();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be(entry.Name);
        }

        [Fact]
        public async Task CategoryRepository_CreateCategory_ThrowException()
        {
            //arrange
            var repository = new CategoryRepository(_context);

            CreateCategoryRequestDto entry = null;

            //act
            Func<Task> result = () => repository.CreateCategory(entry);

            //assert
            await result.Should().ThrowAsync<Exception>(); 
        }
        
        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        public async Task CategoryRepository_UpdateCategory_ReturnCategory(int id)
        {
            //arrange
            var entry = new UpdateCategoryRequestDto
            {
                Name = $"Updated Test Category {id}"
            };

            var repo = new CategoryRepository(_context);

            //act
            var result = await repo.UpdateCategory(id, entry);

            //assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Category>();
            result.Id.Should().Be(id);
            result.Name.Should().Be(entry.Name);
        }

        [Theory]
        [InlineData(11)]
        [InlineData(21)]
        public async Task CategoryRepository_UpdateCategory_ThrowException(int id)
        {
            // Arrange
            var repository = new CategoryRepository(_context);
            var entry = new UpdateCategoryRequestDto
            {
                Name = $"Updated Test Category {id}"
            };

            // Act
            Func<Task> result = () => repository.UpdateCategory(id, entry);

            // Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Category not found");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task CategoryRepository_DeleteCategory_ReturnVoid(int id)
        {
            //arrange
            var repo = new CategoryRepository(_context);

            //act
            var result = repo.DeleteCategory(id);

            //assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData(11)]
        [InlineData(21)]
        public async Task CategoryRepository_DeleteCategory_ThrowException(int id)
        {
            // Arrange
            var dbContext = await GetDbContext();
            var repository = new CategoryRepository(dbContext);

            // Act
            Func<Task> result = () => repository.DeleteCategory(id);

            // Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Category not found");
        }

        [Theory]
        [InlineData(4)]
        [InlineData(5)]
        public async Task CategoryRepository_CategoryExists_ReturnBool(int id)
        {
            //Arrange
            var repo = new CategoryRepository(_context);

            //Act
            var result = await repo.CategoryExists(id);

            //Assert
            result.Should().BeTrue();
        }
    }
}
