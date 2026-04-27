using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using api.Dtos.Product;
using api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/product/v{version:apiVersion}/")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductController(IProductService service)
        {
            _service = service;
        }

        [ApiVersion("1.0")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _service.GetProducts();
            return Ok(products);
        }

        [ApiVersion("1.0")]
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _service.GetUniqueProductById(id);
            return Ok(product);
        }

        [ApiVersion("1.0")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequestDto productDto)
        {
            var product = await _service.CreateNewProduct(productDto);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [ApiVersion("1.0")]
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromForm] UpdateProductRequestDto productDto)
        {
            var updatedProduct = await _service.UpdateExistingProduct(id, productDto);
            return Ok(updatedProduct);
        }

        [ApiVersion("1.0")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            await _service.DeleteExistingProduct(id);

            return NoContent();
        }


        // Code for the route for uploading file, was later embedded into create product route
        // [ApiVersion("1.0")]
        // [HttpPost("api/v{version:apiVersion}/upload_file")]
        // public async Task<IActionResult> UploadFile([FromForm] UploadDto uploadDto){

        //     var allowedextensions = new[] { ".jpg", ".jpeg", ".png" };

        //     var extension = Path.GetExtension(uploadDto.file.FileName);

        //     if(!allowedextensions.Contains(extension)){
        //         return BadRequest("Invalid File Type");
        //     }
        //     if(uploadDto.file==null || uploadDto.file.Length==0){
        //         return BadRequest("No file uploaded.");
        //     }
        //     if(!Directory.Exists("uploads")){
        //         Directory.CreateDirectory("Uploads");
        //         return BadRequest("No file uploads");
        //     }
        //     else{
        //         var filepath = Path.Combine("Uploads", uploadDto.file.FileName);
        //         using (var stream = new FileStream(filepath, FileMode.Create))
        //              await uploadDto.file.CopyToAsync(stream);

        //         return Ok("File uploaded successfully.");
        //     }
        //     }  
        }
    }
