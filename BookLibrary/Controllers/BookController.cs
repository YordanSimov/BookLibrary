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

            var query = dbContext.UserBooks
                .Where(x => x.UserId == userId)
                .Where(x => x.IsDeleted == false)
                .Where(x => x.Book.IsDeleted == false)
                .Select(u => new
                {
                    Id = u.Book.Id,
                    Title = u.Book.Title,
                    Description = u.Book.Description,
                    Authors = u.Book.Authors.Select(x => x.Author),
                    Genres = u.Book.Genres.Select(x => x.Genre),
                    PublisherName = u.Book.Publisher.Name,
                    Pages = u.Book.Pages,
                }).ToList();


            foreach (var book in query)
            {
                viewModels.Books.Add(new AllBooksViewModel
                {
                    Id = book.Id,
                    Title = book.Title,
                    Description = book.Description,
                    Authors = book.Authors.ToList(),
                    Genres = book.Genres.ToList(),
                    Pages = book.Pages,
                    PublisherName = book.PublisherName,
                });
            }

            return this.View(viewModels);
        }

        [HttpPost]
        public IActionResult RemoveBook(int id)
        {
            var userId = this.HttpContext.Session.GetInt32("loggedUser");

            var bookToRemove = dbContext.UserBooks
                .Where(x => x.UserId == userId)
                .Where(x => x.BookId == id)
                .FirstOrDefault(x => x.IsDeleted == false);

            bookToRemove.IsDeleted = true;

            dbContext.SaveChanges();
            return this.RedirectToAction("MyBooks", "Book");
        }
    }
}
