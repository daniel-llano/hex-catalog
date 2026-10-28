using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HexCatalog.Admin.Application.Abstractions;

namespace HexCatalog.Admin.Infrastructure.Http;

/// <summary>Sends requests with the signed-in user's token and turns problem responses into ApiException.</summary>
public abstract class ApiClientBase(HttpClient http, IAccessTokenProvider? tokens)
{
    protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, string uri, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, uri);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        if (tokens is not null && await tokens.GetAccessTokenAsync() is { Length: > 0 } token)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            throw await ToExceptionAsync(response, ct);
        return response;
    }

    protected async Task<T> SendAsync<T>(HttpMethod method, string uri, object? body, CancellationToken ct)
    {
        using var response = await SendAsync(method, uri, body, ct);
        return (await response.Content.ReadFromJsonAsync<T>(ct))!;
    }

    private static async Task<ApiException> ToExceptionAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var fallback = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Your session has expired. Sign in again.",
            HttpStatusCode.Forbidden => "You don't have permission to do that.",
            HttpStatusCode.TooManyRequests => "Too many attempts. Wait a minute and try again.",
            _ => $"The catalog API returned {(int)response.StatusCode}."
        };

        try
        {
            var problem = await response.Content.ReadFromJsonAsync<Problem>(ct);
            if (problem is not null)
                return new ApiException(response.StatusCode, problem.Detail ?? problem.Title ?? fallback, problem.Errors);
        }
        catch (JsonException) { }
        catch (NotSupportedException) { }

        return new ApiException(response.StatusCode, fallback);
    }

    private sealed record Problem(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}
