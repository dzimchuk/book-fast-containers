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
                                                                  configuration.CreateMap<Microsoft.Azure.Search.Models.Document, Document>()
                                                                               .ConvertUsing((searchDocument, doc) =>
                                                                                             {
                                                                                                 doc = new Document();
                                                                                                 foreach (var key in searchDocument.Keys)
                                                                                                 {
                                                                                                     doc.Add(key, searchDocument[key]);
                                                                                                 }

                                                                                                 return doc;
                                                                                             });
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
                                                                  configuration.CreateMap<Microsoft.Azure.Search.Models.SearchResult<Microsoft.Azure.Search.Models.Document>, SearchResult>();
                                                                  configuration.CreateMap<Microsoft.Azure.Search.Models.SuggestResult<Microsoft.Azure.Search.Models.Document>, SuggestResult>();

                                                              });

            mapperConfiguration.AssertConfigurationIsValid();
            Mapper = mapperConfiguration.CreateMapper();
        }

        public IList<SearchResult> MapFrom(IList<Microsoft.Azure.Search.Models.SearchResult<Microsoft.Azure.Search.Models.Document>> results)
        {
            return Mapper.Map<IList<SearchResult>>(results);
        }

        public IList<SuggestResult> MapFrom(IList<Microsoft.Azure.Search.Models.SuggestResult<Microsoft.Azure.Search.Models.Document>> results)
        {
            return Mapper.Map<IList<SuggestResult>>(results);
        }
    }
}