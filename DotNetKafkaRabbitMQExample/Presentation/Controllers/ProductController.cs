using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DotNetKafkaRabbitMQExample.Domain.Entities;
using DotNetKafkaRabbitMQExample.Application.DTOs;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using DotNetKafkaRabbitMQExample.Application.Interfaces;
using DotNetKafkaRabbitMQExample.Application.DTOs.Responses;

namespace DotNetKafkaRabbitMQExample.Presentation.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IMapper mapper,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _webHostEnvironment = webHostEnvironment;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productDtos = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDtos);
        }

        [AllowAnonymous]
        [HttpGet("GetProductPage")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductPage([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {

            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Page number and page size must be greater than zero.");

            var totalProducts = _productRepository.GetTotalProductsCount();
            var totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            if(pageNumber > totalPages)
                return BadRequest($"Page number exceeds total pages. Total pages: {totalPages}.");

            var products = _productRepository.GetProductPaginated(pageNumber, pageSize);
            var productDtos = _mapper.Map<List<ProductDto>>(products);

            var paginationResult = new PaginationResponse<ProductDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Items = productDtos
            };
            return Ok(paginationResult);
        }

        [HttpGet("{id:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int id)
        {
            var product = _productRepository.GetProduct(id);
            if (product == null)
                return NotFound($"Product with id {id} not found.");

            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
                return BadRequest(ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (_productRepository.ProductExists(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", "Product already exists.");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"Category with id {createProductDto.CategoryId} does not exist.");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDto);

            product.ImgUrl = "";
            product.ImgUrlLocal = "";

            // Primero creamos el producto para obtener el ID real de la base de datos
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", "Error while creating the product.");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            // Una vez creado, manejamos la imagen con el ID real
            if (createProductDto.Image != null)
            {
                string fileName = product.Id + Guid.NewGuid().ToString() + Path.GetExtension(createProductDto.Image.FileName);
                var imageFolder = Path.Combine(_webHostEnvironment.WebRootPath, "ProductImages");

                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);
                }

                var filePath = Path.Combine(imageFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    createProductDto.Image.CopyTo(stream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                product.ImgUrl = $"{baseUrl}/ProductImages/{fileName}";
                product.ImgUrlLocal = filePath;

                // Actualizamos el producto con la URL de la imagen
                _productRepository.UpdateProduct(product);
            }
            else
            {
                product.ImgUrl = "https://placehold.co/600x400";
                _productRepository.UpdateProduct(product);
            }

            var createdProduct = _productRepository.GetProduct(product.Id);
            if (createdProduct == null)
            {
                return NotFound();
            }
            var productDto = _mapper.Map<ProductDto>(createdProduct);

            return CreatedAtRoute("GetProduct", new { id = product.Id }, productDto);
        }

        [HttpPatch("{id:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int id, [FromForm] CreateProductDto updateProductDto)
        {
            if (!_productRepository.ProductExists(id))
                return NotFound($"Product with id {id} not found.");

            if (updateProductDto == null)
                return BadRequest(ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = _mapper.Map<Product>(updateProductDto);
            product.Id = id;

            if (updateProductDto.Image != null)
            {
                string fileName = product.Id + Guid.NewGuid().ToString() + Path.GetExtension(updateProductDto.Image.FileName);
                var imageFolder = Path.Combine(_webHostEnvironment.WebRootPath, "ProductImages");

                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);
                }

                var filePath = Path.Combine(imageFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    updateProductDto.Image.CopyTo(stream);
                }

                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                product.ImgUrl = $"{baseUrl}/ProductImages/{fileName}";
                product.ImgUrlLocal = filePath;
            }
            else
            {
                // Si no se envía imagen nueva, mantenemos la anterior o ponemos la por defecto
                var existingProduct = _productRepository.GetProduct(id);
                product.ImgUrl = existingProduct?.ImgUrl ?? "https://placehold.co/600x400";
                product.ImgUrlLocal = existingProduct?.ImgUrlLocal ?? string.Empty;
            }

            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CustomError", "Error while updating the product.");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteProduct(int id)
        {
            var product = _productRepository.GetProduct(id);
            if (product is null)
                return NotFound($"Product with id {id} not found.");

            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", "Error while deleting the product.");
                return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
            }

            return NoContent();
        }
        [HttpGet("GetProductsForCategory/{categoryId:int}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            var products = _productRepository.GetProductsForCategory(categoryId);
            if (products == null || !products.Any())
                return NotFound($"No products found for category id {categoryId}.");

            var productDtos = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDtos);
        }

        [HttpGet("Search")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProduct(string searchTerm)
        {
            var products = _productRepository.SearchProduct(searchTerm);
            var productDtos = _mapper.Map<List<ProductDto>>(products);
            return Ok(productDtos);
        }

        [HttpPost("BuyProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            if (string.IsNullOrWhiteSpace(name) || quantity <= 0)
                return BadRequest("Invalid product name or quantity.");

            if (!_productRepository.ProductExists(name))
                return NotFound($"Product '{name}' not found.");

            if (!_productRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError("CustomError", "Error while processing the purchase (insufficient stock or database error).");
                return BadRequest(ModelState);
            }

            return Ok($"Purchase of {quantity} units of '{name}' was successful.");
        }
    }
}


