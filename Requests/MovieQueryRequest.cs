namespace MovieRatingApp.Requests
{
    public class MovieQueryRequest
    {
        public Guid? GenreId { get; set; }
        public Guid? MovieId { get; set; }
        public string? SearchTerm { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? UpdateFromDate { get; set; }
        public DateTime? UpdateToDate { get; set; }

        public string SortBy { get; set; } = "date";
        public bool IsDescending { get; set; } = true;

        public int Page { get; set; } = 0;
        public int PageSize { get; set; } = 12;
    }
}
