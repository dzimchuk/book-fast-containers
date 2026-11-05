using BookFast.Common.Application.Security;

namespace BookFast.Common.TestInfrastructure.IntegrationTest
{
    public static class Constants
    {
        public const string CallerTenant = "54f673c8-44a8-4d05-aeac-92ea5f18c633";
        public const string UserId = "85ee76a1-422f-4e41-8d77-8af76f60620f";
        public const string UserName = "test@test.com";
        public const string Role = Roles.TenantUser;

        public const string AuthenticationScheme = "TestScheme";
    }
}