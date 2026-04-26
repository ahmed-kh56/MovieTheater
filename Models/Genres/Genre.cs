using MovieRatingApp.Models.Common;
using MovieRatingApp.Models.Movies;

namespace MovieRatingApp.Models.Genres
{
    public class Genre : IHasId
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastUpdate { get; private set; }
        public ICollection<MovieGenre> MovieGenres { get; private set; } = new List<MovieGenre>();
        public Genre(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            CreatedAt = DateTime.Now;
        }
        public void Update(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Genre name cannot be empty.");
            }
            Name = name;
            LastUpdate = DateTime.Now;
        }
        private Genre() { }
    }

}