using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cors;
using RadicalBookApi.Models;

namespace RadicalBookApi.Controllers
{
    [ApiController]
    [Route("[action]")]
    public class BookListController : ControllerBase
    {
        private string? _nytKey;
        private string? _nytBookApi;

        private readonly ILogger<BookListController>? _logger;

        public BookListController(ILogger<BookListController> logger, IConfiguration configuration)
        {
            //TO DO add logging
            _logger = logger;

            _nytKey = "api-key=" + configuration["NYT_API_KEY"];
            _nytBookApi = configuration["NYT_BOOK_API"];
        }

        [HttpGet]
        [EnableCors]
        public async Task<JsonResult> GetLists()
        {

            using HttpClient client = new();
            try
            {
                client.BaseAddress = new Uri($"{_nytBookApi}lists/names.json");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception ex)
            {
                return new JsonResult(StatusCode(503, ex.Message));
            }

            HttpResponseMessage response = await client.GetAsync('?' + _nytKey);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                return new JsonResult(StatusCode(502, "Could not connect to New York Times Book API"));
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            if (responseBody is null)
            {
                return new JsonResult(StatusCode(502, "Could not read response from NYT"));
            }

            nytResponse? wholeResponse = JsonConvert.DeserializeObject<nytResponse>(responseBody);
            if (wholeResponse != null)
            {
                return new JsonResult(Ok(wholeResponse.results));
            }
            else
            {
                return new JsonResult(StatusCode(500, "Deserializing NYT Response failed."));
            }
        }
        [EnableCors("AllowLocalhost")]
        [HttpGet]
        public async Task<JsonResult> GetBooksList(string list)
        {
            //TO DO:sanitise list

            using HttpClient client = new();
            try
            {
                client.BaseAddress = new Uri($"{_nytBookApi}lists/{list}.json");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception)
            {
                return new JsonResult(StatusCode(406, "List not found"));
            }

            HttpResponseMessage response = await client.GetAsync('?' + _nytKey);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                return new JsonResult(StatusCode(400, ex.Message));
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            if (responseBody is null)
            {
                return new JsonResult(StatusCode(502, "Could not read response from NYT"));
            }
            nytBookList? wholeResponse = JsonConvert.DeserializeObject<nytBookList>(responseBody);

            if (wholeResponse?.results?.books is not null)
            {
                return new JsonResult(wholeResponse.results.books);
            }
            else
            {
                return new JsonResult(StatusCode(500, "Could not read response from NYT"));
            }


        }
        [HttpGet]
        [EnableCors]
        public async Task<IActionResult> Search(string term)
        {
            //TO DO:sanitise term
            string searchType = "?title=";
            try
            {
                if (Regex.Match(term, @"^(97(8|9))?\d{9}(\d|X)$").Success)
                {
                    searchType = "?isbn=";
                }
                else if (Regex.Match(term, @"^author[\s:=]\w+", RegexOptions.IgnoreCase).Success)
                {
                    searchType = "?author=";
                    term = term[7..].Replace(' ', '+');
                }
            }
            catch (Exception)
            {
                return new JsonResult(StatusCode(400, "Poorly Formed Search Term"));
            }

            using HttpClient client = new();
            try
            {
                client.BaseAddress = new Uri($"{_nytBookApi}reviews.json");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            catch (Exception ex)
            {
                return new JsonResult(StatusCode(503, ex.Message));
            }

            HttpResponseMessage response = await client.GetAsync(searchType + term + '&' + _nytKey);

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                return new JsonResult(StatusCode(400, ex.Message));
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            if (responseBody is null)
            {
                return new JsonResult(StatusCode(502, "Could not read response from NYT"));
            }

            BookSearch? wholeResponse = JsonConvert.DeserializeObject<BookSearch>(responseBody);

            if (wholeResponse?.results is not null)
            {
                if (wholeResponse.results.Count() > 0)
                {
                    return new JsonResult(wholeResponse.results);
                }
                else
                {
                    return new JsonResult("No results found");
                }
            }
            else
            {
                return new JsonResult(StatusCode(500, "Could not read response from NYT"));
            }


        }
        public class nytResponse
        {
            public string? status { get; set; }
            public string? copyright { get; set; }
            public int? num_results { get; set; }
            public IEnumerable<BookList>? results;
        }
        public class nytBookList
        {
            public string? status { get; set; }
            public string? copyright { get; set; }
            public int? num_results { get; set; }
            public DateTime? last_modified { get; set; }
            public Results? results { get; set; }
        }
        public class BookSearch
        {
            public string? status { get; set; }
            public string? copyright { get; set; }
            public int? num_results { get; set; }
            public IList<Result>? results { get; set; }
        }

        public class Isbn
        {
            public string? isbn10 { get; set; }
            public string? isbn13 { get; set; }
        }

        public class BuyLink
        {
            public string? name { get; set; }
            public string? url { get; set; }
        }

        public class FullBook
        {
            public int? rank { get; set; }
            public int? rank_last_week { get; set; }
            public int? weeks_on_list { get; set; }
            public int? asterisk { get; set; }
            public int? dagger { get; set; }
            public string? primary_isbn10 { get; set; }
            public string? primary_isbn13 { get; set; }
            public string? publisher { get; set; }
            public string? description { get; set; }
            public string? price { get; set; }
            public string? title { get; set; }
            public string? author { get; set; }
            public string? contributor { get; set; }
            public string? contributor_note { get; set; }
            public string? book_image { get; set; }
            public int? book_image_width { get; set; }
            public int? book_image_height { get; set; }
            public string? amazon_product_url { get; set; }
            public string? age_group { get; set; }
            public string? book_review_link { get; set; }
            public string? first_chapter_link { get; set; }
            public string? sunday_review_link { get; set; }
            public string? article_chapter_link { get; set; }
            public IList<Isbn>? isbns { get; set; }
            public IList<BuyLink>? buy_links { get; set; }
            public string? book_uri { get; set; }
        }

        public class Results
        {
            public string? list_name { get; set; }
            public string? list_name_encoded { get; set; }
            public string? bestsellers_date { get; set; }
            public string? published_date { get; set; }
            public string? published_date_description { get; set; }
            public string? next_published_date { get; set; }
            public string? previous_published_date { get; set; }
            public string? display_name { get; set; }
            public int? normal_list_ends_at { get; set; }
            public string? updated { get; set; }
            public IList<FullBook>? books { get; set; }
            public IList<object>? corrections { get; set; }
        }

        public class Result
        {
            public string? url { get; set; }
            public string? publication_dt { get; set; }
            public string? byline { get; set; }
            public string? book_title { get; set; }
            public string? book_author { get; set; }
            public string? summary { get; set; }
            public string? uuid { get; set; }
            public string? uri { get; set; }
            public IList<string>? isbn13 { get; set; }
        }
    }
}