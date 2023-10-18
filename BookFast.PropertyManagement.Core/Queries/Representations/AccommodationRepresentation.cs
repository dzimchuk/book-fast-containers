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
        /// Property ID
        /// </summary>
        [Required]
        public int PropertyId { get; set; }

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

        /// <summary>
        /// Quantity
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        public bool IsActive { get; set; }
    }

    internal static class Mapper
    {
        public static AccommodationRepresentation ToRepresentation(this Accommodation model) =>
            new AccommodationRepresentation
            {
                Id = model.Id,
                PropertyId = model.PropertyId,
                Name = model.Name,
                Description = model.Description,
                RoomCount = model.RoomCount,
                Images = model.Images,
                Quantity = model.Quantity,
                Price = model.Price,
                IsActive = model.IsActive   
            };
    }
}