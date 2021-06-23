using BookFast.SeedWork;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using BookFast.Search.Adapter.Mappers;
using BookFast.Search.Contracts;
using Azure.Search.Documents;
using Azure;

namespace BookFast.Search.Adapter.Composition
{
    public class CompositionModule : ICompositionModule
    {
        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SearchOptions>(configuration.GetSection("Search"));

            services.AddScoped(provider => CreateSearchClient(provider, true));

            services.AddScoped<ISearchResultMapper, SearchResultMapper>();
            services.AddScoped<ISearchServiceProxy>(provider => new SearchServiceProxy(CreateSearchClient(provider, false), provider.GetService<ISearchResultMapper>()));

            services.AddScoped<ISearchIndexer, SearchIndexer>();
        }

        private static SearchClient CreateSearchClient(IServiceProvider provider, bool useAdminKey)
        {
            var options = provider.GetService<IOptions<SearchOptions>>();
            return new SearchClient(new Uri(options.Value.ServiceEndpoint),
                options.Value.IndexName, new AzureKeyCredential(useAdminKey ? options.Value.AdminKey : options.Value.QueryKey));
        }
    }
}
