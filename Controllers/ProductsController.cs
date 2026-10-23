using AdvancedEFCoreAPI.Data;
using AdvancedEFCoreAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdvancedEFCoreAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ApplicationDbContext db) : ControllerBase
{
    private const int MaxPageSize = 100;

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<ProductResponseDto>>> GetProducts(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest(new { message = $"pageNumber must be at least 1 and pageSize must be between 1 and {MaxPageSize}." });

        var query = db.Products.AsNoTracking().OrderBy(p => p.Id);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id, Name = p.Name, Price = p.Price, CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                RowVersion = Convert.ToBase64String(p.RowVersion)
            }).ToListAsync();

        return Ok(new PagedResultDto<ProductResponseDto>
        {
            PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount, Items = items
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetProduct(int id)
    {
        var product = await db.Products.AsNoTracking().Where(p => p.Id == id)
            .Select(p => new ProductResponseDto
            {
                Id = p.Id, Name = p.Name, Price = p.Price, CategoryId = p.CategoryId,
                CategoryName = p.Category.Name, RowVersion = Convert.ToBase64String(p.RowVersion)
            }).SingleOrDefaultAsync();
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponseDto>> UpdateProduct(int id, UpdateProductDto request)
    {
        byte[] rowVersion;
        try { rowVersion = Convert.FromBase64String(request.RowVersion); }
        catch (FormatException) { return BadRequest(new { message = "RowVersion must be a valid Base64 value." }); }

        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        if (!await db.Categories.AnyAsync(c => c.Id == request.CategoryId))
            return BadRequest(new { message = "The requested CategoryId does not exist." });

        product.Name = request.Name.Trim();
        product.Price = request.Price;
        product.CategoryId = request.CategoryId;
        db.Entry(product).Property(p => p.RowVersion).OriginalValue = rowVersion;

        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new { message = "The product was modified by another user. Please reload the product and try again." });
        }

        var categoryName = await db.Categories.Where(c => c.Id == product.CategoryId).Select(c => c.Name).SingleAsync();
        return Ok(new ProductResponseDto { Id = product.Id, Name = product.Name, Price = product.Price,
            CategoryId = product.CategoryId, CategoryName = categoryName, RowVersion = Convert.ToBase64String(product.RowVersion) });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        product.IsDeleted = true;
        await db.SaveChangesAsync();
        return NoContent();
    }
}
