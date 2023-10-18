using System.ComponentModel.DataAnnotations;

namespace BookFast.PropertyManagement.Core.Queries.Representations
{
    public class AddressRepresentation
    {
        [Required]
        public string Country { get; set; }

        public string State { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        /// <summary>
        /// Street address
        /// </summary>
        public string Street { get; set; }

        [Required]
        public string ZipCode { get; set; }

        public static AddressRepresentation Map(Address address) =>
            new()
            {
                Country = address.Country,
                State = address.State,
                City = address.City,
                Street = address.Street,
                ZipCode = address.ZipCode
            };
    }
}
