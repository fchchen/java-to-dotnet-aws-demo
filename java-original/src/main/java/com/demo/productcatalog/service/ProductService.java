package com.demo.productcatalog.service;

import com.demo.productcatalog.model.Product;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.util.*;
import java.util.concurrent.ConcurrentHashMap;

@Service
public class ProductService {

    private final Map<String, Product> products = new ConcurrentHashMap<>();
    private final S3StorageService s3StorageService;

    @Autowired
    public ProductService(S3StorageService s3StorageService) {
        this.s3StorageService = s3StorageService;
    }

    public List<Product> getAllProducts() {
        return new ArrayList<>(products.values());
    }

    public Optional<Product> getProductById(String id) {
        return Optional.ofNullable(products.get(id));
    }

    public Product createProduct(Product product) {
        if (product.getId() == null || product.getId().isEmpty()) {
            product.setId(UUID.randomUUID().toString());
        }
        products.put(product.getId(), product);
        return product;
    }

    public Optional<Product> updateProduct(String id, Product updatedProduct) {
        if (!products.containsKey(id)) {
            return Optional.empty();
        }
        updatedProduct.setId(id);
        // Preserve image URL if not provided in update
        if (updatedProduct.getImageUrl() == null) {
            Product existing = products.get(id);
            updatedProduct.setImageUrl(existing.getImageUrl());
        }
        products.put(id, updatedProduct);
        return Optional.of(updatedProduct);
    }

    public boolean deleteProduct(String id) {
        Product removed = products.remove(id);
        if (removed != null && removed.getImageUrl() != null) {
            // Extract key from URL and delete from S3
            String imageKey = "products/" + id + "/image";
            try {
                s3StorageService.deleteFile(imageKey);
            } catch (Exception e) {
                // Log but don't fail deletion
            }
        }
        return removed != null;
    }

    public Optional<Product> uploadProductImage(String id, MultipartFile file) throws IOException {
        Optional<Product> productOpt = getProductById(id);
        if (productOpt.isEmpty()) {
            return Optional.empty();
        }

        Product product = productOpt.get();
        String imageKey = "products/" + id + "/image";

        String imageUrl = s3StorageService.uploadFile(
                imageKey,
                file.getInputStream(),
                file.getSize(),
                file.getContentType()
        );

        product.setImageUrl(imageUrl);
        products.put(id, product);

        return Optional.of(product);
    }

    public byte[] getProductImage(String id) {
        String imageKey = "products/" + id + "/image";
        return s3StorageService.downloadFile(imageKey);
    }
}
