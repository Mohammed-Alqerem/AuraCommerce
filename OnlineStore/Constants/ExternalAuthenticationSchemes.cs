namespace OnlineStore.Constants;

public static class ExternalAuthenticationSchemes
{
    public const string ExternalCookie = "AuraCommerce.External";
    public const string Google = "Google";
    public const string Apple = "Apple";

    public static bool IsSupported(string? provider) => provider is Google or Apple;
}
