using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RadicalBookReview.Data;
using RadicalBookReview.Models;
using System.Security.Policy;

namespace RadicalBookReview.Controllers
{
    public class BookController : Controller
    {
        private readonly RadicalDbContext _db;
        public BookController(RadicalDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Book> reviewedBooks = _db.Books;
            return View(reviewedBooks);
        }

        [HttpPost]
        public JsonResult AddToFavourites(string ISBN ,string author, string title , string publisher,string description,string price,string imgUrl,int rating)
        {
            Book book = new(ISBN,author,title,publisher,description,imgUrl,price,rating);
            _db.Books.Add(book);
            _db.SaveChanges();
            return new JsonResult(Ok(book.title+" added to favourites."));
        }
        [HttpPost]
        public string Update(string ISBN, int rating)
        {
            Book book = _db.Books.Find(ISBN);
            book.rating = rating;
            _db.Books.Update(book);
            _db.SaveChanges();
            return "Rating Updated";
        }
        [HttpPost]
        public string Delete(string ISBN)
        {
            Book book = _db.Books.Find(ISBN);
            _db.Books.Remove(book);
            _db.SaveChanges();
            return "Book Removed";
        }
    }
}
