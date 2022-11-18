using System.ComponentModel.DataAnnotations;

namespace RadicalBookReview.Models
{
    public class Book
    {
        [Key]
        public string ISBN { get; set; }
        [Required]
        public string author { get; set; }
        [Required]
        public string? title { get; set; }
        public string? publisher { get; set; }
        public string? description { get; set; }
        public string? price { get; set; }
        public string? imgUrl { get; set; }
        public int rating { get; set; }
        public Book(string ISBN, string author, string title,   string publisher="",string description="", string imgUrl="",string price="0", int rating=0)
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
