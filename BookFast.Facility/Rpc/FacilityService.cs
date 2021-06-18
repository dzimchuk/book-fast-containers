using BookFast.Facility.QueryStack;
using Grpc.Core;
using System.Threading.Tasks;

namespace BookFast.Facility.Rpc
{
    internal class FacilityService : Facility.FacilityBase
    {
        private readonly IFacilityQueryDataSource facilityDataSource;
        private readonly IAccommodationQueryDataSource accommodationDataSource;

        public FacilityService(IFacilityQueryDataSource facilityDataSource, IAccommodationQueryDataSource accommodationDataSource)
        {
            this.facilityDataSource = facilityDataSource;
            this.accommodationDataSource = accommodationDataSource;
        }

        public override async Task<FacilityRepresentation> FindFacility(FindRequest request, ServerCallContext context)
        {
            var facility = await facilityDataSource.FindAsync(request.Id);
            if (facility == null)
            {
                var metadata = new Metadata
                {
                    { "Id", request.Id.ToString() }
                };

                throw new RpcException(new Status(StatusCode.NotFound, "Facility not found"), metadata);
            }

            var result = new FacilityRepresentation
            {
                Id = facility.Id,
                Name = facility.Name,
                Description = facility.Description,
                StreetAddress = facility.StreetAddress,
                Longitude = facility.Longitude,
                Latitude = facility.Latitude,
                AccommodationCount = facility.AccommodationCount
            };

            result.Images.AddRange(facility.Images);

            return result;
        }

        public async override Task<AccommodationRepresentation> FindAccommodation(FindRequest request, ServerCallContext context)
        {
            var accommodation = await accommodationDataSource.FindAsync(request.Id);
            if (accommodation == null)
            {
                var metadata = new Metadata
                {
                    { "Id", request.Id.ToString() }
                };

                throw new RpcException(new Status(StatusCode.NotFound, "Accommodation not found"), metadata);
            }

            var result = new AccommodationRepresentation
            {
                Id = accommodation.Id,
                FacilityId = accommodation.FacilityId,
                Name = accommodation.Name,
                Description = accommodation.Description,
                RoomCount = accommodation.RoomCount
            };

            result.Images.AddRange(accommodation.Images);

            return result;
        }
    }
}
