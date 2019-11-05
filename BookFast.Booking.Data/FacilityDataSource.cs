using System.Threading.Tasks;
using BookFast.Facility.Client;
using BookFast.Booking.CommandStack.Data;
using BookFast.Booking.Domain.Models;
using BookFast.Booking.Data.Mappers;
using BookFast.Rest;

namespace BookFast.Booking.Data
{
    internal class FacilityDataSource : IFacilityDataSource
    {
        private readonly IApiClientFactory<IBookFastFacilityAPI> apiClientFactory;

        public FacilityDataSource(IApiClientFactory<IBookFastFacilityAPI> apiClientFactory)
        {
            this.apiClientFactory = apiClientFactory;
        }

        public async Task<Accommodation> FindAccommodationAsync(int accommodationId)
        {
            var api = await apiClientFactory.CreateApiClientAsync();
            var result = await api.FindAccommodationWithHttpMessagesAsync(accommodationId);

            return result.Response.StatusCode == System.Net.HttpStatusCode.OK
                ? result.Body.ToDomainModel()
                : null;
        }

        public async Task<Domain.Models.Facility> FindFacilityAsync(int facilityId)
        {
            var api = await apiClientFactory.CreateApiClientAsync();
            var result = await api.FindFacilityWithHttpMessagesAsync(facilityId);

            return result.Response.StatusCode == System.Net.HttpStatusCode.OK
                ? result.Body.ToDomainModel()
                : null;
        }
    }
}
