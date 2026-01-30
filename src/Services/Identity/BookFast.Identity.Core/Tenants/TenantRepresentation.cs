namespace BookFast.Identity.Core.Tenants
{
    public record TenantRepresentation
    {
        public string TenantId { get; set; }
        public string TenantName { get; set; }
    }
}
