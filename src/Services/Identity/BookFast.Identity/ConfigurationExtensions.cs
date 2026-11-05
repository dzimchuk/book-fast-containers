namespace BookFast.Identity
{
    internal static class ConfigurationExtensions
    {
        public static AuthServerSettings GetAuthServerSettings(this IConfiguration configuration, string configKey = "AuthServer")
        {
            var authSettings = new AuthServerSettings();

            configuration.Bind(configKey, authSettings);

            return authSettings;
        }

        internal class AuthServerSettings
        {
            public string[] Scopes { get; set; }

            public string SigningCertificateThumbprint { get; set; }
            public string SigningCertificate { get; set; }
                        
            public string EncryptionSymmetricKey { get; set; }
            public bool EnableAccessTokenEncryption { get; set; }

            public int CookieExpireTimeInMinutes { get; set; }
            public bool CookieSlidingExpiration { get; set; }
        }
    }
}
