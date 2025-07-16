using Library.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Entities;

public class BookDetails : AuditableEntity
{
    [Required(ErrorMessage = "ISBN är obligatoriskt")]
    [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN måste vara mellan 10 och 13 tecken")]
    [Display(Name = "ISBN")]
    [RegularExpression(@"^(?:ISBN(?:-1[03])?:?\s*)?(?=[0-9X]{10}$|(?=(?:[0-9]+[-\s]){3})[-\s0-9X]{13}$|97[89][0-9]{10}$|(?=(?:[0-9]+[-\s]){4})[-\s0-9]{17}$)(?:97[89][-\s]?)?[0-9]{1,5}[-\s]?[0-9]+[-\s]?[0-9]+[-\s]?[0-9X]$",
        ErrorMessage = "Ange ett giltigt ISBN")]
    public string ISBN { get; set; } = string.Empty;

    [Required(ErrorMessage = "Titel är obligatorisk")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Titeln måste vara mellan 1 och 200 tecken")]
    [Display(Name = "Titel")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Undertitel")]
    [StringLength(200)]
    public string? Subtitle { get; set; }

    [Display(Name = "Författare")]
    [Required(ErrorMessage = "Författare är obligatorisk")]
    public int AuthorId { get; set; }

    public virtual Author? Author { get; set; }

    [StringLength(2000)]
    [Display(Name = "Beskrivning")]
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Omslagsbild")]
    public string? ImageBinary { get; set; }

    [Display(Name = "Publiceringsdatum")]
    [DataType(DataType.Date)]
    public DateTime? PublicationDate { get; set; }

    [Display(Name = "Antal sidor")]
    [Range(1, 10000, ErrorMessage = "Antalet sidor måste vara mellan 1 och 10 000")]
    public int? Pages { get; set; }

    [Display(Name = "Förlag")]
    [StringLength(100)]
    public string? Publisher { get; set; }

    [Display(Name = "Språk")]
    [StringLength(50)]
    public string? Language { get; set; } = "Engelska";

    [Display(Name = "Genre")]
    [StringLength(100)]
    public string? Genre { get; set; }

    [Display(Name = "Upplaga")]
    [StringLength(50)]
    public string? Edition { get; set; }

    public virtual ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();

    // Computed properties
    public int TotalCopies => Copies?.Count ?? 0;
    public int AvailableCopies => Copies?.Count(c => c.IsAvailable) ?? 0;
    public bool IsAvailable => AvailableCopies > 0;
}
