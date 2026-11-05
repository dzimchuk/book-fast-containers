using Microsoft.Extensions.VectorData;

namespace BookFast.Search.Store
{
    internal class AccommodationIndexRecord
    {
        [VectorStoreKey]
        public Guid AccommodationId { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string TenantId { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string PropertyId { get; set; }

        [VectorStoreData]
        public string Name { get; set; }

        [VectorStoreData]
        public string Description { get; set; }

        [VectorStoreData]
        public string PropertyName { get; set; }

        [VectorStoreData]
        public string PropertyDescription { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public int? Bedrooms { get; set; }

        /// <summary>The accommodation's own images, from the accommodation event. Not filterable/returned
        /// directly. <see cref="Images"/> is the materialized, query-facing field (falls back to the
        /// property's images when this is empty).</summary>
        [VectorStoreData]
        public string[] OwnImages { get; set; } = [];

        [VectorStoreData]
        public string[] Images { get; set; } = [];

        /// <summary>The accommodation's own facilities, from the accommodation event. Not filterable/returned
        /// directly. <see cref="Facilities"/> is the materialized, query-facing field (merged with the
        /// property's facilities).</summary>
        [VectorStoreData]
        public string[] OwnFacilities { get; set; } = [];

        [VectorStoreData(IsIndexed = true)]
        public string[] Facilities { get; set; } = [];

        [VectorStoreData(IsIndexed = true)]
        public string City { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string Country { get; set; }

        [VectorStoreData]
        public double? Latitude { get; set; }

        [VectorStoreData]
        public double? Longitude { get; set; }

        /// <summary>The price a guest will actually pay, sourced from Booking's published Rate</summary>
        [VectorStoreData]
        public double? RateAmount { get; set; }

        [VectorStoreData]
        public string RateCurrency { get; set; }

        [VectorStoreData(IsFullTextIndexed = true)]
        public string SearchText { get; set; }

        [VectorStoreVector(EmbeddingsOptions.VectorDimensionSize)]
        public ReadOnlyMemory<float> Embedding { get; set; }

        /// <summary>Hash of <see cref="SearchText"/> at the time <see cref="Embedding"/> was last generated,
        /// so a re-index that doesn't change the composed text can skip regenerating it.</summary>
        [VectorStoreData]
        public string ContentHash { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public bool Active { get; set; } = true;

        [VectorStoreData(IsIndexed = true)]
        public bool Bookable { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public DateTimeOffset AccommodationOccurredAt { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public DateTimeOffset? PropertyOccurredAt { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public DateTimeOffset? BookableOccurredAt { get; set; }
    }
}
