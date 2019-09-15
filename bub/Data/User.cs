
namespace bub.Data
{
    public class User
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public User(string userName, int id)
        {
            Name = userName;
            Id = id;
        }
    }
}
