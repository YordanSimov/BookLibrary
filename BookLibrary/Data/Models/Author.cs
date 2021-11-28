namespace BookLibrary.Data
{
    using BookLibrary.Data.Models;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Author
    {
        public Author()
        {
            this.Books = new List<BookAuthors>();
        }

        public int Id { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        public ICollection<BookAuthors> Books { get; set; }
    }
}
