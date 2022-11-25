using Microsoft.AspNetCore.Mvc;

namespace RadicalBookReview.Controllers
{
    public class BookListController : Controller
    {
        public BookListController()
        {
        }
        public IActionResult NytLists()
        {
            return View();
        }
    }
}
