namespace HexCatalog.Domain;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";

    // Self-registered accounts land here until an admin approves them.
    public const string Viewer = "Viewer";

    public static readonly IReadOnlyList<string> All = [Admin, User, Viewer];

    public static bool IsValid(string role) => All.Contains(role, StringComparer.Ordinal);
}
