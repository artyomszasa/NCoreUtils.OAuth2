using NCoreUtils.Data;

namespace NCoreUtils.OAuth2.Data
{
    public class RefreshToken : IHasId<int>
    {
        public int Id { get; set; }

        public string Sub { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Scopes { get; set; } = string.Empty;

        public long IssuedAt { get; set; }

        public long ExpiresAt { get; set; }
    }
}