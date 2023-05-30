using BookFast.PropertyManagement.Core.Commands.CreateFacility;
using BookFast.PropertyManagement.Core.Commands.DeleteFacility;
using BookFast.PropertyManagement.Core.Commands.UpdateFacility;
using BookFast.PropertyManagement.Core.Queries.GetFacility;
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
    [Authorize(Policy = AuthorizationPolicies.FacilityWrite)]
    [ApiController]
    public class PropertyController : ControllerBase
    {
        private readonly IMediator mediator;

        public PropertyController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// List facilities by owner
        /// </summary>
        /// <returns></returns>
        [HttpGet("api/facilities")]
        [SwaggerOperation("list-facilities")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(IEnumerable<FacilityRepresentation>))]
        public async Task<IActionResult> List()
        {
            return Ok(await mediator.Send(new ListFacilitiesQuery()));
        }

        /// <summary>
        /// Find facility by ID
        /// </summary>
        /// <param name="id">Facility ID</param>
        /// <returns></returns>
        [HttpGet("/api/facilities/{id}")]
        [SwaggerOperation("find-facility")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(FacilityRepresentation))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Facility not found")]
        [AllowAnonymous]
        public async Task<IActionResult> Find(int id)
        {
            return Ok(await mediator.Send(new GetFacilityQuery { Id = id }));
        }

        /// <summary>
        /// Create new facility
        /// </summary>
        /// <param name="facilityData">Facility details</param>
        /// <returns></returns>
        [HttpPost("api/facilities")]
        [SwaggerOperation("create-facility")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Created)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Description = "Invalid parameters")]
        public async Task<IActionResult> Create([FromBody] CreatePropertyCommand facilityData)
        {
            var facilityId = await mediator.Send(facilityData);
            return CreatedAtAction("Find", new { id = facilityId }, null);
        }

        /// <summary>
        /// Update facility
        /// </summary>
        /// <param name="id">Facility ID</param>
        /// <param name="facilityData">Facility details</param>
        /// <returns></returns>
        [HttpPut("api/facilities/{id}")]
        [SwaggerOperation("update-facility")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NoContent)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Description = "Invalid parameters")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Facility not found")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePropertyCommand facilityData)
        {
            facilityData.PropertyId = id;
            await mediator.Send(facilityData);

            return NoContent();
        }

        /// <summary>
        /// Delete facility
        /// </summary>
        /// <param name="id">Facility ID</param>
        /// <returns></returns>
        [HttpDelete("api/facilities/{id}")]
        [SwaggerOperation("delete-facility")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NoContent)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Facility not found")]
        public async Task<IActionResult> Delete(int id)
        {
            await mediator.Send(new DeletePropertyCommand { PropertyId = id });
            return NoContent();
        }
    }
}