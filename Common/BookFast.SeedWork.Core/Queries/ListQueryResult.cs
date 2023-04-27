namespace BookFast.SeedWork.Core.Queries
{
    public class ListQueryResult<TData>
    {
        public int? PageNumber { get; set; }

        public int? TotalPages { get; set; }

        public int? TotalRecords { get; set; }

        public IEnumerable<TData> Records { get; set; }
    }
}
