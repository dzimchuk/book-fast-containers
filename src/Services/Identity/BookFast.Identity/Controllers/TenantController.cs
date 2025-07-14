using BookFast.Common.Presentation.Authorization;
using BookFast.Identity.Core.Tenants.AddTenant;
using BookFast.Identity.Core.Tenants.FindTenant;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace BookFast.Identity.Controllers
{
    [ApiController]
    [Authorize(Policy = AuthorizationPolicies.GlobalAdmin)]
    public class TenantController : ControllerBase
    {
        private readonly ISender sender;

        public TenantController(ISender sender)
        {
            this.sender = sender;
        }

        [HttpGet("tenants/{id}")]
        [SwaggerOperation("find-tenant")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Find(string id)
        {
            var result = await sender.Send(new FindTenantQuery() { TenantId = id });
            return Ok(result);
        }

        [HttpPost("tenants")]
        [SwaggerOperation("add-tenant")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Add([FromBody] AddTenantCommand command)
        {
            var id = await sender.Send(command);
            return CreatedAtAction(nameof(Find), new { id }, null);
        }
    }
}
