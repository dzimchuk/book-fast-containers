using BookFast.Common.Application.Security;
using Microsoft.AspNetCore.Http;

namespace BookFast.Common.Api.SecurityContext
{
    internal class SecurityContextMiddleware
    {
        private readonly RequestDelegate next;

        public SecurityContextMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public Task Invoke(HttpContext context, ISecurityContext securityContext)
        {
            var acceptor = securityContext as SecurityContextProvider;
            if (acceptor != null)
            {
                acceptor.SetPrincipal(context.User);
            }

            return next(context);
        }
    }
}
