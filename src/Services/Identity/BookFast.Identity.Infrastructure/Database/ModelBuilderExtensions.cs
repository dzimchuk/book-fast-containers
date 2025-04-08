using BookFast.Identity.Core.Models;
using EFCore.NamingConventions.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using System.Globalization;

namespace BookFast.Identity.Infrastructure.Database
{
    internal static class ModelBuilderExtensions
    {
        public static void RewriteIdentityTableNames(this ModelBuilder modelBuilder)
        {
            var rewriter = new SnakeCaseNameRewriter(CultureInfo.InvariantCulture);

            modelBuilder.Entity<User>(b =>
            {
                b.ToTable(rewriter.RewriteName("Users"));
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable(rewriter.RewriteName("UserClaims"));
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable(rewriter.RewriteName("UserLogins"));
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable(rewriter.RewriteName("UserTokens"));
            });

            modelBuilder.Entity<Role>(b =>
            {
                b.ToTable(rewriter.RewriteName("Roles"));
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable(rewriter.RewriteName("RoleClaims"));
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable(rewriter.RewriteName("UserRoles"));
            });
        }

        public static void RewriteOpenIddictTableNames(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OpenIddictEntityFrameworkCoreApplication>(b =>
            {
                b.ToTable("openiddict_applications");
            });

            modelBuilder.Entity<OpenIddictEntityFrameworkCoreAuthorization>(b =>
            {
                b.ToTable("openiddict_authorizations");
            });

            modelBuilder.Entity<OpenIddictEntityFrameworkCoreScope>(b =>
            {
                b.ToTable("openiddict_scopes");
            });

            modelBuilder.Entity<OpenIddictEntityFrameworkCoreToken>(b =>
            {
                b.ToTable("openiddict_tokens");
            });
        }
    }
}
