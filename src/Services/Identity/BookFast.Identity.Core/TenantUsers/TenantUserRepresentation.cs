namespace BookFast.Identity.Core.TenantUsers
{
    public record TenantUserRepresentation
    {
        public string UserId { get; init; }
        public string UserName { get; init; }
        public string Role { get; init; }
    }
}
