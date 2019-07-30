using BookFast.SeedWork;
using BookFast.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Booking.Client.Composition
{
    public class CompositionModule : ICompositionModule
    {
        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiOptions>(configuration.GetSection("BookingApi"));
            services.AddSingleton<IApiClientFactory<IBookFastBookingAPI>, BookingApiClientFactory>();
        }
    }
}
