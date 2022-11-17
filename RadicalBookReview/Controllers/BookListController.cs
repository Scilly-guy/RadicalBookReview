using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using RadicalBookReview.Models;

namespace RadicalBookReview.Controllers
{
    public class BookListController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var nytBookApi = "https://api.nytimes.com/svc/books/v3/lists/names.json";
            var nytKey="?api-key=ZnKshfRonaC0A3biKUAi3cYhYI0XdZDY";

            using HttpClient client = new();
            client.BaseAddress = new Uri(nytBookApi);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync(nytKey);
            response.EnsureSuccessStatusCode();
            var responseBody=await response.Content.ReadAsStringAsync();
            nytResponse wholeResponse=JsonConvert.DeserializeObject<nytResponse>(responseBody);

            return View(wholeResponse.results);

        }

        public class nytResponse
        {
            public string status { get; set; }
            public string copyright { get; set; }
            public int num_results  { get; set; }
            public IEnumerable<BookList> results;
        }

    }
}
