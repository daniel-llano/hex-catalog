using FluentAssertions;
using HexCatalog.Domain.Entities;

namespace HexCatalog.UnitTests.Domain.Entities;

public class ProductTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var product = Product.Create("Laptop", "A fast laptop", 999.99m, "Electronics", 10);

        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be("Laptop");
        product.Description.Should().Be("A fast laptop");
        product.Price.Should().Be(999.99m);
        product.Category.Should().Be("Electronics");
        product.Stock.Should().Be(10);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        product.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Update_ShouldModifyMutableFields_AndSetUpdatedAt()
    {
        var product = Product.Create("Old Name", "Old desc", 100m, "OldCat", 5);

        product.Update("New Name", "New desc", 200m, "NewCat", 15);

        product.Name.Should().Be("New Name");
        product.Description.Should().Be("New desc");
        product.Price.Should().Be(200m);
        product.Category.Should().Be("NewCat");
        product.Stock.Should().Be(15);
        product.UpdatedAt.Should().NotBeNull();
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldGenerateUniqueId_ForEachProduct()
    {
        var p1 = Product.Create("A", "d", 1m, "c", 1);
        var p2 = Product.Create("B", "d", 2m, "c", 2);

        p1.Id.Should().NotBe(p2.Id);
    }

    [Fact]
    public void TryReserve_DecrementsStock_WhenEnough()
    {
        var product = Product.Create("A", "d", 1m, "c", 5);
        product.TryReserve(2).Should().BeTrue();
        product.Stock.Should().Be(3);
        product.TryReserve(10).Should().BeFalse();
        product.Stock.Should().Be(3);
    }

    [Fact]
    public void SoftDelete_HidesProductFromActiveUse()
    {
        var product = Product.Create("A", "d", 1m, "c", 5);
        product.SoftDelete();
        product.IsDeleted.Should().BeTrue();
        product.TryReserve(1).Should().BeFalse();
    }
}
