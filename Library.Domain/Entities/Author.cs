using Library.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Entities;

public class Author : AuditableEntity
{
    [Display(Name = "Fullständigt namn")]
    [Required(ErrorMessage = "Författarnamn är obligatoriskt")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Författarnamn måste vara mellan 2 och 100 tecken")]
    [RegularExpression(@"^[a-zA-Z\s\.\-\']+$", ErrorMessage = "Namnet får endast innehålla bokstäver, mellanslag, punkt, bindestreck och apostrof")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Biografi")]
    [StringLength(2000, ErrorMessage = "Biografin får inte överstiga 2000 tecken.")]
    public string? Biography { get; set; }

    [Display(Name = "Födelsedatum")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [Display(Name = "Dödsdatum")]
    [DataType(DataType.Date)]
    public DateTime? DeathDate { get; set; }

    [Display(Name = "Nationalitet")]
    [StringLength(50, ErrorMessage = "Nationalitet får vara högst 50 tecken")]
    public string? Nationality { get; set; }

    [Display(Name = "Webbplats")]
    [Url(ErrorMessage = "Ange en giltig webbadress")]
    [StringLength(50, ErrorMessage = "Webbplats får vara högst 50 tecken")]
    public string? Website { get; set; }

    public virtual ICollection<BookDetails> Books { get; set; } = new List<BookDetails>();

    // Computed property
    public int BooksCount => Books?.Count ?? 0;
}
