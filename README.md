# Java-to-.NET AWS Migration Demo

A demonstration project showcasing the migration of a Java Spring Boot application to .NET 8, with AWS S3 integration for image storage.

## Project Overview

This project contains two implementations of the same Product Catalog REST API:

- **java-original/** - Original Java 17 / Spring Boot 3.x implementation
- **dotnet-migrated/** - Migrated .NET 8 / ASP.NET Core implementation

Both applications provide identical REST API endpoints and functionality, demonstrating practical enterprise migration patterns.

## Features

- CRUD operations for products (GET, POST, PUT, DELETE)
- Image upload/download via AWS S3
- In-memory data storage
- LocalStack support for local development (no AWS account required)

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | List all products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create new product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |
| POST | `/api/products/{id}/image` | Upload product image |
| GET | `/api/products/{id}/image` | Download product image |

## Running the Applications

### Prerequisites

- Java 17+ and Maven (for Java app)
- .NET 8 SDK (for .NET app)
- Docker (optional, for LocalStack)

### Start LocalStack (Optional)

For local S3 testing without an AWS account:

```bash
docker run -d -p 4566:4566 localstack/localstack

# Create the bucket
aws --endpoint-url=http://localhost:4566 s3 mb s3://product-catalog-images
```

### Java Application

```bash
cd java-original
mvn clean compile
mvn spring-boot:run
```

The API will be available at `http://localhost:8080`

### .NET Application

```bash
cd dotnet-migrated
dotnet restore
dotnet build
dotnet run --project ProductCatalog.Api
```

The API will be available at `http://localhost:5000` (or as shown in console output)

## Testing the API

### Create a Product

```bash
curl -X POST http://localhost:8080/api/products \
  -H "Content-Type: application/json" \
  -d '{"name": "Widget", "description": "A useful widget", "price": 29.99}'
```

### List All Products

```bash
curl http://localhost:8080/api/products
```

### Upload Product Image

```bash
curl -X POST http://localhost:8080/api/products/{id}/image \
  -F "file=@/path/to/image.jpg"
```

## Configuration

### Java (application.properties)

```properties
aws.access-key-id=${AWS_ACCESS_KEY_ID:test}
aws.secret-access-key=${AWS_SECRET_ACCESS_KEY:test}
aws.s3.region=${AWS_REGION:us-east-1}
aws.s3.bucket-name=${S3_BUCKET_NAME:product-catalog-images}
aws.s3.endpoint=${AWS_S3_ENDPOINT:http://localhost:4566}
```

### .NET (appsettings.json)

```json
{
  "AWS": {
    "AccessKeyId": "test",
    "SecretAccessKey": "test",
    "Region": "us-east-1",
    "S3": {
      "BucketName": "product-catalog-images",
      "Endpoint": "http://localhost:4566"
    }
  }
}
```

## Project Structure

```
java-to-dotnet-aws-demo/
├── java-original/
│   ├── src/main/java/com/demo/productcatalog/
│   │   ├── ProductCatalogApplication.java
│   │   ├── controller/ProductController.java
│   │   ├── model/Product.java
│   │   ├── service/ProductService.java
│   │   └── service/S3StorageService.java
│   ├── src/main/resources/application.properties
│   └── pom.xml
├── dotnet-migrated/
│   ├── ProductCatalog.Api/
│   │   ├── Controllers/ProductController.cs
│   │   ├── Models/Product.cs
│   │   ├── Services/ProductService.cs
│   │   ├── Services/S3StorageService.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   └── ProductCatalog.sln
└── docs/
    └── migration-guide.md
```

## Documentation

See [docs/migration-guide.md](docs/migration-guide.md) for detailed migration patterns and decisions.
