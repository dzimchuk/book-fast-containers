using System.ComponentModel.DataAnnotations;

namespace BookFast.PropertyManagement.Core.Queries.Representations
{
    public class PropertyRepresentation
    {
        /// <summary>
        /// Property ID
        /// </summary>
        [Required]
        public int Id { get; set; }

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

        public bool IsActive { get; set; }
    }
}