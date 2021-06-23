using System.Collections.Generic;
using BookFast.Search.Contracts.Models;

namespace BookFast.Search.Adapter
{
    public interface ISearchResultMapper
    {
        IList<SearchResult> MapFrom(IList<Azure.Search.Documents.Models.SearchResult<Accommodation>> results);
        IList<SuggestResult> MapFrom(IList<Azure.Search.Documents.Models.SearchSuggestion<Accommodation>> results);

    }
}