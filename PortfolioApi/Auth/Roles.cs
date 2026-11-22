namespace PortfolioApi.Auth;

public static class Roles
{
    public const string Gast = "Gast";
    public const string Editor = "Editor";
    public const string Admin = "Admin";

    public static readonly string[] All = { Gast, Editor, Admin };
}