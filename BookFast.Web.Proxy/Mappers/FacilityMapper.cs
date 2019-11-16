using AutoMapper;
using BookFast.Facility.Client.Models;
using BookFast.Web.Contracts.Models;
using System.Collections.Generic;
using System.Linq;

namespace BookFast.Web.Proxy.Mappers
{
    internal class FacilityMapper : IFacilityMapper
    {
        private static readonly IMapper Mapper;

        static FacilityMapper()
        {
            var mapperConfiguration = new MapperConfiguration(configuration =>
            {
                configuration.CreateMap<FacilityRepresentation, Contracts.Models.Facility>()
                             .ConvertUsing(representation => new Contracts.Models.Facility
                                                             {
                                                                 Id = representation.Id,
                                                                 Owner = null,
                                                                 Details = new FacilityDetails
                                                                           {
                                                                               Name = representation.Name,
                                                                               Description = representation.Description,
                                                                               StreetAddress = representation.StreetAddress,
                                                                               Images = representation.Images != null ? representation.Images.ToArray() : null
                                                                           },
                                                                 Location = new Location
                                                                            {
                                                                                Latitude = representation.Latitude,
                                                                                Longitude = representation.Longitude
                                                                            },
                                                                 AccommodationCount = representation.AccommodationCount
                                                             });
                configuration.CreateMap<FacilityDetails, CreateFacilityCommand>()
                             .ForMember(command => command.Latitude, config => config.Ignore())
                             .ForMember(command => command.Longitude, config => config.Ignore())
                             .ForMember(command => command.Images, 
                                config => config.ConvertUsing(new ArrayToListConverter()));

                configuration.CreateMap<FacilityDetails, UpdateFacilityCommand>()
                             .ForMember(command => command.FacilityId, config => config.Ignore())
                             .ForMember(command => command.Latitude, config => config.Ignore())
                             .ForMember(command => command.Longitude, config => config.Ignore())
                             .ForMember(command => command.Images,
                                config => config.ConvertUsing(new ArrayToListConverter()));
            });

            mapperConfiguration.AssertConfigurationIsValid();
            Mapper = mapperConfiguration.CreateMapper();
        }

        public List<Contracts.Models.Facility> MapFrom(IList<FacilityRepresentation> facilities)
        {
            return Mapper.Map<List<Contracts.Models.Facility>>(facilities);
        }

        public Contracts.Models.Facility MapFrom(FacilityRepresentation facility)
        {
            return Mapper.Map<Contracts.Models.Facility>(facility);
        }

        public CreateFacilityCommand ToCreateCommand(FacilityDetails details)
        {
            return Mapper.Map<CreateFacilityCommand>(details);
        }

        public UpdateFacilityCommand ToUpdateCommand(FacilityDetails details)
        {
            return Mapper.Map<UpdateFacilityCommand>(details);
        }
    }
}