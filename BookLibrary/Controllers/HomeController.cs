namespace BookLibrary.Controllers
{
    using BookLibrary.Data;
    using BookLibrary.Data.Models;
    using BookLibrary.Models;
    using BookLibrary.ViewModels;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class HomeController : Controller
    {
        private readonly BookLibraryDbContext dbContext;

        public HomeController(BookLibraryDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            var viewModels = new ListOfAllBooksViewModel()
            {
                Books = new List<AllBooksViewModel>()
            };

            var query = dbContext.Books
                .Where(x => x.IsDeleted == false)
                .Select(x => new
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Authors = x.Authors.Select(x => x.Author),
                    Genres = x.Genres.Select(x => x.Genre),
                    Pages = x.Pages,
                    PublisherName = x.Publisher.Name,
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
        public IActionResult AddToBookList(int id)
        {
            var userId = this.HttpContext.Session.GetInt32("loggedUser");

            if (dbContext.Users.Include(x => x.Books).FirstOrDefault(x => x.Id == userId)
                .Books.Where(x => x.IsDeleted == false).Any(x => x.BookId == id))
            {
                return this.RedirectToAction("MyBooks", "Book");
            }

            dbContext.Users.FirstOrDefault(x => x.Id == userId).Books
                .Add(new UserBooks { BookId = id });

            dbContext.SaveChanges();
            return this.RedirectToAction("MyBooks", "Book");
        }

        public IActionResult Add()
        {
            if (String.IsNullOrEmpty(this.HttpContext.Session.GetString("adminUser")))
            {
                return RedirectToAction("Index", "Home");
            }

            return this.View();
        }

        public IActionResult Login()
        {
            if (!String.IsNullOrEmpty(this.HttpContext.Session.GetString("loggedUser")))
            {
                return RedirectToAction("Index", "Home");
            }

            return this.View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!String.IsNullOrEmpty(this.HttpContext.Session.GetString("loggedUser")) || !String.IsNullOrEmpty(this.HttpContext.Session.GetString("adminUser")))
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return this.View(model);
            }

            var loggedUser = dbContext.Users.FirstOrDefault(x => x.Username == model.Username);

            if (loggedUser == null || loggedUser.Password != model.Password)
            {
                ModelState.AddModelError(nameof(model.Username), "Incorrect username or password");
                return this.View(model);
            }

            if (loggedUser.Username == "admin")
            {
                this.HttpContext.Session.SetInt32("adminUser", loggedUser.Id);
                return this.RedirectToAction("Index");
            }

            this.HttpContext.Session.SetInt32("loggedUser", loggedUser.Id);

            return this.RedirectToAction("Index");
        }

        public IActionResult Logout()
        {
            if (String.IsNullOrEmpty(this.HttpContext.Session.GetString("loggedUser")) 
                && String.IsNullOrEmpty(this.HttpContext.Session.GetString("adminUser")))
            {
                return RedirectToAction("Index", "Home");
            }

            if (this.HttpContext.Session.Keys.Contains("loggedUser"))
            {
                this.HttpContext.Session.Remove("loggedUser");
                return RedirectToAction("Login", "Home");
            }

            this.HttpContext.Session.Remove("adminUser");

            return RedirectToAction("Login", "Home");
        }


        public IActionResult Register()
        {
            return this.View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel input)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(input);
            }

            if (input.Password.Length < 4 && !input.Password.Any(char.IsDigit))
            {
                ModelState.AddModelError(nameof(input.Password), "Password needs to be at least 4 characters long and should contain a digit");
                return this.View(input);
            }

            var userExists = dbContext.Users.FirstOrDefault(x => x.Username == input.Username);

            if (userExists != null)
            {
                ModelState.AddModelError(nameof(input.Username), "This username is already taken");
                return this.View(input);
            }

            dbContext.Users.Add(new User
            {
                Username = input.Username,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Password = input.Password,
                Role = new Role { Name = "User" }
            });

            dbContext.SaveChanges();

            return this.RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Add(AddAndEditBookViewModel input)
        {
            if (String.IsNullOrEmpty(this.HttpContext.Session.GetString("adminUser")))
            {
                return this.RedirectToAction("Index");
            }

            var authorsArray = input.Authors.Split(",");
            var genresArray = input.Genres.Split(",");

            var book = new Book
            {
                Title = input.Title,
                Authors = new List<BookAuthors>(),
                Pages = input.Pages,
                Description = input.Description,
                Publisher = new Publisher { Name = input.Publisher },
                Users = new List<UserBooks>(),
                Genres = new List<BookGenres>(),
            };
            book.Publisher.Books.Add(book);
            foreach (var author in authorsArray)
            {
                dbContext.BookAuthors.Add(new BookAuthors
                { Author = new Author { Name = author }, Book = book });
            }
            foreach (var genre in genresArray)
            {
                dbContext.BookGenres.Add(new BookGenres
                { Genre = new Genre { Name = genre }, Book = book });
            }
            book.Users.Add(new UserBooks { UserId = (int)this.HttpContext.Session.GetInt32("adminUser") });

            this.dbContext.Books.Add(book);
            this.dbContext.SaveChanges();

            return this.RedirectToAction("Index");
        }
        
        public IActionResult Edit(int id)
        {
            if (String.IsNullOrEmpty(this.HttpContext.Session.GetString("adminUser")))
            {
                return this.RedirectToAction("Index");
            }

            var bookToEdit = dbContext.Books
                .Where(x => x.Id == id)
                .Select(x=> new 
                { 
                    Id = x.Id,
                    Authors = x.Authors.Select(x=>x.Author.Name),
                    Genres = x.Genres.Select(x=>x.Genre.Name),
                    Title = x.Title,
                    Description = x.Description,
                    Pages = x.Pages,
                    PublisherName = x.Publisher.Name,
                }).FirstOrDefault();

            var viewModel = new AddAndEditBookViewModel
            {
                Id = bookToEdit.Id,
                Title = bookToEdit.Title,
                Description = bookToEdit.Description,
                Pages = bookToEdit.Pages,
                Publisher = bookToEdit.PublisherName,
            };

            foreach (var author in bookToEdit.Authors)
            {
                viewModel.Authors += author + ", ";
            }

            foreach (var genre in bookToEdit.Genres)
            {
                viewModel.Genres += genre + ", ";
            }

            return this.View(viewModel);
        }

        [HttpPost]

        public IActionResult Edit(AddAndEditBookViewModel input)
        {
            var bookToEdit = dbContext.Books.Include(x=>x.Publisher)
               .Where(x => x.Id == input.Id)
              .FirstOrDefault();

            bookToEdit.Title = input.Title;
            bookToEdit.Description = input.Description;
            bookToEdit.Pages = input.Pages;
            bookToEdit.Publisher.Name = input.Publisher;

            dbContext.SaveChanges();
            return this.RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (String.IsNullOrEmpty(this.HttpContext.Session.GetString("adminUser")))
            {
                return this.RedirectToAction("Index");
            }

            var bookToRemove = dbContext.Books
                .FirstOrDefault(x => x.Id == id);

            bookToRemove.IsDeleted = true;

            dbContext.SaveChanges();

            return this.RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
