using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BookFast.Web.Contracts;
using BookFast.Web.Contracts.Exceptions;
using BookFast.Web.Contracts.Models;
using BookFast.Booking.Client;
using BookFast.Rest;

namespace BookFast.Web.Proxy
{
    internal class BookingProxy : IBookingProxy
    {
        private readonly IApiClientFactory<IBookFastBookingAPI> apiClientFactory;
        private readonly IBookingMapper mapper;

        public BookingProxy(IBookingMapper mapper, IApiClientFactory<IBookFastBookingAPI> apiClientFactory)
        {
            this.mapper = mapper;
            this.apiClientFactory = apiClientFactory;
        }

        public async Task BookAsync(string userId, int accommodationId, BookingDetails details)
        {
            var data = mapper.MapFrom(details);

            var api = await apiClientFactory.CreateApiClientAsync();
            var result = await api.CreateBookingWithHttpMessagesAsync(accommodationId, data);

            if (result.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AccommodationNotFoundException(accommodationId);
            }
        }

        public async Task<List<Contracts.Models.Booking>> ListPendingAsync(string userId)
        {
            var api = await apiClientFactory.CreateApiClientAsync();
            var result = await api.ListBookingsWithHttpMessagesAsync();

            return mapper.MapFrom(result.Body);
        }

        public async Task CancelAsync(string userId, Guid id)
        {
            var api = await apiClientFactory.CreateApiClientAsync();
            var result = await api.DeleteBookingWithHttpMessagesAsync(id);

            if (result.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new BookingNotFoundException(id);
            }
        }

        public async Task<Contracts.Models.Booking> FindAsync(string userId, Guid id)
        {
            var api = await apiClientFactory.CreateApiClientAsync();
            var result = await api.FindBookingWithHttpMessagesAsync(id);

            if (result.Response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new BookingNotFoundException(id);
            }

            return mapper.MapFrom(result.Body);
        }
    }
}