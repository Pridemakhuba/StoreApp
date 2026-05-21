// ============================================================
// Program.cs
// ASP.NET Core 6+ minimal hosting model.
// Configures EF Core with SQL Server and registers MVC.
// ============================================================

using Microsoft.EntityFrameworkCore;
using StoreApp.Models;

var builder = WebApplication.CreateBuilder(args);

// ── Register MVC with Views ──────────────────────────────────
builder.Services.AddControllersWithViews();

// ── Register EF Core DbContext with SQL Server ───────────────
// Connection string is read from appsettings.json
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("StoreDB")
    )
);

var app = builder.Build();

// ── Middleware pipeline ──────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();   // Serve wwwroot files (CSS, JS)
app.UseRouting();
app.UseAuthorization();

// Default route: /Home/Index
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
