using AdvancedEFCoreAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEFCoreAPI.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (await db.Categories.AnyAsync()) return;

        var electronics = new Category { Name = "Electronics" };
        var groceries = new Category { Name = "Groceries" };
        var books = new Category { Name = "Books" };
        db.Categories.AddRange(electronics, groceries, books);
        db.Products.AddRange(
            new Product { Name = "Laptop", Price = 45000m, Category = electronics },
            new Product { Name = "Wireless Mouse", Price = 750m, Category = electronics },
            new Product { Name = "Coffee", Price = 220m, Category = groceries },
            new Product { Name = "Clean Code", Price = 900m, Category = books },
            new Product { Name = "USB-C Cable", Price = 300m, Category = electronics });
        await db.SaveChangesAsync();
    }
}
