using System.ComponentModel.DataAnnotations;

namespace HexCatalog.Admin.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999", ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }

    [Required, StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Stock can't be negative.")]
    public int Stock { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
