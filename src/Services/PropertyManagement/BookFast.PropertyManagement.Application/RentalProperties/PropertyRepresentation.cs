using System.ComponentModel.DataAnnotations;
using BookFast.PropertyManagement.Domain;

namespace BookFast.PropertyManagement.Application.RentalProperties
{
    public class PropertyRepresentation
    {
        /// <summary>
        /// Property ID
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Property name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Property description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Property address
        /// </summary>
        [Required]
        public AddressRepresentation Address { get; set; }

        /// <summary>
        /// Property location
        /// </summary>
        public LocationRepresentation Location { get; set; }

        /// <summary>
        /// Property images
        /// </summary>
        public string[] Images { get; set; }

        /// <summary>
        /// Facilities
        /// </summary>
        public Facility[] Facilities { get; set; }

        public bool IsActive { get; set; }
    }
}