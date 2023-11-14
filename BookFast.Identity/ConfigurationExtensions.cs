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

        public static DataProtectionSettings GetDataProtectionSettings(this IConfiguration configuration, string configKey = "DataProtection")
        {
            return configuration.GetSection(configKey).Get<DataProtectionSettings>();
        }

        internal class AuthServerSettings
        {
            public string[] Scopes { get; set; }

            public bool EnableAccessTokenEncryption { get; set; }

            public string SigningCertificate { get; set; }

            public string EncryptionSymmetricKey { get; set; }

            public int CookieLifetimeInMinutes { get; set; }
            public bool CookieSlidingExpiration { get; set; }
        }

        internal class DataProtectionSettings
        {
            public string StorageConnectionString { get; set; }
        }
    }

}
