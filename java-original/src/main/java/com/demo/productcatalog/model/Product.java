package com.demo.productcatalog.model;

import java.math.BigDecimal;
import java.util.UUID;

public class Product {

    private String id;
    private String name;
    private String description;
    private BigDecimal price;
    private String imageUrl;

    public Product() {
        this.id = UUID.randomUUID().toString();
    }

    public Product(String name, String description, BigDecimal price) {
        this();
        this.name = name;
        this.description = description;
        this.price = price;
    }

    public String getId() {
        return id;
    }

    public void setId(String id) {
        this.id = id;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public BigDecimal getPrice() {
        return price;
    }

    public void setPrice(BigDecimal price) {
        this.price = price;
    }

    public String getImageUrl() {
        return imageUrl;
    }

    public void setImageUrl(String imageUrl) {
        this.imageUrl = imageUrl;
    }
}
