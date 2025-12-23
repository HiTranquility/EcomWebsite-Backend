using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace App.DAL.OrderModels;

public partial class EcomOrdersContext : DbContext
{
    public EcomOrdersContext()
    {
    }

    public EcomOrdersContext(DbContextOptions<EcomOrdersContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDelivery> OrderDeliveries { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<PaymentsLog> PaymentsLogs { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    // OnConfiguring đã được xóa - connection string được config trong DatabaseConfig.cs

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("carts");

            entity.HasIndex(e => e.UserId, "idx_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'active'")
                .HasColumnType("enum('active','expired','checked_out')")
                .HasColumnName("status");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("total_price");
            entity.Property(e => e.TotalQuantity)
                .HasDefaultValueSql("'0'")
                .HasColumnName("total_quantity");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("cart_items");

            entity.HasIndex(e => e.CartId, "cart_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PriceAtTime)
                .HasPrecision(10, 2)
                .HasColumnName("price_at_time");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValueSql("'1'")
                .HasColumnName("quantity");
            entity.Property(e => e.Subtotal)
                .HasPrecision(10, 2)
                .HasColumnName("subtotal");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("cart_items_ibfk_1");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.CartId, "cart_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CartId).HasColumnName("cart_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("discount_amount");
            entity.Property(e => e.FinalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("final_price");
            entity.Property(e => e.PaymentStatus)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('pending','paid','failed','refunded')")
                .HasColumnName("payment_status");
            entity.Property(e => e.ShippingFee)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasColumnName("shipping_fee");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('pending','paid','shipping','cancelled','refunded')")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Cart).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("orders_ibfk_1");
        });

        modelBuilder.Entity<OrderDelivery>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("order_deliveries");

            entity.HasIndex(e => e.OrderId, "order_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeliveredAt)
                .HasColumnType("datetime")
                .HasColumnName("delivered_at");
            entity.Property(e => e.DeliveryStatus)
                .HasDefaultValueSql("'waiting_pickup'")
                .HasColumnType("enum('waiting_pickup','delivering','delivered','failed')")
                .HasColumnName("delivery_status");
            entity.Property(e => e.DeliveryType)
                .HasDefaultValueSql("'standard'")
                .HasColumnType("enum('standard','fast')")
                .HasColumnName("delivery_type");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ShippedAt)
                .HasColumnType("datetime")
                .HasColumnName("shipped_at");
            entity.Property(e => e.ShippingProvider)
                .HasMaxLength(100)
                .HasColumnName("shipping_provider");
            entity.Property(e => e.TrackingCode)
                .HasMaxLength(255)
                .HasColumnName("tracking_code");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDeliveries)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_deliveries_ibfk_1");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("order_items");

            entity.HasIndex(e => e.OrderId, "order_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PriceAtTime)
                .HasPrecision(10, 2)
                .HasColumnName("price_at_time");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VariantId).HasColumnName("variant_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_items_ibfk_1");
        });

        modelBuilder.Entity<PaymentsLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("payments_log");

            entity.HasIndex(e => e.TransactionId, "transaction_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PayloadRawJson)
                .HasColumnType("json")
                .HasColumnName("payload_raw_json");
            entity.Property(e => e.SignatureVerified)
                .HasDefaultValueSql("'0'")
                .HasColumnName("signature_verified");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.PaymentsLogs)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("payments_log_ibfk_1");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.OrderId, "order_id");
            entity.HasIndex(e => e.ProviderTransactionId, "idx_provider_transaction_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GatewayTransactionCode)
                .HasMaxLength(255)
                .HasColumnName("gateway_transaction_code");
            entity.Property(e => e.Method)
                .HasColumnType("enum('COD','VNPAY','MOMO','STRIPE')")
                .HasColumnName("method");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'pending'")
                .HasColumnType("enum('pending','success','failed','expired','refunded','partially_refunded')")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            // New fields for extended payment tracking
            entity.Property(e => e.ProviderTransactionId)
                .HasMaxLength(255)
                .HasColumnName("provider_transaction_id");
            entity.Property(e => e.ProviderRefundId)
                .HasMaxLength(255)
                .HasColumnName("provider_refund_id");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND")
                .HasColumnName("currency");
            entity.Property(e => e.FailureReason)
                .HasMaxLength(500)
                .HasColumnName("failure_reason");
            entity.Property(e => e.PayerInfo)
                .HasColumnType("json")
                .HasColumnName("payer_info");
            entity.Property(e => e.RefundAmount)
                .HasPrecision(10, 2)
                .HasColumnName("refund_amount");
            entity.Property(e => e.RefundedAt)
                .HasColumnType("datetime")
                .HasColumnName("refunded_at");

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("transactions_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
