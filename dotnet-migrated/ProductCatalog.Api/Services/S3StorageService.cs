using Amazon.S3;
using Amazon.S3.Model;

namespace ProductCatalog.Api.Services;

public interface IS3StorageService
{
    Task<string> UploadFileAsync(string key, Stream inputStream, string contentType);
    Task<byte[]> DownloadFileAsync(string key);
    Task DeleteFileAsync(string key);
    string GetFileUrl(string key);
}

public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _region;
    private readonly string? _endpoint;

    public S3StorageService(IConfiguration configuration)
    {
        _bucketName = configuration["AWS:S3:BucketName"] ?? "product-catalog-images";
        _region = configuration["AWS:Region"] ?? "us-east-1";
        _endpoint = configuration["AWS:S3:Endpoint"];

        var accessKeyId = configuration["AWS:AccessKeyId"] ?? "test";
        var secretAccessKey = configuration["AWS:SecretAccessKey"] ?? "test";

        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(_region)
        };

        // Support LocalStack for local development
        if (!string.IsNullOrEmpty(_endpoint))
        {
            config.ServiceURL = _endpoint;
            config.ForcePathStyle = true;
        }

        _s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, config);
    }

    public async Task<string> UploadFileAsync(string key, Stream inputStream, string contentType)
    {
        var putRequest = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = inputStream,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(putRequest);

        return GetFileUrl(key);
    }

    public async Task<byte[]> DownloadFileAsync(string key)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(getRequest);
        using var memoryStream = new MemoryStream();
        await response.ResponseStream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task DeleteFileAsync(string key)
    {
        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(deleteRequest);
    }

    public string GetFileUrl(string key)
    {
        if (!string.IsNullOrEmpty(_endpoint))
        {
            return $"{_endpoint}/{_bucketName}/{key}";
        }
        return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{key}";
    }
}
