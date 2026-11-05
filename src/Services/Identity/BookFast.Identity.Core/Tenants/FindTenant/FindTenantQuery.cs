using BookFast.Common.Application.Messaging;

namespace BookFast.Identity.Core.Tenants.FindTenant
{
    public record FindTenantQuery : IQuery<TenantRepresentation>
    {
        [SwaggerIgnore]
        public string TenantId { get; init; }
    }
}
