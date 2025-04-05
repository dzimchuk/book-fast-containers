using BookFast.Common.Application.Security;

namespace BookFast.Common.TestInfrastructure.IntegrationTest
{
    public static class Constants
    {
        public const string CallerTenant = "830b76cf-36ae-4b0e-ad82-74afd1bf8a31";
        public const string UserId = "bd958059-a9a6-43e6-a84a-f1d8bd14ccd6";
        public const string UserName = "test@test.com";
        public const string Role = Roles.TenantUser;

        public const string AuthenticationScheme = "TestScheme";
    }
}