using MovieRatingApp.Models.Common;

namespace MovieRatingApp.Models.Movies
{
    public class FilmSeries :IHasId
    {

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public ICollection<Movie> Movies { get; private set; }= new List<Movie>();


        public FilmSeries(string name)
        {
            Name = name;
        }

        public void Update(string name)
        {
            Name=name;
        }
    }
}
