using Library.Domain.Common;
using Library.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Entities;

public class BookCopy : AuditableEntity
{
    [Display(Name = "Bokinformation")]
    [Required(ErrorMessage = "Bokinformation är obligatorisk")]
    public int DetailsId { get; set; }

    [ValidateNever]
    public virtual BookDetails? Details { get; set; }

    [Display(Name = "Skick")]
    [Required(ErrorMessage = "Skick är obligatoriskt")]
    public BookCondition Condition { get; set; } = BookCondition.Good;

    [Display(Name = "Plats")]
    [StringLength(100, ErrorMessage = "Platsen får vara högst 100 tecken")]
    public string? Location { get; set; }

    [Display(Name = "Streckkod")]
    [StringLength(50, ErrorMessage = "Streckkoden får vara högst 50 tecken")]
    public string? Barcode { get; set; }

    [Display(Name = "Inköpsdatum")]
    [DataType(DataType.Date)]
    public DateTime? PurchaseDate { get; set; }

    [Display(Name = "Inköpspris")]
    [Range(0, 10000, ErrorMessage = "Priset måste vara mellan 0 och 10 000")]
    [DataType(DataType.Currency)]
    public decimal? PurchasePrice { get; set; }

    [Display(Name = "Tillgänglig")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Anteckningar")]
    [StringLength(500, ErrorMessage = "Anteckningarna får vara högst 500 tecken")]
    public string? Notes { get; set; }

    // Navigation properties
    [ValidateNever]
    public virtual ICollection<BookCopyLoan> BookCopyLoans { get; set; } = new List<BookCopyLoan>();
}


