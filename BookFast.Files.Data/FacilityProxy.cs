using BookFast.Files.Business.Data;
using System.Threading.Tasks;
using BookFast.Files.Contracts.Models;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using System;
using BookFast.Facility.Rpc;
using static BookFast.Facility.Rpc.Facility;
using Grpc.Core;

namespace BookFast.Files.Data
{
    internal class FacilityProxy : IFacilityProxy, IDisposable
    {
        private readonly GrpcChannel channel;

        public FacilityProxy(IConfiguration configuration)
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
                return new Accommodation { Id = accommodation.Id, FacilityId = accommodation.FacilityId };
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Contracts.Models.Facility> FindFacilityAsync(int facilityId)
        {
            var client = new FacilityClient(channel);

            try
            {
                var facility = await client.FindFacilityAsync(new FindRequest { Id = facilityId });
                return new Contracts.Models.Facility { Id = facility.Id };
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                return null;
            }
        }
    }
}
