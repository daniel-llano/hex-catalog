namespace HexCatalog.Admin.Domain.Entities;

public record Page<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);
}
