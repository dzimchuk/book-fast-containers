using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookFast.Booking.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBookingInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
