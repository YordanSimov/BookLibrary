namespace BookLibrary.Data
{
    using BookLibrary.Data.Models;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Book
    {
        public Book()
        {
            this.Authors = new List<BookAuthors>();
            this.Genres = new List<BookGenres>();
            this.Users = new List<UserBooks>();
        }

        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string Title { get; set; }

        [MaxLength(700)]
        [Required]
        public string Description { get; set; }

        public int Pages { get; set; }
       
        public bool IsDeleted { get; set; }

        public int PublisherId { get; set; }

        public Publisher Publisher { get; set; }

        public ICollection<UserBooks> Users { get; set; }

        public ICollection<BookAuthors> Authors { get; set; }

        public ICollection<BookGenres> Genres { get; set; }       
    }
}
