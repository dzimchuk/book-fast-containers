using BookFast.PropertyManagement.Core.Commands.CreateFacility;
using BookFast.PropertyManagement.Core.Commands.DeleteFacility;
using BookFast.PropertyManagement.Core.Commands.UpdateFacility;
using BookFast.PropertyManagement.Core.Queries.GetFacility;
using BookFast.PropertyManagement.Core.Queries.GetProperty;
using BookFast.PropertyManagement.Core.Queries.ListFacilities;
using BookFast.PropertyManagement.Core.Queries.Representations;
using BookFast.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookFast.PropertyManagement.Controllers
{
    [Authorize(Policy = AuthorizationPolicies.PropertyWrite)]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private readonly IMediator mediator;

        public PropertyController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// List properties by owner
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/properties")]
        [SwaggerOperation("list-properties")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(IEnumerable<PropertyRepresentation>))]
        public async Task<IActionResult> List()
        {
            return Ok(await mediator.Send(new ListPropertiesQuery()));
        }

        /// <summary>
        /// Find property by ID
        /// </summary>
        /// <param name="id">Property ID</param>
        /// <returns></returns>
        [HttpGet("/api/properties/{id}")]
        [SwaggerOperation("find-property")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(PropertyRepresentation))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Property not found")]
        [AllowAnonymous]
        public async Task<IActionResult> Find(int id)
        {
            return Ok(await mediator.Send(new GetPropertyQuery { Id = id }));
        }

        /// <summary>
        /// Create new property
        /// </summary>
        /// <param name="propertyData">Property details</param>
        /// <returns></returns>
        [HttpPost("api/properties")]
        [SwaggerOperation("create-property")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Created)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Description = "Invalid parameters")]
        public async Task<IActionResult> Create([FromBody] CreatePropertyCommand propertyData)
        {
            var facilityId = await mediator.Send(propertyData);
            return CreatedAtAction("Find", new { id = facilityId }, null);
        }

        /// <summary>
        /// Update property
        /// </summary>
        /// <param name="id">Property ID</param>
        /// <param name="propertyData">Property details</param>
        /// <returns></returns>
        [HttpPut("api/properties/{id}")]
        [SwaggerOperation("update-property")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NoContent)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Description = "Invalid parameters")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Property not found")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePropertyCommand propertyData)
        {
            propertyData.PropertyId = id;
            await mediator.Send(propertyData);

            return NoContent();
        }

        /// <summary>
        /// Delete property
        /// </summary>
        /// <param name="id">Property ID</param>
        /// <returns></returns>
        [HttpDelete("api/properties/{id}")]
        [SwaggerOperation("delete-property")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NoContent)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Property not found")]
        public async Task<IActionResult> Delete(int id)
        {
            await mediator.Send(new DeletePropertyCommand { PropertyId = id });
            return NoContent();
        }
    }
}