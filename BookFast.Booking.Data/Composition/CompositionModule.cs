using BookFast.SeedWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BookFast.Rest;
using Microsoft.Extensions.Caching.Distributed;
using BookFast.Booking.CommandStack.Data;
using BookFast.Booking.QueryStack;
using BookFast.ReliableEvents;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookFast.Booking.Data.Composition
{
    public class CompositionModule : ICompositionModule
    {
        public void AddServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BookingContext>(options => options.UseSqlServer(configuration["Data:DefaultConnection:ConnectionString"], sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null); // see also https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
            })
            .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))); // disable client side evaluation, see https://docs.microsoft.com/en-us/ef/core/querying/client-eval

            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IBookingQueryDataSource, BookingQueryDataSource>();

            services.AddSingleton<IReliableEventsDataSource, ReliableEventsDataSource>();

            services.AddScoped<IFacilityDataSource, FacilityDataSource>();
            //services.AddScoped<IFacilityDataSource, CachingFacilityDataSource>(serviceProvider => 
            //    new CachingFacilityDataSource(serviceProvider.GetService<FacilityDataSource>(), serviceProvider.GetService<IDistributedCache>()));
            
            services.AddSingleton<IAccessTokenProvider, NullAccessTokenProvider>();
            new Facility.Client.Composition.CompositionModule().AddServices(services, configuration);

            //services.AddDistributedRedisCache(redisCacheOptions =>
            //{
            //    redisCacheOptions.Configuration = configuration["Redis:Configuration"];
            //    redisCacheOptions.InstanceName = configuration["Redis:InstanceName"];
            //});
        }
    }
}
