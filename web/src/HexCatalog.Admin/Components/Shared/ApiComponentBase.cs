using HexCatalog.Admin.Application.Abstractions;
using Microsoft.AspNetCore.Components;

namespace HexCatalog.Admin.Components.Shared;

/// <summary>Runs API calls for a page and turns failures into a message, or a sign-in redirect.</summary>
public abstract class ApiComponentBase : ComponentBase
{
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] private ILogger<ApiComponentBase> Logger { get; set; } = default!;

    protected string? Error { get; set; }
    protected bool Busy { get; private set; }

    protected async Task<bool> RunAsync(Func<Task> action)
    {
        Error = null;
        Busy = true;
        try
        {
            await action();
            return true;
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            var returnUrl = Uri.EscapeDataString(Nav.ToBaseRelativePath(Nav.Uri));
            Nav.NavigateTo($"account/login?returnUrl={returnUrl}", forceLoad: true);
            return false;
        }
        catch (ApiException ex)
        {
            Error = ex.Describe();
            return false;
        }
        catch (HttpRequestException ex)
        {
            Logger.LogError(ex, "Catalog API is unreachable");
            Error = "The catalog API isn't reachable right now. Try again in a moment.";
            return false;
        }
        finally
        {
            Busy = false;
        }
    }
}
