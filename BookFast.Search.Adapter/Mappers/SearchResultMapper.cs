using System.Collections.Generic;
using AutoMapper;
using BookFast.Search.Contracts.Models;

namespace BookFast.Search.Adapter.Mappers
{
    internal class SearchResultMapper : ISearchResultMapper
    {
        private static readonly IMapper Mapper;

        static SearchResultMapper()
        {
            var mapperConfiguration = new MapperConfiguration(configuration =>
                                                              {
                                                                  configuration.CreateMap<IDictionary<string, IList<string>>, HitHighlights>()
                                                                               .ConvertUsing((searchHighlights, highlights) =>
                                                                                             {
                                                                                                 if (searchHighlights == null)
                                                                                                     return null;

                                                                                                 highlights = new HitHighlights();
                                                                                                 foreach (var key in searchHighlights.Keys)
                                                                                                 {
                                                                                                     highlights.Add(key, searchHighlights[key]);
                                                                                                 }

                                                                                                 return highlights;
                                                                                             });
                                                                  configuration.CreateMap<Azure.Search.Documents.Models.SearchResult<Accommodation>, SearchResult>();
                                                                  configuration.CreateMap<Azure.Search.Documents.Models.SearchSuggestion<Accommodation>, SuggestResult>();

                                                              });

            mapperConfiguration.AssertConfigurationIsValid();
            Mapper = mapperConfiguration.CreateMapper();
        }

        public IList<SearchResult> MapFrom(IList<Azure.Search.Documents.Models.SearchResult<Accommodation>> results)
        {
            return Mapper.Map<IList<SearchResult>>(results);
        }

        public IList<SuggestResult> MapFrom(IList<Azure.Search.Documents.Models.SearchSuggestion<Accommodation>> results)
        {
            return Mapper.Map<IList<SuggestResult>>(results);
        }
    }
}