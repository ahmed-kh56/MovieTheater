using Microsoft.EntityFrameworkCore;
using MovieRatingApp.Models.Common;
using MovieRatingApp.Models.Genres;

namespace MovieRatingApp.Models.Movies
{
    public class Movie : IHasId
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string? Description { get; private set; }
        public string? PhotoUrl { get; private set; }
        public string? PhotoShowUrl { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedAt { get; private set; } = null;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastUpdate { get; private set; } = null;

        public Guid? SeriesId { get; private set; }
        public int? OrderOnSeries { get; private set; }
        public FilmSeries Series { get; private set; }



        public ICollection<OldMoviePhoto> OldPhotos { get; private set; } = new List<OldMoviePhoto>();
        public ICollection<MovieGenre> MovieGenres { get; private set; } = new List<MovieGenre>();

        public Movie(string title, string? description = null, string? photoUrl = null, Guid? seriesId = null, int? orderOnSeries = null)
        {
            Id = Guid.NewGuid();
            Title = title;
            SeriesId = seriesId;
            OrderOnSeries = orderOnSeries;
            Description = description;
            CreatedAt = DateTime.Now;
            PhotoUrl = photoUrl;
            CreatedAt = DateTime.Now;
        }
        private Movie() { }
        public void Update(string? title = null, string? description = null, string? photoUrl = null)
        {
            Title = title ?? Title;
            Description = description ?? Description;
            PhotoUrl = photoUrl ?? PhotoUrl;
            LastUpdate = DateTime.UtcNow;
        }
        public void UpdateSeries(Guid? seriesId, int? orderOnSeries)
        {
            SeriesId = seriesId;
            OrderOnSeries = orderOnSeries;
            LastUpdate = DateTime.UtcNow;
        }
        public void MarkDeleted()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
        public void SetPhotoShowUrl(string url)
        {
            PhotoShowUrl = url;
        }
    }

}
