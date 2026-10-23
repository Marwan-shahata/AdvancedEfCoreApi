using System.ComponentModel.DataAnnotations;

namespace AdvancedEFCoreAPI.DTOs;

public sealed class UpdateProductDto
{
    [Required, StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999.99")]
    public decimal Price { get; init; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; init; }

    [Required]
    public string RowVersion { get; init; } = string.Empty;
}
