using api.Dtos.Category;
using api.Interfaces;
using api.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Controllers
{
    [Route("api/Category/v{version:apiVersion}/")]
    [ApiController]

    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [ApiVersion("1.0")]
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _service.GetCategories();
            if (categories == null || !categories.Any())
            {
                return NotFound("No categories found.");
            }
            return Ok(categories);
        }

        [ApiVersion("1.0")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _service.GetUniqueCategoryById(id);
            if (category == null)
            {
                return NotFound("No category found.");
            }
            return Ok(category);
        }

        [ApiVersion("1.0")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestDto categoryDto)
        {
            var category = await _service.CreateNewCategory(categoryDto);
            if (category == null)
            {
                return NotFound("category not created.");
            }
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }

        [ApiVersion("1.0")]
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] UpdateCategoryRequestDto categoryDto)
        {
            var updatedCategory = await _service.UpdateExistingCategory(id, categoryDto);
            if (updatedCategory == null)
            {
                return NotFound("No category found.");
            }
            return Ok(updatedCategory);
        }

        [ApiVersion("1.0")]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            await _service.DeleteExistingCategory(id);
            return NoContent();
        }
    }
}