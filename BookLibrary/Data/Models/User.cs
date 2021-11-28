namespace BookLibrary.Data
{
    using BookLibrary.Data.Models;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class User
    {
        public User()
        {
            this.Books = new List<UserBooks>();
        }

        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Username { get; set; }

        [MaxLength(20)]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(20)]
        [Required]
        public string LastName { get; set; }

        [MaxLength(25)]
        public string Password { get; set; }

        public Role Role { get; set; }

        public ICollection<UserBooks> Books { get; set; }
    }
}
