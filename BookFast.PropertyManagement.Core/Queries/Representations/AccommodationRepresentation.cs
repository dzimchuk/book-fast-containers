using BookFast.PropertyManagement.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace BookFast.PropertyManagement.Core.Queries.Representations
{
    public class AccommodationRepresentation
    {
        /// <summary>
        /// Accommodation ID
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Facility ID
        /// </summary>'
        [Required]
        public int FacilityId { get; set; }

        /// <summary>
        /// Accommodation name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Accommodation description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Number of rooms
        /// </summary>
        [Required]
        public int RoomCount { get; set; }

        /// <summary>
        /// Accommodation images
        /// </summary>
        public string[] Images { get; set; }
    }

    internal static class Mapper
    {
        public static AccommodationRepresentation ToRepresentation(this Accommodation model) =>
            new AccommodationRepresentation
            {
                Id = model.Id,
                FacilityId = model.PropertyId,
                Name = model.Name,
                Description = model.Description,
                RoomCount = model.RoomCount,
                Images = model.Images
            };
    }
}