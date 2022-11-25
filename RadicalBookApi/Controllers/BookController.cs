using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RadicalBookApi.Data;
using RadicalBookApi.Models;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Cors;

namespace RadicalBookApi.Controllers
{
    [ApiController]
    [Route("[action]")]
    public class BookController : Controller
    {

        Regex _imageRegex;
        private readonly RadicalDbContext _db;
        public BookController(RadicalDbContext db)
        {
            _db = db;

            _imageRegex = new Regex(@"^(((https?:[/]{2})|([a-z]:[/\\])|[/\\])?([^<>:""/\\|?*\n\r\s]+[/\\])*([^<>:""/\\|?*\n\r\s]+\.(gif|png|jpg|jpeg)))$", RegexOptions.IgnoreCase);
        }

        [EnableCors("AllowLocalhost")]
        [HttpGet]
        public IActionResult GetBooks()
        {
            IEnumerable<Book> reviewedBooks = _db.Books;
            return new JsonResult(reviewedBooks);
        }

        [EnableCors("AllowLocalhost")]
        [HttpGet]
        public IActionResult AddToFavourites(string ISBN, string author, string title, string? publisher, string? description, string? price, string? imgUrl, int rating = 0)
        {
            if (ISBN == null || author == null || title == null)
            {
                return StatusCode(400, "ISBN, author and title are all required");
            }

            try
            {
                if (!Regex.Match(ISBN, @"^(97(8|9))?\d{9}(\d|X)$").Success)
                {
                    return StatusCode(400, "ISBN is incorrect. (ISBNs are 10 or 13 digits long, 13 digit ISBNs start with 978 or 979");
                }

                if (string.IsNullOrEmpty(imgUrl) || !_imageRegex.Match(imgUrl).Success)
                {
                    imgUrl = string.Empty;
                }

            }
            catch (Exception)
            {
                return StatusCode(400, "Check your data!");
            }

            Book book = new(ISBN,
                HttpUtility.HtmlEncode(author),
                HttpUtility.HtmlEncode(title),
                HttpUtility.HtmlEncode(publisher),
                HttpUtility.HtmlEncode(description),
                imgUrl,
                HttpUtility.HtmlEncode(price),
                rating);
            try
            {
                _db.Books.Add(book);
                _db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message.Contains("Violation of PRIMARY KEY"))
                    {
                        return StatusCode(400, "ISBN already in database");
                    }
                    return StatusCode(400, ex.InnerException.Message);
                }
                return StatusCode(400, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return new JsonResult($"{book.title} added to favourites.");
        }

        [EnableCors("AllowLocalhost")]
        [HttpGet]
        public IActionResult Update(string ISBN, string? author, string? title, string? publisher, string? description, string? price, string? imgUrl, int? rating)
        {

            if (ISBN == null)
            {
                return StatusCode(400, "ISBN required");
            }

            try
            {

                if (!Regex.Match(ISBN, @"^(97(8|9))?\d{9}(\d|X)$").Success)
                {
                    return StatusCode(400, "ISBN is incorrect. (ISBNs are 10 or 13 digits long, 13 digit ISBNs start with 978 or 979)");
                }

                Book? book = _db.Books.Find(ISBN);
                if (book == null)
                {
                    return StatusCode(404, "Book was not found in Database");
                }

                if (author is not null)
                {
                    author = HttpUtility.HtmlEncode(author.Trim());
                }

                if (title is not null)
                {
                    book.title = HttpUtility.HtmlEncode(title.Trim());
                }

                if (publisher is not null)
                {
                    book.publisher = HttpUtility.HtmlEncode(publisher.Trim());
                }

                if (description is not null)
                {
                    book.description = HttpUtility.HtmlEncode(description.Trim());
                }

                if (price is not null)
                {
                    book.price = HttpUtility.HtmlEncode(price.Trim());
                }

                if (imgUrl is not null && _imageRegex.Match(imgUrl).Success)
                {
                    book.imgUrl = imgUrl;
                }
                else if (imgUrl is not null)
                {
                    return StatusCode(400, "imgUrl must be a valid image file of type png, jpg or gif!");
                }

                if (rating is not null)
                {
                    if (rating < 0 || rating > 5)
                    {
                        return StatusCode(400, "Rating must be between 0 and 5 inclusive.");
                    }
                    book.rating = (int)rating;
                }

                _db.Books.Update(book);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(502, ex.Message);
            }
            return new JsonResult($"{ISBN} Updated");
        }

        [EnableCors("AllowLocalhost")]
        [HttpGet]
        public IActionResult Delete(string ISBN)
        {
            if (ISBN is null)
            {
                return StatusCode(400, "ISBN is required to delete a book.");
            }

            EntityEntry<Book> removedBook;

            try
            {
                Match ISBNCheck = Regex.Match(ISBN, @"^(97(8|9))?\d{9}(\d|X)$");
                if (!ISBNCheck.Success)
                {
                    return StatusCode(400, "ISBN is incorrect. (ISBNs are 10 or 13 digits long, 13 digit ISBNs start with 978 or 979");
                }

                Book? book = _db.Books.Find(ISBN);
                if (book == null)
                {
                    return StatusCode(404, "Book not found in database.");
                }
                removedBook = _db.Books.Remove(book);
                _db.SaveChanges();
            }
            catch (Exception)
            {
                return StatusCode(400, "Unable to delete that book from database.");
            }
            return new JsonResult($"Book Removed - {removedBook.OriginalValues["title"]}");
        }

    }
}
