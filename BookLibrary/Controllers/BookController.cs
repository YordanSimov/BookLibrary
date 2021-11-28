namespace BookLibrary.Controllers
{
    using BookLibrary.Data;
    using BookLibrary.ViewModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class BookController : Controller
    {
        private readonly BookLibraryDbContext dbContext;

        public BookController(BookLibraryDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult MyBooks()
        {
            if (String.IsNullOrEmpty(this.HttpContext.Session.GetString("loggedUser")))
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = this.HttpContext.Session.GetInt32("loggedUser");

            var viewModels = new ListOfAllBooksViewModel()
            {
                Books = new List<AllBooksViewModel>()
            };

            var queryDbContext = dbContext.Users
                .Include(x=>x.Books).ThenInclude(x=>x.Book).ThenInclude(x => x.Authors).ThenInclude(x => x.Author)
                .Include(x => x.Books).ThenInclude(x=>x.Book).ThenInclude(x => x.Genres).ThenInclude(x=>x.Genre)
                .Include(x => x.Books).ThenInclude(x=>x.Book).ThenInclude(x => x.Publisher)
                .FirstOrDefault(x => x.Id == userId).Books
                .Where(x=>x.IsDeleted == false).Where(x => x.Book.IsDeleted == false).Select(x=>x.Book);


            foreach (var book in queryDbContext)
            {
                viewModels.Books.Add(new AllBooksViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    Authors = book.Authors.Where(x => x.AuthorId == x.AuthorId).ToList(),
                    Genres = book.Genres.ToList(),
                    Pages = book.Pages,
                    PublisherName = book.Publisher.Name,
                });
            }

            return this.View(viewModels);
        }

        [HttpPost]
        public IActionResult RemoveBook(int id)
        {
            var userId = this.HttpContext.Session.GetInt32("loggedUser");

            var removeBook = dbContext.Users.Include(x=>x.Books).FirstOrDefault(x => x.Id == userId).Books.Where(x => x.BookId == id).FirstOrDefault();
            removeBook.IsDeleted = true;

            dbContext.SaveChanges();
            return this.RedirectToAction("MyBooks", "Book");
        }
    }
}
