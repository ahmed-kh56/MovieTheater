using MovieRatingApp.Models.Auth;
using MovieRatingApp.Models.Common;
using MovieRatingApp.Models.Movies;

namespace MovieRatingApp.Models.FavLists
{
    public class FavListItem : IHasId
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; private set; }
        public Guid? MovieId { get; private set; }
        public Movie Movie { get; private set; }
        public Guid? SeriesId { get; private set; }
        public FilmSeries Series { get; private set; }

        public FavListItemType Type { get; private set; }
        public FavListAction Action { get; private set; } = FavListAction.ToWatch;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LastUpdate { get; private set; } = null;

        private FavListItem() { }

        public FavListItem(
            Guid userId,
            Guid? movieId = null, Guid? seriesId = null,
            FavListItemType type = FavListItemType.Movie,
            FavListAction action = FavListAction.ToWatch)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            if (movieId.HasValue && seriesId.HasValue)
                throw new ArgumentException("Only one of movieId or seriesId should be provided.");
            if (type == FavListItemType.Movie && !movieId.HasValue)
                throw new ArgumentException("movieId must be provided for Movie type.");

            if (type == FavListItemType.Series && !seriesId.HasValue)
                throw new ArgumentException("seriesId must be provided for Series type.");

            if (type== FavListItemType.Movie)
                MovieId = movieId.Value;
            if (type == FavListItemType.Series)
                SeriesId = seriesId.Value;
            Type = type;
            Action = action;
            CreatedAt = DateTime.UtcNow;
        }
        public void Update(FavListAction action)
        {
            Action = action;
        }
        public FavListItem(Movie movie, FilmSeries series, Guid userId,FavListAction action)
        {
            Id = Guid.NewGuid();
            UserId= userId;
            if (movie is null && series is null)
                throw new ArgumentException("Cant be both is null.");

            Type = movie is null ? FavListItemType.Movie : FavListItemType.Series;

            MovieId = movie?.Id;
            SeriesId = series?.Id;
            Action = action;
            CreatedAt = DateTime.UtcNow;

        }

    }
}
