package com.demo.productcatalog.service;

import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;
import software.amazon.awssdk.auth.credentials.AwsBasicCredentials;
import software.amazon.awssdk.auth.credentials.StaticCredentialsProvider;
import software.amazon.awssdk.core.sync.RequestBody;
import software.amazon.awssdk.regions.Region;
import software.amazon.awssdk.services.s3.S3Client;
import software.amazon.awssdk.services.s3.model.*;

import jakarta.annotation.PostConstruct;
import java.io.InputStream;
import java.net.URI;

@Service
public class S3StorageService {

    @Value("${aws.s3.bucket-name}")
    private String bucketName;

    @Value("${aws.s3.region}")
    private String region;

    @Value("${aws.access-key-id}")
    private String accessKeyId;

    @Value("${aws.secret-access-key}")
    private String secretAccessKey;

    @Value("${aws.s3.endpoint:#{null}}")
    private String endpoint;

    private S3Client s3Client;

    @PostConstruct
    public void init() {
        var credentialsProvider = StaticCredentialsProvider.create(
                AwsBasicCredentials.create(accessKeyId, secretAccessKey)
        );

        var builder = S3Client.builder()
                .region(Region.of(region))
                .credentialsProvider(credentialsProvider);

        // Support LocalStack for local development
        if (endpoint != null && !endpoint.isEmpty()) {
            builder.endpointOverride(URI.create(endpoint))
                   .forcePathStyle(true);
        }

        this.s3Client = builder.build();
    }

    public String uploadFile(String key, InputStream inputStream, long contentLength, String contentType) {
        PutObjectRequest putRequest = PutObjectRequest.builder()
                .bucket(bucketName)
                .key(key)
                .contentType(contentType)
                .build();

        s3Client.putObject(putRequest, RequestBody.fromInputStream(inputStream, contentLength));

        return getFileUrl(key);
    }

    public byte[] downloadFile(String key) {
        GetObjectRequest getRequest = GetObjectRequest.builder()
                .bucket(bucketName)
                .key(key)
                .build();

        return s3Client.getObjectAsBytes(getRequest).asByteArray();
    }

    public void deleteFile(String key) {
        DeleteObjectRequest deleteRequest = DeleteObjectRequest.builder()
                .bucket(bucketName)
                .key(key)
                .build();

        s3Client.deleteObject(deleteRequest);
    }

    public String getFileUrl(String key) {
        if (endpoint != null && !endpoint.isEmpty()) {
            return String.format("%s/%s/%s", endpoint, bucketName, key);
        }
        return String.format("https://%s.s3.%s.amazonaws.com/%s", bucketName, region, key);
    }
}
