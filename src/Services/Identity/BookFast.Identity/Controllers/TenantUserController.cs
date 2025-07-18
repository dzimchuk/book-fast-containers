using BookFast.Common.Presentation.Authorization;
using BookFast.Common.Presentation.Results;
using BookFast.Identity.Core.TenantUsers.AddTenantUser;
using BookFast.Identity.Core.TenantUsers.ChangeRole;
using BookFast.Identity.Core.TenantUsers.FindTenantUser;
using BookFast.Identity.Core.TenantUsers.ListTenantUsers;
using BookFast.Identity.Core.TenantUsers.RemoveTenantUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace BookFast.Identity.Controllers
{
    [ApiController]
    [Authorize(Policy = AuthorizationPolicies.TenantAdmin)]
    public class TenantUserController : ControllerBase
    {
        private readonly ISender sender;

        public TenantUserController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet("users/{id}")]
        [SwaggerOperation("find-tenant-user")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Find(string id)
        {
            var result = await sender.Send(new FindTenantUserQuery() { UserId = id });
            return result.Map(user => MvcResults.Ok(user), MvcResults.Problem);
        }

        [HttpGet("users")]
        [SwaggerOperation("list-tenant-users")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get([FromQuery] ListTenantUsersQuery query)
        {
            var result = await sender.Send(query);
            return result.Map(userList => MvcResults.Ok(userList), MvcResults.Problem);
        }

        [HttpPost("users")]
        [SwaggerOperation("add-tenant-user")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Add([FromBody] AddTenantUserCommand command)
        {
            var result = await sender.Send(command);
            return result.Map(id => MvcResults.CreatedAtAction(nameof(Find), new { id }, null), MvcResults.Problem);
        }

        [HttpPut("users/{id}/role")]
        [SwaggerOperation("change-tenant-user-role")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleCommand command)
        {
            command.UserId = id;
            var result = await sender.Send(command);

            return result.Map(MvcResults.NoContent, MvcResults.Problem);
        }

        [HttpDelete("users/{id}")]
        [SwaggerOperation("remove-tenant-user")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Remove(string id)
        {
            var result = await sender.Send(new RemoveTenantUserCommand { UserId = id });

            return result.Map(MvcResults.NoContent, MvcResults.Problem);
        }
    }
}
