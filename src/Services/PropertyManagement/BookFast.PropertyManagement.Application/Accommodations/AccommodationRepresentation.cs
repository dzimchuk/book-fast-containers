using System.ComponentModel.DataAnnotations;

namespace BookFast.PropertyManagement.Application.Accommodations
{
    public class AccommodationRepresentation
    {
        /// <summary>
        /// Accommodation ID
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Property ID
        /// </summary>
        [Required]
        public Guid PropertyId { get; set; }

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
}