// ============================================================
// StoreContext.cs
// Entity Framework DbContext – Database First approach.
// Connects to StoreDB and maps all tables to C# model classes.
// Run scaffold command to auto-generate, or use this manually:
//   Scaffold-DbContext "Server=.;Database=StoreDB;Trusted_Connection=True;"
//   Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models
// ============================================================

using Microsoft.EntityFrameworkCore;

namespace StoreApp.Models
{
    /// <summary>
    /// Main EF Core DbContext. Represents a session with StoreDB.
    /// Each DbSet maps to a table in the database.
    /// </summary>
    public class StoreContext : DbContext
    {
        // Constructor receives options (connection string) via DI
        public StoreContext(DbContextOptions<StoreContext> options) : base(options) { }

        // DbSet properties — one per table
        public DbSet<Category>   Categories  { get; set; }
        public DbSet<Product>    Products    { get; set; }
        public DbSet<Customer>   Customers   { get; set; }
        public DbSet<Order>      Orders      { get; set; }
        public DbSet<OrderItem>  OrderItems  { get; set; }

        /// <summary>
        /// Configures the EF model: table names, keys, relationships
        /// and the computed column on OrderItems.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------- Category ----------
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.CategoryID);
                entity.Property(c => c.CategoryName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.IsActive).HasDefaultValue(true);
                entity.Property(c => c.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // ---------- Product ----------
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.ProductID);
                entity.Property(p => p.ProductName).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
                entity.Property(p => p.IsActive).HasDefaultValue(true);
                entity.Property(p => p.CreatedDate).HasDefaultValueSql("GETDATE()");

                // FK: Product → Category (many-to-one)
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- Customer ----------
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers");
                entity.HasKey(c => c.CustomerID);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(c => c.Email).IsUnique();
                entity.Property(c => c.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // ---------- Order ----------
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.OrderID);
                entity.Property(o => o.Status).HasDefaultValue("Pending");
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
                entity.Property(o => o.OrderDate).HasDefaultValueSql("GETDATE()");

                // FK: Order → Customer (many-to-one)
                entity.HasOne(o => o.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(o => o.CustomerID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---------- OrderItem ----------
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("OrderItems");
                entity.HasKey(oi => oi.OrderItemID);
                entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(10,2)");
                // Subtotal is a computed column — mark as read-only
                entity.Property(oi => oi.Subtotal)
                      .HasColumnType("decimal(10,2)")
                      .ValueGeneratedOnAddOrUpdate();

                // FK: OrderItem → Order (many-to-one)
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderID)
                      .OnDelete(DeleteBehavior.Cascade);

                // FK: OrderItem → Product (many-to-one)
                entity.HasOne(oi => oi.Product)
                      .WithMany(p => p.OrderItems)
                      .HasForeignKey(oi => oi.ProductID)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
