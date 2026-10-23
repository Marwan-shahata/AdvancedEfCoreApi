using System.ComponentModel.DataAnnotations;

namespace AdvancedEFCoreAPI.Models;

public class Product
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 999999999.99)]
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
