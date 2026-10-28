using System.Net;

namespace HexCatalog.Admin.Application.Abstractions;

/// <summary>A non-success response from the catalog API, with the problem details it sent.</summary>
public class ApiException(HttpStatusCode status, string message, IReadOnlyDictionary<string, string[]>? errors = null)
    : Exception(message)
{
    public HttpStatusCode Status { get; } = status;
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors ?? new Dictionary<string, string[]>();

    public bool IsUnauthorized => Status == HttpStatusCode.Unauthorized;

    /// <summary>The detail plus any field errors, ready to show in an alert.</summary>
    public string Describe() =>
        Errors.Count == 0 ? Message : $"{Message} {string.Join(" ", Errors.SelectMany(e => e.Value))}";
}
