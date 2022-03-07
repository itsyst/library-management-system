using System.ComponentModel.DataAnnotations;

namespace Library.Domain
{
    public class Author
    {
        public int Id { get; set; }

        [Display(Name = "Full Name")]
        [MaxLength(30)]
        [Required]
        public string Name { get; set; } = string.Empty;
        public IList<BookDetails>? Books { get; set; }
    }
}