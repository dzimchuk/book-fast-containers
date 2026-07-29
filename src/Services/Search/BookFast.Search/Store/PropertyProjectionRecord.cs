using Microsoft.Extensions.VectorData;

namespace BookFast.Search.Store
{
    internal class PropertyProjectionRecord
    {
        [VectorStoreKey]
        public Guid PropertyId { get; set; }

        [VectorStoreData]
        public string Name { get; set; }

        [VectorStoreData]
        public string Description { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string City { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string Country { get; set; }

        [VectorStoreData]
        public double? Latitude { get; set; }

        [VectorStoreData]
        public double? Longitude { get; set; }

        [VectorStoreData]
        public string[] Facilities { get; set; } = [];

        [VectorStoreData]
        public string[] Images { get; set; } = [];

        [VectorStoreData]
        public bool Active { get; set; } = true;

        [VectorStoreData(IsIndexed = true)]
        public DateTimeOffset OccurredAt { get; set; }

        [VectorStoreVector(EmbeddingsOptions.VectorDimensionSize)]
        public ReadOnlyMemory<float> Placeholder { get; set; } = new float[EmbeddingsOptions.VectorDimensionSize];
    }
}
