using Library.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Library.MVC.ViewModels;

public class LoanViewModel
{
    // Add the missing property to fix the CS1061 error  
    [Required(ErrorMessage = "Du måste välja en medlem.")]
    public string? SelectedMemberId { get; set; }

    // Existing properties  
    public IEnumerable<SelectListItem>? Members { get; set; }
    public IEnumerable<SelectListItem>? Copies { get; set; }

    [Required(ErrorMessage = "Låneperiod fältet är obligatoriskt.")]
    [Range(1, 90, ErrorMessage = "Låneperioden måste vara mellan 1 och 90 dagar.")]
    public int LoanPeriodDays { get; set; }
 
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Du måste välja minst en bok.")]
    [MinLength(1, ErrorMessage = "Du måste välja minst en bok.")]
    public IEnumerable<string>? SelectedBookCopyIds { get; set; }

    public Loan? Loan { get; internal set; }
  
    [Display(Name = "Startdatum")]
    [DataType(DataType.DateTime)]
    [Required(ErrorMessage = "Startdatum är obligatoriskt.")]
    public DateTime StartDate { get; set; }

    [Display(Name = "Förfallodatum")]
    [DataType(DataType.DateTime)]
    [Required(ErrorMessage = "Förfallodatum är obligatoriskt.")]
    [CustomValidation(typeof(LoanViewModel), nameof(ValidateDueDate))]
    public DateTime DueDate { get; set; }

    public IReadOnlyList<BookCopyLoan>? BookCopyLoans { get; internal set; }
    public LoanStatistics? Statistics { get; internal set; }
    public IReadOnlyList<Loan>? Loans { get; internal set; }

    // Custom validation method for DueDate
    public static ValidationResult ValidateDueDate(DateTime dueDate, ValidationContext context)
    {
        var instance = (LoanViewModel)context.ObjectInstance;
        if (dueDate <= instance.StartDate)
        {
            return new ValidationResult("Förfallodatum måste vara efter startdatum.");
        }
        return ValidationResult.Success;
    }
}
