using BookFast.SeedWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookFast.Rest;

namespace BookFast.Search.Client.Composition
{
    public class CompositionModule : ICompositionModule
    {
        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiOptions>(configuration.GetSection("SearchApi"));
            services.AddSingleton<IApiClientFactory<IBookFastSearchAPI>, SearchApiClientFactory>();
        }
    }
}
