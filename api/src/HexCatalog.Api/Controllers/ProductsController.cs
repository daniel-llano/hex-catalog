using HexCatalog.Application.Commands.Products;
using HexCatalog.Application.DTOs;
using HexCatalog.Application.Queries.Products;
using HexCatalog.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexCatalog.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
[Authorize(Roles = $"{Roles.Admin},{Roles.User}")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    /// <summary>Search products by name or category, one page at a time.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ProductDto>>(StatusCodes.Status200OK)]
    public Task<PagedResult<ProductDto>> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => mediator.Send(new GetProductsQuery(q, page, pageSize), ct);

    /// <summary>Distinct categories currently in use.</summary>
    [HttpGet("categories")]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyList<string>> Categories(CancellationToken ct)
        => mediator.Send(new GetCategoriesQuery(), ct);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ProductDto> GetById(Guid id, CancellationToken ct)
        => mediator.Send(new GetProductByIdQuery(id), ct);

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateProductRequest request, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateProductCommand(request.Name, request.Description, request.Price, request.Category, request.Stock), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<ProductDto> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
        => mediator.Send(new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.Category, request.Stock), ct);

    /// <summary>Soft-delete a product. It disappears from search and lookups.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id), ct);
        return NoContent();
    }

    /// <summary>Take units out of stock. Concurrent reservations can't oversell.</summary>
    [HttpPost("{id:guid}/reserve")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public Task<ProductDto> Reserve(Guid id, ReserveStockRequest request, CancellationToken ct)
        => mediator.Send(new ReserveStockCommand(id, request.Quantity), ct);
}
