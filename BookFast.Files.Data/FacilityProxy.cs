using BookFast.Files.Business.Data;
using System.Threading.Tasks;
using BookFast.Files.Contracts.Models;
using BookFast.Facility.Client;
using BookFast.Rest;

namespace BookFast.Files.Data
{
    internal class FacilityProxy : IFacilityProxy
    {
        private readonly IApiClientFactory<IBookFastFacilityAPI> apiClientFactory;
        private readonly IFacilityMapper mapper;

        public FacilityProxy(IApiClientFactory<IBookFastFacilityAPI> apiClientFactory, IFacilityMapper mapper)
        {
            this.apiClientFactory = apiClientFactory;
            this.mapper = mapper;
        }

        public async Task<Accommodation> FindAccommodationAsync(int accommodationId)
        {
            var api = await apiClientFactory.CreateApiClientAsync();
            var result = await api.FindAccommodationWithHttpMessagesAsync(accommodationId);

            return result.Response.StatusCode == System.Net.HttpStatusCode.OK ? mapper.MapFrom(result.Body) : null;
        }

        public async Task<Contracts.Models.Facility> FindFacilityAsync(int facilityId)
        {
            var api = await apiClientFactory.CreateApiClientAsync();
            var result = await api.FindFacilityWithHttpMessagesAsync(facilityId);

            return result.Response.StatusCode == System.Net.HttpStatusCode.OK ? mapper.MapFrom(result.Body) : null;
        }
    }
}
