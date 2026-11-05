using System.ComponentModel.DataAnnotations;
using BookFast.PropertyManagement.Domain;

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
        /// Number of bedrooms
        /// </summary>
        public int? Bedrooms { get; set; }

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
        /// Price range
        /// </summary>
        public PriceRangeRepresentation PriceRange { get; set; }

        /// <summary>
        /// Facilities
        /// </summary>
        public Facility[] Facilities { get; set; }

        public bool IsActive { get; set; }
    }
}