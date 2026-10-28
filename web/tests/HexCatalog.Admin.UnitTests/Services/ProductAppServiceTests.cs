using FluentAssertions;
using HexCatalog.Admin.Application.Abstractions;
using HexCatalog.Admin.Application.Contracts;
using HexCatalog.Admin.Application.Services;
using HexCatalog.Admin.Domain.Entities;
using Moq;

namespace HexCatalog.Admin.UnitTests.Services;

public class ProductAppServiceTests
{
    private readonly Mock<IProductApiClient> _client = new();
    private ProductAppService Sut => new(_client.Object);

    private static ProductDto Dto(string name = "Laptop", int stock = 10) =>
        new(Guid.NewGuid(), name, "Desc", 999.99m, "Electronics", stock, DateTime.UtcNow, null);

    [Fact]
    public async Task SearchAsync_MapsPageFromClient()
    {
        var dto = Dto();
        _client.Setup(c => c.SearchAsync("lap", 2, 20, default))
            .ReturnsAsync(new PagedResult<ProductDto>([dto], 21, 2, 20));

        var page = await Sut.SearchAsync("lap", 2, 20);

        page.Total.Should().Be(21);
        page.TotalPages.Should().Be(2);
        page.Items.Should().ContainSingle().Which.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenMissing()
    {
        _client.Setup(c => c.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((ProductDto?)null);

        (await Sut.GetByIdAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_SendsFieldsAndMapsResponse()
    {
        ProductRequest? sent = null;
        _client.Setup(c => c.CreateAsync(It.IsAny<ProductRequest>(), default))
            .Callback<ProductRequest, CancellationToken>((r, _) => sent = r)
            .ReturnsAsync(Dto("Phone", 5));

        var product = await Sut.CreateAsync(new Product { Name = "Phone", Description = "Mobile", Price = 499m, Category = "Electronics", Stock = 5 });

        sent.Should().Be(new ProductRequest("Phone", "Mobile", 499m, "Electronics", 5));
        product.Name.Should().Be("Phone");
    }

    [Fact]
    public async Task ReserveAsync_ReturnsUpdatedStock()
    {
        var id = Guid.NewGuid();
        _client.Setup(c => c.ReserveAsync(id, 3, default)).ReturnsAsync(Dto(stock: 7) with { Id = id });

        var product = await Sut.ReserveAsync(id, 3);

        product.Stock.Should().Be(7);
    }

    [Fact]
    public void ApiException_DescribeIncludesFieldErrors()
    {
        var ex = new ApiException(System.Net.HttpStatusCode.BadRequest, "One or more fields are invalid.",
            new Dictionary<string, string[]> { ["price"] = ["Price must be greater than 0."] });

        ex.Describe().Should().Be("One or more fields are invalid. Price must be greater than 0.");
    }
}
