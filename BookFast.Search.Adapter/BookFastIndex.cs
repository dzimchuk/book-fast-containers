using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Configuration;

namespace BookFast.Search.Adapter
{
    internal class BookFastIndex
    {
        private readonly SearchIndexClient client;
        private readonly IConfiguration configuration;

        public BookFastIndex(SearchIndexClient client, IConfiguration configuration)
        {
            this.client = client;
            this.configuration = configuration;
        }

        public async Task ProvisionAsync()
        {
            var indexName = configuration["Search:IndexName"];
            await CreateIndexAsync(indexName);
        }

        private Task CreateIndexAsync(string indexName)
        {
            var suggester = new SearchSuggester("sg", new[] { "Name", "FacilityName" });

            var definition = new SearchIndex(indexName)
            {
                Fields = new List<SearchField>
                         {
                             new SearchField("Id", SearchFieldDataType.String) { IsKey = true },
                             new SearchField("FacilityId", SearchFieldDataType.Int32) { IsFilterable = true },
                             new SearchField("Name", SearchFieldDataType.String) { IsSearchable = true, AnalyzerName = LexicalAnalyzerName.EnMicrosoft },
                             new SearchField("Description", SearchFieldDataType.String) { IsSearchable = true, AnalyzerName = LexicalAnalyzerName.EnMicrosoft },
                             new SearchField("FacilityName", SearchFieldDataType.String) { IsSearchable = true, AnalyzerName = LexicalAnalyzerName.EnMicrosoft },
                             new SearchField("FacilityDescription", SearchFieldDataType.String) { IsSearchable = true, AnalyzerName = LexicalAnalyzerName.EnMicrosoft },
                             new SearchField("Location", SearchFieldDataType.GeographyPoint) { IsFilterable = true },
                             new SearchField("RoomCount", SearchFieldDataType.Int32) { IsFilterable = true },
                             new SearchField("Images", SearchFieldDataType.Collection(SearchFieldDataType.String)) { IsFilterable = false }
                         }
            };

            definition.Suggesters.Add(suggester);

            return client.CreateOrUpdateIndexAsync(definition);
        }
    }
}