namespace BookFast.Common.Api.Cors
{
    internal class CorsOptions
    {
        public string[] AllowOrigins { get; set; }
        public bool AllowCredentials { get; set; }
    }
}
