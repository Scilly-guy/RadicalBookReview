using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace RadicalBookApi.Models
{
    public class Book
    {
        [Key]
        [RegularExpression(@"^(97(8|9))?\d{9}(\d|X)$")]
        public string ISBN { get; set; }
        [Required]
        public string author { get; set; }
        [Required]
        public string? title { get; set; }
        public string? publisher { get; set; }
        public string? description { get; set; }
        public string? price { get; set; }
        public string? imgUrl { get; set; }
        [Range(0, 5, ErrorMessage = "Rating must be an integer in the range 0 to 5 inclusive.")]
        public int rating { get; set; }
        public Book(string ISBN, string author, string title, string publisher = "", string description = "", string imgUrl = "", string price = "0", int rating = 0)
        {
            this.ISBN = ISBN;
            this.publisher = publisher;
            this.author = author;
            this.title = title;
            this.description = description;
            this.price = price;
            this.imgUrl = imgUrl;
            this.rating = rating;
        }
    }
}