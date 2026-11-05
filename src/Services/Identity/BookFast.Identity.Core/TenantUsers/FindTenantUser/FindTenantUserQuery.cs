using BookFast.Common.Application.Messaging;

namespace BookFast.Identity.Core.TenantUsers.FindTenantUser
{
    public record FindTenantUserQuery : IQuery<TenantUserRepresentation>
    {
        [SwaggerIgnore]
        public string UserId { get; init; }
    }
}
