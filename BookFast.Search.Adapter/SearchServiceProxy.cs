using System.Collections.Generic;
using System.Threading.Tasks;
using SearchResult = BookFast.Search.Contracts.Models.SearchResult;
using SuggestResult = BookFast.Search.Contracts.Models.SuggestResult;
using BookFast.Search.Contracts;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BookFast.Search.Contracts.Models;
using System.Linq;

namespace BookFast.Search.Adapter
{
    internal class SearchServiceProxy : ISearchServiceProxy
    {
        private const int PageSize = 10;
        private readonly SearchClient client;
        private readonly ISearchResultMapper mapper;

        public SearchServiceProxy(SearchClient client, ISearchResultMapper mapper)
        {
            this.client = client;
            this.mapper = mapper;
        }

        public async Task<IList<SearchResult>> SearchAsync(string searchText, int page)
        {
            var options = new Azure.Search.Documents.SearchOptions
            {
                SearchMode = SearchMode.All,
                HighlightPreTag = "<b>",
                HighlightPostTag = "</b>",
                Skip = (page - 1) * PageSize
            };

            options.HighlightFields.Add("Name");
            options.HighlightFields.Add("Description");
            options.HighlightFields.Add("FacilityName");
            options.HighlightFields.Add("FacilityDescription");

            var result = await client.SearchAsync<Accommodation>(searchText, options);
            var items = result.Value.GetResults();
            
            return result.Value != null ? mapper.MapFrom(items.ToArray()) : null;
        }

        public async Task<IList<SuggestResult>> SuggestAsync(string searchText)
        {
            var options = new SuggestOptions
            {
                UseFuzzyMatching = true,
                HighlightPreTag = "<b>",
                HighlightPostTag = "</b>"
            };

            var result = await client.SuggestAsync<Accommodation>(searchText, "sg", options);
            var items = result.Value.Results;

            return result.Value != null ? mapper.MapFrom(items.ToArray()) : null;
        }
    }
}
