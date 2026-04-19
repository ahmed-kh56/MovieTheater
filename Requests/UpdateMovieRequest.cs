namespace MovieRatingApp.Requests
{
    public class UpdateMovieRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? Photo { get; set; }
    }
}
