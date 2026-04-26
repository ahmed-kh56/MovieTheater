using MovieRatingApp.Models.Common;
using MovieRatingApp.Models.FavLists;

namespace MovieRatingApp.Models.Auth
{
    public class User:IHasId
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Password { get; private set; }
        public Role Role { get; private set; }
        public ICollection<FavListItem> FavListItems { get; private set; } = new List<FavListItem>();
        public User(string name, string password, Role role = Role.User)
        {
            Id = Guid.NewGuid();
            Name = name;
            Password = password;
            Role = role;
        }
        private User()
        {

        }
    }
}
