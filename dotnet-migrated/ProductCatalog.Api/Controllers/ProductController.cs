using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.Models;
using ProductCatalog.Api.Services;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public ActionResult<List<Product>> GetAllProducts()
    {
        return Ok(_productService.GetAllProducts());
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetProductById(string id)
    {
        var product = _productService.GetProductById(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpPost]
    public ActionResult<Product> CreateProduct([FromBody] Product product)
    {
        var created = _productService.CreateProduct(product);
        return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public ActionResult<Product> UpdateProduct(string id, [FromBody] Product product)
    {
        var updated = _productService.UpdateProduct(id, product);
        if (updated == null)
        {
            return NotFound();
        }
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProduct(string id)
    {
        if (_productService.DeleteProduct(id))
        {
            return NoContent();
        }
        return NotFound();
    }

    [HttpPost("{id}/image")]
    public async Task<ActionResult<Product>> UploadProductImage(string id, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        using var stream = file.OpenReadStream();
        var product = await _productService.UploadProductImageAsync(id, stream, file.ContentType);

        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    [HttpGet("{id}/image")]
    public async Task<IActionResult> GetProductImage(string id)
    {
        try
        {
            var imageData = await _productService.GetProductImageAsync(id);
            return File(imageData, "image/jpeg");
        }
        catch
        {
            return NotFound();
        }
    }
}
