namespace AdvancedEFCoreAPI.DTOs;

public sealed class ProductResponseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string RowVersion { get; init; } = string.Empty;
}
