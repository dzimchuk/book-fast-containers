using System.Threading.Tasks;
using BookFast.Booking.CommandStack.Data;
using BookFast.Booking.Domain.Models;
using BookFast.Booking.Data.Mappers;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Grpc.Core;
using BookFast.Facility.Rpc;
using static BookFast.Facility.Rpc.Facility;

namespace BookFast.Booking.Data
{
    internal class FacilityDataSource : IFacilityDataSource
    {
        private readonly GrpcChannel channel;

        public FacilityDataSource(IConfiguration configuration)
        {
            channel = GrpcChannel.ForAddress(configuration["FacilityApi:ServiceUri"]);
        }

        public void Dispose()
        {
            if (channel != null)
            {
                channel.Dispose();
            }
        }

        public async Task<Accommodation> FindAccommodationAsync(int accommodationId)
        {
            var client = new FacilityClient(channel);

            try
            {
                var accommodation = await client.FindAccommodationAsync(new FindRequest { Id = accommodationId });
                return accommodation.ToDomainModel();
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Domain.Models.Facility> FindFacilityAsync(int facilityId)
        {
            var client = new FacilityClient(channel);

            try
            {
                var facility = await client.FindFacilityAsync(new FindRequest { Id = facilityId });
                return facility.ToDomainModel();
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
