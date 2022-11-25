using Microsoft.AspNetCore.Mvc;

namespace RadicalBookReview.Controllers
{
    public class BookController : Controller
    {
        public BookController()
        {
        }

        public IActionResult Books()
        {
            return View();
        }
    }
}
