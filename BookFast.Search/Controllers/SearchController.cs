﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BookFast.Search.Contracts;
using BookFast.Search.Contracts.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace BookFast.Search.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private readonly ISearchServiceProxy service;

        public SearchController(ISearchServiceProxy service)
        {
            this.service = service;
        }

        /// <summary>
        /// Search for accommodations
        /// </summary>
        /// <param name="searchText">Search terms</param>
        /// <param name="page">Page number</param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("search")]
        [SwaggerResponse((int)System.Net.HttpStatusCode.OK, Type = typeof(IEnumerable<SearchResult>))]
        [SwaggerResponse((int)System.Net.HttpStatusCode.BadRequest, Description = "Invalid parameters")]
        public async Task<IActionResult> Search([FromQuery]string searchText, [FromQuery]int page = 1)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return BadRequest();

            if (page < 1)
                return BadRequest();

            var searchResults = await service.SearchAsync(searchText, page);
            return Ok(searchResults);
        }
    }
}