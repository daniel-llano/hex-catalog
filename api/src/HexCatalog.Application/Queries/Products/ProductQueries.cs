using HexCatalog.Application.Commands.Products;
using HexCatalog.Application.DTOs;
using HexCatalog.Domain.Entities;
using HexCatalog.Domain.Exceptions;
using HexCatalog.Domain.Ports;
using MediatR;

namespace HexCatalog.Application.Queries.Products;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdHandler(IProductRepository repo) : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException(nameof(Product), query.Id);
        return product.ToDto();
    }
}

public record GetProductsQuery(string? Q = null, int Page = 1, int PageSize = 20) : IRequest<PagedResult<ProductDto>>;

public class GetProductsHandler(IProductRepository repo) : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public const int MaxPageSize = 100;

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery query, CancellationToken ct)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var (items, total) = await repo.SearchAsync(query.Q, page, pageSize, ct);
        return new PagedResult<ProductDto>(items.Select(p => p.ToDto()).ToList(), total, page, pageSize);
    }
}

public record GetCategoriesQuery : IRequest<IReadOnlyList<string>>;

public class GetCategoriesHandler(IProductRepository repo) : IRequestHandler<GetCategoriesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetCategoriesQuery query, CancellationToken ct) => repo.GetCategoriesAsync(ct);
}
