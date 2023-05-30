using BookFast.PropertyManagement.Core.Commands.CreateAccommodation;
using BookFast.PropertyManagement.Core.Commands.DeleteAccommodation;
using BookFast.PropertyManagement.Core.Commands.UpdateAccommodation;
using BookFast.PropertyManagement.Core.Queries.GetAccommodation;
using BookFast.PropertyManagement.Core.Queries.ListAccommodations;
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
    public class AccommodationController : ControllerBase
    {
        private readonly IMediator mediator;

        public AccommodationController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        /// <summary>
        /// List accommodations by facility
        /// </summary>
        /// <param name="facilityId">Facility ID</param>
        /// <returns></returns>
        [HttpGet("api/facilities/{facilityId}/accommodations")]
        [SwaggerOperation("list-accommodations")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(IEnumerable<AccommodationRepresentation>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Facility not found")]
        [AllowAnonymous]
        public async Task<IActionResult> List(int facilityId)
        {
            return Ok(await mediator.Send(new ListAccommodationsQuery { FacilityId = facilityId }));
        }

        /// <summary>
        /// Find accommodation by ID
        /// </summary>
        /// <param name="id">Accommodation ID</param>
        /// <returns></returns>
        [HttpGet("api/accommodations/{id}")]
        [SwaggerOperation("find-accommodation")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(AccommodationRepresentation))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Accommodation not found")]
        [AllowAnonymous]
        public async Task<IActionResult> Find(int id)
        {
            return Ok(await mediator.Send(new GetAccommodationQuery { Id = id }));
        }

        /// <summary>
        /// Create new accommodation
        /// </summary>
        /// <param name="facilityId">Facility ID</param>
        /// <param name="accommodationData">Accommodation details</param>
        /// <returns></returns>
        [HttpPost("api/facilities/{facilityId}/accommodations")]
        [SwaggerOperation("create-accommodation")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.Created)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Description = "Invalid parameters")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Facility not found")]
        public async Task<IActionResult> Create([FromRoute] int facilityId, [FromBody] CreateAccommodationCommand accommodationData)
        {
            accommodationData.FacilityId = facilityId;
            var accommodationId = await mediator.Send(accommodationData);
            return CreatedAtAction("Find", new { id = accommodationId }, null);
        }

        /// <summary>
        /// Update accommodation
        /// </summary>
        /// <param name="id">Accommodation ID</param>
        /// <param name="accommodationData">Accommodation details</param>
        /// <returns></returns>
        [HttpPut("api/accommodations/{id}")]
        [SwaggerOperation("update-accommodation")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NoContent)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Description = "Invalid parameters")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Facility not found, Accommodation not found")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAccommodationCommand accommodationData)
        {
            accommodationData.AccommodationId = id;
            await mediator.Send(accommodationData);
            return NoContent();
        }

        /// <summary>
        /// Delete accommodation
        /// </summary>
        /// <param name="id">Accommodation ID</param>
        /// <returns></returns>
        [HttpDelete("api/accommodations/{id}")]
        [SwaggerOperation("delete-accommodation")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NoContent)]
        [SwaggerResponse((int)System.Net.HttpStatusCode.NotFound, Description = "Accommodation not found")]
        public async Task<IActionResult> Delete(int id)
        {
            await mediator.Send(new DeleteAccommodationCommand { AccommodationId = id });
            return NoContent();
        }
    }
}