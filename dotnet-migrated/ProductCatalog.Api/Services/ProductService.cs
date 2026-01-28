using System.Collections.Concurrent;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Services;

public interface IProductService
{
    List<Product> GetAllProducts();
    Product? GetProductById(string id);
    Product CreateProduct(Product product);
    Product? UpdateProduct(string id, Product updatedProduct);
    bool DeleteProduct(string id);
    Task<Product?> UploadProductImageAsync(string id, Stream fileStream, string contentType);
    Task<byte[]> GetProductImageAsync(string id);
}

public class ProductService : IProductService
{
    private readonly ConcurrentDictionary<string, Product> _products = new();
    private readonly IS3StorageService _s3StorageService;

    public ProductService(IS3StorageService s3StorageService)
    {
        _s3StorageService = s3StorageService;
    }

    public List<Product> GetAllProducts()
    {
        return _products.Values.ToList();
    }

    public Product? GetProductById(string id)
    {
        return _products.TryGetValue(id, out var product) ? product : null;
    }

    public Product CreateProduct(Product product)
    {
        if (string.IsNullOrEmpty(product.Id))
        {
            product.Id = Guid.NewGuid().ToString();
        }
        _products[product.Id] = product;
        return product;
    }

    public Product? UpdateProduct(string id, Product updatedProduct)
    {
        if (!_products.ContainsKey(id))
        {
            return null;
        }

        updatedProduct.Id = id;

        // Preserve image URL if not provided in update
        if (updatedProduct.ImageUrl == null && _products.TryGetValue(id, out var existing))
        {
            updatedProduct.ImageUrl = existing.ImageUrl;
        }

        _products[id] = updatedProduct;
        return updatedProduct;
    }

    public bool DeleteProduct(string id)
    {
        if (_products.TryRemove(id, out var removed))
        {
            if (removed.ImageUrl != null)
            {
                var imageKey = $"products/{id}/image";
                try
                {
                    _s3StorageService.DeleteFileAsync(imageKey).Wait();
                }
                catch
                {
                    // Log but don't fail deletion
                }
            }
            return true;
        }
        return false;
    }

    public async Task<Product?> UploadProductImageAsync(string id, Stream fileStream, string contentType)
    {
        var product = GetProductById(id);
        if (product == null)
        {
            return null;
        }

        var imageKey = $"products/{id}/image";
        var imageUrl = await _s3StorageService.UploadFileAsync(imageKey, fileStream, contentType);

        product.ImageUrl = imageUrl;
        _products[id] = product;

        return product;
    }

    public async Task<byte[]> GetProductImageAsync(string id)
    {
        var imageKey = $"products/{id}/image";
        return await _s3StorageService.DownloadFileAsync(imageKey);
    }
}
