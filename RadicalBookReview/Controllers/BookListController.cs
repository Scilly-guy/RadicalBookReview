using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using RadicalBookReview.Models;
using System.Text.RegularExpressions;

namespace RadicalBookReview.Controllers
{
    public class BookListController : Controller
    {
        private readonly IConfiguration _configuration;
        private string _nytKey;
        private string _nytBookApi;
        public BookListController(IConfiguration configuration)
        {
            _configuration = configuration;

            _nytKey = "api-key=" + _configuration["NYT_API_KEY"];
            _nytBookApi="https://api.nytimes.com/svc/books/v3/";
        }
        public async Task<IActionResult> Index()
        {
            using HttpClient client = new();
            try
            {
                client.BaseAddress = new Uri(_nytBookApi+"lists/names.json");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }

            HttpResponseMessage response = await client.GetAsync('?'+_nytKey);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }

            var responseBody=await response.Content.ReadAsStringAsync();
            if(responseBody is null)
            {
                return new JsonResult("Could not read response from NYT");
            }
            nytResponse wholeResponse = JsonConvert.DeserializeObject<nytResponse>(responseBody);

            return View(wholeResponse.results);

        }
        [HttpPost]
        public async Task<IActionResult> GetBooks(string list)
        {
            //TO DO:sanitise list

            using HttpClient client = new();
            try
            {
                client.BaseAddress = new Uri(_nytBookApi+"lists/"+list+".json");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }

            HttpResponseMessage response = await client.GetAsync('?'+_nytKey);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            if(responseBody is null)
            {
                return new JsonResult("Could not read response from NYT");
            }
            nytBookList wholeResponse = JsonConvert.DeserializeObject<nytBookList>(responseBody);

            return new JsonResult(wholeResponse.results.books);

        }
        [HttpPost]
        public async Task<IActionResult> Search(string term)
        {
            //TO DO:sanitise term
            string searchType="?title=";
            if(Regex.Match(term, @"^(97(8|9))?\d{9}(\d|X)$").Success)
            {
                searchType = "?isbn=";
            }
            else if (Regex.Match(term, @"^author[\s:=]\w+", RegexOptions.IgnoreCase).Success) 
            {
                searchType = "?author=";
                term = term.Substring(7).Replace(' ', '+');            
            }

            using HttpClient client = new();            
            try
            {
                client.BaseAddress = new Uri(_nytBookApi+"reviews.json");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }

            HttpResponseMessage response = await client.GetAsync(searchType + term + '&'+_nytKey);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            if(responseBody is null)
            {
                return new JsonResult("Could not read response from NYT");
            }
            BookSearch wholeResponse = JsonConvert.DeserializeObject<BookSearch>(responseBody);

            return new JsonResult(wholeResponse.results);

        }



        public class nytResponse
        {
            public string status { get; set; }
            public string copyright { get; set; }
            public int num_results  { get; set; }
            public IEnumerable<BookList> results;
        }
        public class nytBookList
        {
            public string status { get; set; }
            public string copyright { get; set; }
            public int num_results { get; set; }
            public DateTime last_modified { get; set; }
            public Results results { get; set; }
        }
        public class BookSearch
        {
            public string status { get; set; }
            public string copyright { get; set; }
            public int num_results { get; set; }
            public IList<Result> results { get; set; }
        }

        public class Isbn
        {
            public string isbn10 { get; set; }
            public string isbn13 { get; set; }
        }

        public class BuyLink
        {
            public string name { get; set; }
            public string url { get; set; }
        }

        public class Book
        {
            public int rank { get; set; }
            public int rank_last_week { get; set; }
            public int weeks_on_list { get; set; }
            public int asterisk { get; set; }
            public int dagger { get; set; }
            public string primary_isbn10 { get; set; }
            public string primary_isbn13 { get; set; }
            public string publisher { get; set; }
            public string description { get; set; }
            public string price { get; set; }
            public string title { get; set; }
            public string author { get; set; }
            public string contributor { get; set; }
            public string contributor_note { get; set; }
            public string book_image { get; set; }
            public int book_image_width { get; set; }
            public int book_image_height { get; set; }
            public string amazon_product_url { get; set; }
            public string age_group { get; set; }
            public string book_review_link { get; set; }
            public string first_chapter_link { get; set; }
            public string sunday_review_link { get; set; }
            public string article_chapter_link { get; set; }
            public IList<Isbn> isbns { get; set; }
            public IList<BuyLink> buy_links { get; set; }
            public string book_uri { get; set; }
        }

        public class Results
        {
            public string list_name { get; set; }
            public string list_name_encoded { get; set; }
            public string bestsellers_date { get; set; }
            public string published_date { get; set; }
            public string published_date_description { get; set; }
            public string next_published_date { get; set; }
            public string previous_published_date { get; set; }
            public string display_name { get; set; }
            public int normal_list_ends_at { get; set; }
            public string updated { get; set; }
            public IList<Book> books { get; set; }
            public IList<object> corrections { get; set; }
        }

        public class Result
        {
            public string url { get; set; }
            public string publication_dt { get; set; }
            public string byline { get; set; }
            public string book_title { get; set; }
            public string book_author { get; set; }
            public string summary { get; set; }
            public string uuid { get; set; }
            public string uri { get; set; }
            public IList<string> isbn13 { get; set; }
        }

        


    }
}
