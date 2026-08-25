namespace OnlineStore.Services;

public sealed class ExternalAuthenticationOptions
{
    public GoogleAuthenticationOptions Google { get; init; } = new();
    public AppleAuthenticationOptions Apple { get; init; } = new();

    public sealed class GoogleAuthenticationOptions
    {
        public string ClientId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ClientId) &&
            !string.IsNullOrWhiteSpace(ClientSecret);
    }

    public sealed class AppleAuthenticationOptions
    {
        public string ClientId { get; init; } = string.Empty;
        public string TeamId { get; init; } = string.Empty;
        public string KeyId { get; init; } = string.Empty;
        public string PrivateKey { get; init; } = string.Empty;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(ClientId) &&
            !string.IsNullOrWhiteSpace(TeamId) &&
            !string.IsNullOrWhiteSpace(KeyId) &&
            !string.IsNullOrWhiteSpace(PrivateKey);
    }
}

public sealed record ExternalProviderAvailability(bool Google, bool Apple)
{
    public bool IsEnabled(string provider) => provider switch
    {
        Constants.ExternalAuthenticationSchemes.Google => Google,
        Constants.ExternalAuthenticationSchemes.Apple => Apple,
        _ => false
    };
}
