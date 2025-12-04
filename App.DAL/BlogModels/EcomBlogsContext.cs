using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace App.DAL.BlogModels;

public partial class EcomBlogsContext : DbContext
{
    public EcomBlogsContext()
    {
    }

    public EcomBlogsContext(DbContextOptions<EcomBlogsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogCategory> BlogCategories { get; set; }

    public virtual DbSet<BlogCategoryJoin> BlogCategoryJoins { get; set; }

    public virtual DbSet<BlogComment> BlogComments { get; set; }

    public virtual DbSet<BlogTag> BlogTags { get; set; }

    public virtual DbSet<BlogTagJoin> BlogTagJoins { get; set; }

    public virtual DbSet<BlogVariant> BlogVariants { get; set; }

    public virtual DbSet<Quote> Quotes { get; set; }

    // OnConfiguring đã được xóa - connection string được config trong DatabaseConfig.cs
    // Khi chạy trong Docker, environment variables từ docker-compose.dev.yml sẽ override
    // Local dev: appsettings.Development.json (Server=localhost)
    // Docker: Environment variables (Server=mysql) sẽ override

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blogs");

            entity.HasIndex(e => e.QuoteId, "fk_quote_id_blogs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Author)
                .HasMaxLength(255)
                .HasColumnName("author");
            entity.Property(e => e.CommentCount).HasColumnName("comment_count");
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
            entity.Property(e => e.QuoteId).HasColumnName("quote_id");
            entity.Property(e => e.Slug)
                .HasMaxLength(150)
                .HasColumnName("slug");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Quote).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.QuoteId)
                .HasConstraintName("fk_quote_id_blogs");
        });

        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_categories");

            entity.HasIndex(e => e.BlogId, "fk_blog_id_blogs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogCategories)
                .HasForeignKey(d => d.BlogId)
                .HasConstraintName("fk_blog_id_blogs");
        });

        modelBuilder.Entity<BlogCategoryJoin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_category_joins");

            entity.HasIndex(e => e.BlogCategoryId, "fk_blog_category_id_blog_category_joins");

            entity.HasIndex(e => e.BlogId, "fk_blog_id_blog_category_joins");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlogCategoryId).HasColumnName("blog_category_id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.BlogCategory).WithMany(p => p.BlogCategoryJoins)
                .HasForeignKey(d => d.BlogCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_blog_category_id_blog_category_joins");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogCategoryJoins)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_blog_id_blog_category_joins");
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_comments");

            entity.HasIndex(e => e.BlogId, "fk_blog_id_blog_comments");

            entity.HasIndex(e => e.Parent, "fk_parent_blog_comments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
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
            entity.Property(e => e.Parent).HasColumnName("parent");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.BlogId)
                .HasConstraintName("fk_blog_id_blog_comments");

            entity.HasOne(d => d.ParentNavigation).WithMany(p => p.InverseParentNavigation)
                .HasForeignKey(d => d.Parent)
                .HasConstraintName("fk_parent_blog_comments");
        });

        modelBuilder.Entity<BlogTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_tags");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<BlogTagJoin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_tag_joins");

            entity.HasIndex(e => e.BlogId, "fk_blog_id_blog_tag_joins");

            entity.HasIndex(e => e.BlogTagId, "fk_blog_tag_id_blog_tag_joins");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.BlogTagId).HasColumnName("blog_tag_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogTagJoins)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_blog_id_blog_tag_joins");

            entity.HasOne(d => d.BlogTag).WithMany(p => p.BlogTagJoins)
                .HasForeignKey(d => d.BlogTagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_blog_tag_id_blog_tag_joins");
        });

        modelBuilder.Entity<BlogVariant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blog_variants");

            entity.HasIndex(e => e.BlogId, "blog_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlogId).HasColumnName("blog_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Type)
                .HasColumnType("enum('image','gallery','video','youtube','soundcloud','audio')")
                .HasColumnName("type");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Url)
                .HasColumnType("text")
                .HasColumnName("url");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogVariants)
                .HasForeignKey(d => d.BlogId)
                .HasConstraintName("blog_variants_ibfk_1");
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("quotes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Author)
                .HasMaxLength(255)
                .HasColumnName("author");
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
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
