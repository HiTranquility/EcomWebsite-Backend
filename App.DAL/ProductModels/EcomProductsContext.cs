using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace App.DAL.ProductModels;

public partial class EcomProductsContext : DbContext
{
    public EcomProductsContext()
    {
    }

    public EcomProductsContext(DbContextOptions<EcomProductsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Manufacturer> Manufacturers { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductReview> ProductReviews { get; set; }

    // OnConfiguring đã được xóa - connection string được config trong DatabaseConfig.cs
    // Khi chạy trong Docker, environment variables từ docker-compose.dev.yml sẽ override
    // Local dev: appsettings.Development.json (Server=localhost)
    // Docker: Environment variables (Server=mysql) sẽ override

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Manufacturer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("manufacturers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("products");

            entity.HasIndex(e => e.ManufacturerId, "fk_manufacturer_id_products");

            entity.HasIndex(e => e.ProductCategoryId, "fk_product_category_id_products");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.EmbeddedUrl)
                .HasMaxLength(255)
                .HasColumnName("embedded_url");
            entity.Property(e => e.Information)
                .HasColumnType("json")
                .HasColumnName("information");
            entity.Property(e => e.IsDeal)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_deal");
            entity.Property(e => e.IsFeature)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_feature");
            entity.Property(e => e.IsFlashsale)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_flashsale");
            entity.Property(e => e.IsFreeShipping)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_free_shipping");
            entity.Property(e => e.IsSpecial)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_special");
            entity.Property(e => e.IsToday)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_today");
            entity.Property(e => e.IsWeekly)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_weekly");
            entity.Property(e => e.LastestPrice)
                .HasPrecision(10, 2)
                .HasColumnName("lastest_price");
            entity.Property(e => e.LongDescription)
                .HasColumnType("text")
                .HasColumnName("long_description");
            entity.Property(e => e.MainImageUrl)
                .HasMaxLength(255)
                .HasColumnName("main_image_url");
            entity.Property(e => e.ManufacturerId).HasColumnName("manufacturer_id");
            entity.Property(e => e.OriginalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("original_price");
            entity.Property(e => e.ProductCategoryId).HasColumnName("product_category_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ReviewCount).HasColumnName("review_count");
            entity.Property(e => e.ShortDescription)
                .HasColumnType("text")
                .HasColumnName("short_description");
            entity.Property(e => e.Size)
                .HasMaxLength(255)
                .HasColumnName("size");
            entity.Property(e => e.Slug)
                .HasMaxLength(150)
                .HasColumnName("slug");
            entity.Property(e => e.StockLeft).HasColumnName("stock_left");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.TotalStarRating)
                .HasPrecision(3, 2)
                .HasColumnName("total_star_rating");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Manufacturer).WithMany(p => p.Products)
                .HasForeignKey(d => d.ManufacturerId)
                .HasConstraintName("fk_manufacturer_id_products");

            entity.HasOne(d => d.ProductCategory).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductCategoryId)
                .HasConstraintName("fk_product_category_id_products");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product_categories");

            entity.HasIndex(e => e.Parent, "fk_parent_product_categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Parent).HasColumnName("parent");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ParentNavigation).WithMany(p => p.InverseParentNavigation)
                .HasForeignKey(d => d.Parent)
                .HasConstraintName("fk_parent_product_categories");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product_images");

            entity.HasIndex(e => e.ProductId, "fk_product_id_product_images");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("image_url");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_id_product_images");
        });

        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("product_reviews");

            entity.HasIndex(e => e.ProductId, "fk_product_id_product_reviews");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.StarRating).HasColumnName("star_rating");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductReviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_product_id_product_reviews");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
