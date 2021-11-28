namespace BookLibrary.Data
{
    using System.Collections.Generic;

    public class Genre
    {
        public Genre()
        {
            this.Books = new List<BookGenres>();
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<BookGenres> Books { get; set; }
    }
}
