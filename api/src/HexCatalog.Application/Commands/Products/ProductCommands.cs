using HexCatalog.Application.DTOs;
using HexCatalog.Domain.Entities;
using HexCatalog.Domain.Exceptions;
using HexCatalog.Domain.Ports;
using MediatR;

namespace HexCatalog.Application.Commands.Products;

public record CreateProductCommand(string Name, string Description, decimal Price, string Category, int Stock)
    : IRequest<ProductDto>;

public class CreateProductHandler(IProductRepository repo, IUnitOfWork uow) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = Product.Create(cmd.Name.Trim(), cmd.Description.Trim(), cmd.Price, cmd.Category.Trim(), cmd.Stock);
        await repo.AddAsync(product, ct);
        await uow.SaveChangesAsync(ct);
        return product.ToDto();
    }
}

public record UpdateProductCommand(Guid Id, string Name, string Description, decimal Price, string Category, int Stock)
    : IRequest<ProductDto>;

public class UpdateProductHandler(IProductRepository repo, IUnitOfWork uow) : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(Product), cmd.Id);

        product.Update(cmd.Name.Trim(), cmd.Description.Trim(), cmd.Price, cmd.Category.Trim(), cmd.Stock);
        await uow.SaveChangesAsync(ct);
        return product.ToDto();
    }
}

public record DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductHandler(IProductRepository repo, IUnitOfWork uow) : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand cmd, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(Product), cmd.Id);

        product.SoftDelete();
        await uow.SaveChangesAsync(ct);
    }
}

public record ReserveStockCommand(Guid Id, int Quantity) : IRequest<ProductDto>;

public class ReserveStockHandler(IProductRepository repo, IUnitOfWork uow) : IRequestHandler<ReserveStockCommand, ProductDto>
{
    public async Task<ProductDto> Handle(ReserveStockCommand cmd, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(Product), cmd.Id);
        if (!product.TryReserve(cmd.Quantity))
            throw new BusinessRuleException($"Only {product.Stock} unit(s) of '{product.Name}' are in stock.");
        await uow.SaveChangesAsync(ct);
        return product.ToDto();
    }
}

internal static class ProductMappings
{
    internal static ProductDto ToDto(this Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Category, p.Stock, p.CreatedAt, p.UpdatedAt);
}
