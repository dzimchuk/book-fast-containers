namespace BookFast.Common.Application.Queries
{
    public class ListQueryResult<TRepresentation>
    {
        public int? PageNumber { get; set; }

        public int? TotalPages { get; set; }

        public int? TotalRecords { get; set; }

        public IEnumerable<TRepresentation> Records { get; set; }
    }
}
