namespace BookLibrary.ViewModels
{
    using BookLibrary.Data;
    using BookLibrary.Data.Models;
    using System.Collections.Generic;

    public class AllBooksViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Pages { get; set; }

        public string PublisherName { get; set; }

        public ICollection<Author> Authors { get; set; }

        public ICollection<Genre> Genres { get; set; }
    }
}
