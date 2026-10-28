namespace HexCatalog.Domain.Ports;

/// <summary>Commits pending writes.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
