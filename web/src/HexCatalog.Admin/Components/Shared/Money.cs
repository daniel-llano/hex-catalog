using System.Globalization;

namespace HexCatalog.Admin.Components.Shared;

public static class Money
{
    // Prices are stored in one currency, so format them the same way on every server.
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");
}
