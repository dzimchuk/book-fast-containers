using BookFast.Facility.Rpc;
using BookFast.Search.Contracts;
using BookFast.Search.Contracts.Models;
using BookFast.Search.Indexer.Commands;
using Grpc.Core;
using Grpc.Net.Client;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using static BookFast.Facility.Rpc.Facility;

namespace BookFast.Search.Indexer.CommandHandlers
{
    public class UpdateAccommodationCommandHandler : AsyncRequestHandler<UpdateAccommodationCommand>
    {
        private readonly ISearchIndexer searchIndexer;
        private readonly IConfiguration configuration;

        public UpdateAccommodationCommandHandler(ISearchIndexer searchIndexer, IConfiguration configuration)
        {
            this.searchIndexer = searchIndexer;
            this.configuration = configuration;
        }

        private async Task<FacilityRepresentation> FindFacilityAsync(int facilityId)
        {
            using (var channel = GrpcChannel.ForAddress(configuration["FacilityApi:ServiceUri"]))
            {
                var client = new FacilityClient(channel);

                try
                {
                    return await client.FindFacilityAsync(new FindRequest { Id = facilityId });
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
                {
                    return null;
                }
            }
        }

        protected override async Task Handle(UpdateAccommodationCommand request, CancellationToken cancellationToken)
        {
            var facility = await FindFacilityAsync(request.FacilityId);

            if (facility == null)
            {
                return;
            }

            var accommodation = new Accommodation
            {
                Id = request.Id,
                FacilityId = request.FacilityId,
                Name = request.Name,
                Description = request.Description,
                RoomCount = request.RoomCount,
                Images = request.Images,
                FacilityName = facility.Name,
                FacilityDescription = facility.Description,
                Location = new Location
                {
                    Latitude = facility.Latitude,
                    Longitude = facility.Longitude
                }
            };

            await searchIndexer.IndexAccommodationAsync(accommodation);
        }
    }
}
