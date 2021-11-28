namespace BookLibrary.Data.Models
{
    public class UserBooks
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        public Book Book { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public bool IsDeleted { get; set; }
    }
}
