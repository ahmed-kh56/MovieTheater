namespace MovieRatingApp.Models
{
    public class Movie :IHasId
    {
        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public string? Description { get; private set; }
        public string? PhotoUrl { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastUpdate { get; private set; }

        public Guid GenreId { get; private set; }
        public Genre Genre { get; private set; }

        public Movie(string title, Guid genreId, string? description = null, string? photoUrl = null)
        {
            Title = title;
            GenreId = genreId;
            Description = description;
            CreatedAt = DateTime.Now;
            PhotoUrl = photoUrl;
            GenreId = genreId;
        }
        public Movie() { }
        public void Update(string? title = null, string? description = null, string? photoUrl = null)
        {
            Title = title ?? Title;
            Description = description ?? Description;
            PhotoUrl = photoUrl ?? PhotoUrl;
            LastUpdate = DateTime.Now;
        }

    }
}
