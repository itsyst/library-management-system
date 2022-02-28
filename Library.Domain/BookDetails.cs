using System.ComponentModel.DataAnnotations;

namespace Library.Domain
{
    public class BookDetails
    {
        public int ID { get; set; }
        public string ISBN { get; set; } = "0000000000000";
        public string Title { get; set; } = string.Empty;
        
        [Display(Name="Author")]
        public int AuthorID { get; set; }
        public Author? Author { get; set; }
        public string Description { get; set; } = string.Empty;

        [Range(0, 1000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public ICollection<BookCopy>? Copies { get; set; }
    }
}
