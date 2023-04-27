using BookFast.Identity.Core.Commands.AddTenantUser;
using BookFast.Identity.Core.Commands.ChangeRole;
using BookFast.Identity.Core.Commands.RemoveTenantUser;
using BookFast.Identity.Core.Queries.FindTenantUser;
using BookFast.Identity.Core.Queries.ListTenantUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace BookFast.Identity.Controllers
{
    [ApiController]
    [Authorize(Policy = AuthorizationPolicies.TenantAdmin)]
    public class UserController : ControllerBase
    {
        private readonly IMediator mediator;

        public UserController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("users/{id}")]
        [SwaggerOperation("find-tenant-user")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Find(string id)
        {
            FindTenantUserQuery query = new() { Id = id };
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("users")]
        [SwaggerOperation("list-tenant-users")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Get([FromQuery] ListTenantUsersQuery query)
        {
            var result = await mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("users")]
        [SwaggerOperation("add-tenant-user")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Add([FromBody] AddTenantUserCommand command)
        {
            var id = await mediator.Send(command);
            return CreatedAtAction(nameof(Find), new { id }, null);
        }

        [HttpPut("users/{id}")]
        [SwaggerOperation("change-tenant-user-role")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ChangeRole(string id, [FromBody] ChangeRoleCommand command)
        {
            command.UserId = id;
            await mediator.Send(command);

            return NoContent();
        }

        [HttpDelete("users/{id}")]
        [SwaggerOperation("remove-tenant-user")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Remove(string id)
        {
            var command = new RemoveTenantUserCommand(id);
            await mediator.Send(command);

            return NoContent();
        }
    }
}
