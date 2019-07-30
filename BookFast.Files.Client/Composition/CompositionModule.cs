using BookFast.SeedWork;
using BookFast.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Files.Client.Composition
{
    public class CompositionModule : ICompositionModule
    {
        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiOptions>(configuration.GetSection("FilesApi"));
            services.AddSingleton<IApiClientFactory<IBookFastFilesAPI>, FilesApiClientFactory>();
        }
    }
}
