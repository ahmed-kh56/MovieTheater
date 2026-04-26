namespace MovieRatingApp.Models.Movies
{
    public class OldMoviePhoto
    {
        public Guid Id { get; private set; }
        public Guid MovieId { get; private set; }
        public Movie Movie { get; private set; }

        public string LocalPhotoUrl { get; private set; }
        public string PhotoShowUrl { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public OldMoviePhoto(Guid movieId, string photoUrl, string showUrl)
        {
            Id = Guid.NewGuid();
            MovieId = movieId;
            LocalPhotoUrl = photoUrl;
            PhotoShowUrl = showUrl;
            CreatedAt = DateTime.UtcNow;
        }

        public OldMoviePhoto() { }

    }
}
