using System.Collections.Generic;
using System.Threading.Tasks;
using BookFast.Web.Contracts;
using BookFast.Web.Contracts.Search;
using BookFast.Search.Client;
using BookFast.Rest;

namespace BookFast.Web.Proxy
{
    internal class SearchProxy : ISearchProxy
    {
        private readonly IApiClientFactory<IBookFastSearchAPI> apiClientFactory;
        private readonly ISearchMapper mapper;

        public SearchProxy(ISearchMapper mapper, IApiClientFactory<IBookFastSearchAPI> apiClientFactory)
        {
            this.mapper = mapper;
            this.apiClientFactory = apiClientFactory;
        }
        
        public async Task<IList<SearchResult>> SearchAsync(string searchText, int page)
        {
            var api = await apiClientFactory.CreateApiClientAsync();
            var result = await api.SearchWithHttpMessagesAsync(page, searchText);
            return mapper.MapFrom(result.Body);
        }
    }
}