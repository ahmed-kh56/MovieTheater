namespace MovieRatingApp.Models
{
    public class Genre : IHasId
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastUpdate {  get; private set; }
        public ICollection<Movie> Movies { get; private set; } = new List<Movie>();
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
        public Genre() { }
    }
}
