# Java to .NET Migration Guide

This document details the key patterns and decisions made when migrating the Product Catalog API from Java/Spring Boot to .NET/ASP.NET Core.

## Technology Mapping

| Java/Spring Boot | .NET/ASP.NET Core |
|------------------|-------------------|
| Java 17 | .NET 8 |
| Spring Boot 3.x | ASP.NET Core |
| Maven | NuGet / dotnet CLI |
| AWS SDK for Java v2 | AWSSDK.S3 |

## Annotation Mappings

### Controller Annotations

| Java | .NET | Notes |
|------|------|-------|
| `@RestController` | `[ApiController]` | Applied at class level |
| `@RequestMapping("/api/products")` | `[Route("api/products")]` | Base route |
| `@GetMapping` | `[HttpGet]` | GET endpoint |
| `@PostMapping` | `[HttpPost]` | POST endpoint |
| `@PutMapping("/{id}")` | `[HttpPut("{id}")]` | PUT with path variable |
| `@DeleteMapping("/{id}")` | `[HttpDelete("{id}")]` | DELETE with path variable |
| `@PathVariable` | Method parameter | Auto-bound in .NET |
| `@RequestBody` | `[FromBody]` | Request body binding |
| `@RequestParam("file")` | `IFormFile file` | File upload |

### Service Annotations

| Java | .NET | Notes |
|------|------|-------|
| `@Service` | Register in DI container | `builder.Services.AddSingleton<>()` |
| `@Autowired` | Constructor injection | Same pattern, no attribute needed |
| `@Value("${...}")` | `IConfiguration` | Inject configuration |
| `@PostConstruct` | Constructor | Initialize in constructor |

## Dependency Injection

### Java (Spring Boot)

```java
@Service
public class ProductService {
    private final S3StorageService s3StorageService;

    @Autowired
    public ProductService(S3StorageService s3StorageService) {
        this.s3StorageService = s3StorageService;
    }
}
```

### .NET (ASP.NET Core)

```csharp
// Program.cs - Register services
builder.Services.AddSingleton<IS3StorageService, S3StorageService>();
builder.Services.AddSingleton<IProductService, ProductService>();

// ProductService.cs - Constructor injection
public class ProductService : IProductService
{
    private readonly IS3StorageService _s3StorageService;

    public ProductService(IS3StorageService s3StorageService)
    {
        _s3StorageService = s3StorageService;
    }
}
```

**Key Difference:** .NET requires explicit interface definitions and manual registration in the DI container. Spring Boot auto-discovers `@Service` classes.

## Configuration

### Java (application.properties)

```properties
aws.s3.bucket-name=${S3_BUCKET_NAME:product-catalog-images}
aws.access-key-id=${AWS_ACCESS_KEY_ID:test}
```

### .NET (appsettings.json)

```json
{
  "AWS": {
    "S3": {
      "BucketName": "product-catalog-images"
    },
    "AccessKeyId": "test"
  }
}
```

**Key Difference:** Spring Boot uses flat key-value pairs with dot notation. ASP.NET Core uses hierarchical JSON accessed via `configuration["AWS:S3:BucketName"]`.

## AWS SDK Integration

### Java (AWS SDK v2)

```java
import software.amazon.awssdk.services.s3.S3Client;
import software.amazon.awssdk.services.s3.model.*;

S3Client s3Client = S3Client.builder()
    .region(Region.of(region))
    .credentialsProvider(credentialsProvider)
    .build();

PutObjectRequest request = PutObjectRequest.builder()
    .bucket(bucketName)
    .key(key)
    .build();

s3Client.putObject(request, RequestBody.fromInputStream(inputStream, length));
```

### .NET (AWSSDK.S3)

```csharp
using Amazon.S3;
using Amazon.S3.Model;

var s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, config);

var request = new PutObjectRequest
{
    BucketName = bucketName,
    Key = key,
    InputStream = inputStream
};

await s3Client.PutObjectAsync(request);
```

**Key Differences:**
- Java SDK v2 uses builder pattern extensively
- .NET SDK uses object initializers
- .NET naturally supports async/await; Java uses synchronous calls (or CompletableFuture)

## Response Handling

### Java

```java
@GetMapping("/{id}")
public ResponseEntity<Product> getProductById(@PathVariable String id) {
    return productService.getProductById(id)
            .map(ResponseEntity::ok)
            .orElse(ResponseEntity.notFound().build());
}
```

### .NET

```csharp
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
```

**Key Difference:** Java uses `Optional<T>` and functional style. .NET typically uses null checks (though nullable reference types help).

## Async Patterns

### Java

Spring Boot typically uses synchronous request handling:

```java
public byte[] downloadFile(String key) {
    return s3Client.getObjectAsBytes(getRequest).asByteArray();
}
```

### .NET

ASP.NET Core embraces async throughout:

```csharp
public async Task<byte[]> DownloadFileAsync(string key)
{
    using var response = await _s3Client.GetObjectAsync(getRequest);
    using var memoryStream = new MemoryStream();
    await response.ResponseStream.CopyToAsync(memoryStream);
    return memoryStream.ToArray();
}
```

**Recommendation:** Always use async/await in .NET for I/O operations.

## Model Classes

### Java

```java
public class Product {
    private String id;
    private String name;
    private BigDecimal price;

    // Getters and setters required
    public String getId() { return id; }
    public void setId(String id) { this.id = id; }
}
```

### .NET

```csharp
public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

**Key Differences:**
- C# has built-in property syntax
- C# 12 supports primary constructors for even more concise models
- .NET uses `decimal` for money (equivalent to Java's `BigDecimal`)

## Startup Configuration

### Java (Main Application)

```java
@SpringBootApplication
public class ProductCatalogApplication {
    public static void main(String[] args) {
        SpringApplication.run(ProductCatalogApplication.class, args);
    }
}
```

### .NET (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IS3StorageService, S3StorageService>();
builder.Services.AddSingleton<IProductService, ProductService>();

var app = builder.Build();

app.MapControllers();
app.Run();
```

**Key Difference:** Spring Boot relies on convention and annotations for auto-configuration. ASP.NET Core uses explicit configuration in Program.cs.

## Collection Types

| Java | .NET |
|------|------|
| `List<T>` | `List<T>` |
| `Map<K,V>` | `Dictionary<K,V>` |
| `ConcurrentHashMap<K,V>` | `ConcurrentDictionary<K,V>` |
| `Optional<T>` | Nullable `T?` |
| `ArrayList<T>` | `List<T>` |

## Exception Handling

Both frameworks support controller-level exception handling:

### Java

```java
@ExceptionHandler(NotFoundException.class)
public ResponseEntity<String> handleNotFound(NotFoundException e) {
    return ResponseEntity.notFound().build();
}
```

### .NET

```csharp
// Using middleware or exception filters
app.UseExceptionHandler("/error");
```

## Migration Checklist

- [x] Map Spring Boot annotations to ASP.NET Core attributes
- [x] Convert Maven dependencies to NuGet packages
- [x] Implement explicit DI registration
- [x] Convert properties files to appsettings.json
- [x] Update AWS SDK calls to .NET equivalents
- [x] Add async/await for I/O operations
- [x] Convert Java getters/setters to C# properties
- [x] Update collection types
- [x] Test all endpoints for parity

## Common Pitfalls

1. **Forgetting to register services** - Unlike Spring's component scanning, ASP.NET Core requires explicit registration
2. **Blocking async calls** - Avoid `.Result` or `.Wait()` on tasks; use `await`
3. **Configuration paths** - Use `:` instead of `.` for nested config in .NET
4. **Nullable handling** - Enable nullable reference types to catch null issues at compile time
