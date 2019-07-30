using BookFast.SeedWork;
using BookFast.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Facility.Client.Composition
{
    public class CompositionModule : ICompositionModule
    {
        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiOptions>(configuration.GetSection("FacilityApi"));
            services.AddSingleton<IApiClientFactory<IBookFastFacilityAPI>, FacilityApiClientFactory>();
        }
    }
}
