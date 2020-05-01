namespace NCoreUtils.OAuth2
{
    public interface ILoginProviderConfiguration
    {
        string Issuer { get; }

        bool UseEmailAsUsername { get; }
    }
}