namespace HexCatalog.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    int Stock,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string Category,
    int Stock
);

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    string Category,
    int Stock
);

public record ReserveStockRequest(int Quantity);
