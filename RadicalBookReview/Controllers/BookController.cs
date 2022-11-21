using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RadicalBookReview.Data;
using RadicalBookReview.Models;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace RadicalBookReview.Controllers
{
    public class BookController : Controller
    {

        Regex _imageRegex;
        private readonly RadicalDbContext _db;
        public BookController(RadicalDbContext db)
        {
            _db = db;

            _imageRegex = new Regex(@"^(((http:[/]{2})|(https:[/]{2})|([a-zA-Z]:[/\\]))?([^<>:""/\\|?*\n\r\s]*[/\\])*([^<>:""/\\|?*\n\r\s]+\.(gif|png|jpg)))$",RegexOptions.IgnoreCase);
        }

        public IActionResult Index()
        {
            IEnumerable<Book> reviewedBooks = _db.Books;
            //Are the data trustable
            return View(reviewedBooks);
        }
        [HttpGet]
        public IActionResult GetBooks()
        {
            IEnumerable<Book> reviewedBooks = _db.Books;
            return new JsonResult(reviewedBooks);
        }

        [HttpGet]
        public JsonResult AddToFavourites(string ISBN ,string author, string title , string publisher,string description,string price,string imgUrl,int rating=0)
        {
            if (ISBN == null || author == null || title == null)
            {
                return new JsonResult(StatusCode(400, "ISBN, author and title are all required"));
            }

            try
            {
                if (!Regex.Match(ISBN, @"^(97(8|9))?\d{9}(\d|X)$").Success)
                {
                    return new JsonResult(StatusCode(400, "ISBN is incorrect. (ISBNs are 10 or 13 digits long, 13 digit ISBNs start with 978 or 979"));
                }

                if (String.IsNullOrEmpty(imgUrl)||!_imageRegex.Match(imgUrl).Success)
                {
                    imgUrl = String.Empty;
                }
                
            }
            catch(Exception ex)
            {
                return new JsonResult(StatusCode(400, "Check your data!"));
            }
            

            //TO DO:sanitising inputs

            Book book = new(ISBN,author,title,publisher,description,imgUrl,price,rating);
            try
            {
                _db.Books.Add(book);
                _db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if(ex.InnerException != null)
                {
                    if(ex.InnerException.Message.Contains("Violation of PRIMARY KEY"))
                    {
                        return new JsonResult(StatusCode(400,"ISBN already in database"));
                    }
                    return new JsonResult(StatusCode(400,ex.InnerException.Message));
                }
                return new JsonResult(StatusCode(400,ex.Message));
            }
            catch (Exception ex)
            {
                return new JsonResult(StatusCode(500,ex.Message));
            }
            
            return new JsonResult(Ok(book.title+" added to favourites."));
        }
        [HttpGet]
        public JsonResult Update(string ISBN, string author, string title, string publisher, string description, string price, string imgUrl, int? rating)
        {

            if (ISBN == null)
            {
                return new JsonResult(StatusCode(400, "ISBN required"));
            }

            try
            {

                if (!Regex.Match(ISBN, @"^(97(8|9))?\d{9}(\d|X)$").Success)
                {
                    return new JsonResult(StatusCode(400, "ISBN is incorrect. (ISBNs are 10 or 13 digits long, 13 digit ISBNs start with 978 or 979"));
                }

                Book book = _db.Books.Find(ISBN);
                if(book == null)
                {
                    return new JsonResult(StatusCode(404,"Book was not found in Database"));
                }

                if(author is not null)
                {
                    author = author.Trim();
                }

                if(title is not null)
                {
                    book.title = title.Trim();
                }

                if(publisher is not null)
                {
                    book.publisher = publisher.Trim();
                }

                if(description is not null)
                {
                    book.description = description.Trim();
                }

                if(price is not null)
                {
                    book.price = price.Trim();
                }

                if((imgUrl is not null)&&_imageRegex.Match(imgUrl).Success)
                {
                    book.imgUrl = imgUrl;
                }

                if(rating is not null)
                {
                    if (rating < 0 || rating > 5) 
                    { 
                        return new JsonResult(StatusCode(400, "Rating must be between 0 and 5 inclusive.")); 
                    }
                    book.rating = (int)rating;
                }

                _db.Books.Update(book);
                _db.SaveChanges();
            }
            catch(Exception ex)
            {
                return new JsonResult(StatusCode(502, ex.Message));
            }
            return new JsonResult(Ok(ISBN+" Updated"));
        }

        public JsonResult Delete(string ISBN)
        {
            if(ISBN is null)
            {
                return new JsonResult(StatusCode(400, "ISBN is required to delete a book."));
            }
            try
            {
                Match ISBNCheck = Regex.Match(ISBN, @"^(97(8|9))?\d{9}(\d|X)$");
                if (!ISBNCheck.Success)
                {
                    return new JsonResult(StatusCode(400, "ISBN is incorrect. (ISBNs are 10 or 13 digits long, 13 digit ISBNs start with 978 or 979"));
                }

                Book book = _db.Books.Find(ISBN);
                _db.Books.Remove(book);
                _db.SaveChanges();
            }
            catch(Exception ex)
            {
                return new JsonResult(StatusCode(400, "Unable to delete that book from database."));
            }
            return new JsonResult(Ok("Book Removed"));
        }

    }
}
