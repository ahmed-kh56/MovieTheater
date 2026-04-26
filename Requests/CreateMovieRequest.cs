using System.ComponentModel.DataAnnotations;

namespace MovieRatingApp.Requests
{
    public class CreateMovieRequest
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Title can't be empty.")]
        [StringLength(maximumLength: 200,MinimumLength = 1, ErrorMessage = "Title can't be longer than 200 characters and can't be empty.")]
        public string Title { get; set; }
        public Guid? SeriesId { get; set; } = null;
        public int? OrderOnSeries { get; set; } = null;
        public string? Description { get; set; } = null;
        public IFormFile? Photo { get; set; } = null;
    }
}
