using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using BookFast.Search.Contracts;
using BookFast.Search.Contracts.Models;
using System.Threading.Tasks;

namespace BookFast.Search.Adapter
{
    internal class SearchIndexer : ISearchIndexer
    {
        private readonly SearchClient client;

        public SearchIndexer(SearchClient client)
        {
            this.client = client;
        }

        public Task DeleteAccommodationIndexAsync(int accommodationId)
        {
            return client.DeleteDocumentsAsync("Id", new[] { accommodationId.ToString() });
        }

        public Task IndexAccommodationAsync(Accommodation accommodation)
        {
            var action = IndexDocumentsAction.MergeOrUpload(accommodation);
            return client.IndexDocumentsAsync(IndexDocumentsBatch.Create(new[] { action }));
        }
    }
}
