// ============================================================
// Models.cs  –  All entity model classes for StoreDB
// Each class maps to a table in the database.
// Data annotations provide validation and display hints.
// ============================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreApp.Models
{
    // ==========================================================
    // Category – maps to dbo.Categories
    // ==========================================================
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation: one Category has many Products
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    // ==========================================================
    // Product – maps to dbo.Products
    // ==========================================================
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryID { get; set; }  // FK to Categories

        [Required, MaxLength(150)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be positive.")]
        public decimal Price { get; set; }

        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int StockQty { get; set; } = 0;

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    // ==========================================================
    // Customer – maps to dbo.Customers
    // ==========================================================
    public class Customer
    {
        [Key]
        public int CustomerID { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required, MaxLength(200)]
        [EmailAddress]
        public string Email { get; set; }

        [Phone, MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(300)]
        public string Address { get; set; }

        [Display(Name = "Member Since")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Computed display property – not mapped to DB column
        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        // Navigation: one Customer has many Orders
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    // ==========================================================
    // Order – maps to dbo.Orders
    // ==========================================================
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        [Display(Name = "Customer")]
        public int CustomerID { get; set; }  // FK to Customers

        [Display(Name = "Order Date")]
        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; } = 0;

        [MaxLength(500)]
        public string Notes { get; set; }

        // Navigation properties
        [ForeignKey("CustomerID")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    // ==========================================================
    // OrderItem – maps to dbo.OrderItems
    // ==========================================================
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        [Required]
        [Display(Name = "Order")]
        public int OrderID { get; set; }    // FK to Orders

        [Required]
        [Display(Name = "Product")]
        public int ProductID { get; set; }  // FK to Products

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        // Computed by SQL Server — read only in EF
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public decimal Subtotal { get; set; }

        // Navigation properties
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}
