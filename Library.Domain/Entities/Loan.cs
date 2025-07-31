using Library.Domain.Common;
using Library.Domain.Enums;
using Library.Domain.Utilities;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Entities;

public class Loan : AuditableEntity
{
    [Display(Name = "Startdatum")]
    [DataType(DataType.DateTime)]
    [Required(ErrorMessage = "Startdatum är obligatoriskt")]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Förfallodatum")]
    [DataType(DataType.DateTime)]
    [Required(ErrorMessage = "Förfallodatum är obligatoriskt")]
    public DateTime DueDate { get; set; }

    [Display(Name = "Returdatum")]
    [DataType(DataType.DateTime)]
    public DateTime? ReturnDate { get; set; }
 
    [Required(ErrorMessage = "Avgift är obligatoriskt.")]
    [Range(0, 999999.99, ErrorMessage = "Avgiften måste vara mellan 0 och 999999,99 SEK.")]
    public decimal Fee { get; set; } = 0;

    [Display(Name = "Status")]
    public LoanStatus Status { get; set; } = LoanStatus.Active;

    [Display(Name = "Anteckningar")]
    [StringLength(500, ErrorMessage = "Anteckningarna får vara högst 500 tecken")]
    public string? Notes { get; set; }

    [Display(Name = "Medlem")]
    [Required(ErrorMessage = "Medlem är obligatoriskt")]
    public int MemberId { get; set; }

    public virtual Member? Member { get; set; }

    public virtual ICollection<BookCopyLoan> BookCopyLoans { get; set; } = [];

    // Computed properties
    public bool IsOverdue => !ReturnDate.HasValue && DateTime.UtcNow > DueDate;
    public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;
    public int TotalBooks => BookCopyLoans?.Count ?? 0;

    // Constructor
    public Loan()
    {
        StartDate = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(FeeSettings.Days);
    }

    // Methods
    public decimal CalculateFee()
    {
        if (!IsOverdue) return 0;

        var daysOverdue = (DateTime.Now - DueDate).Days;
        var totalBooks = BookCopyLoans?.Count ?? 0;
        var calculatedFee = daysOverdue * FeeSettings.FeePerDayPerBook * totalBooks;

        // Apply max fee cap 
        Fee = Math.Min(calculatedFee, FeeSettings.MaxFee);
        return Fee;
    }

    public void MarkAsReturned()
    {
        ReturnDate = DateTime.UtcNow;
        Status = LoanStatus.Returned;
        Fee = CalculateFee();
    }
}
public class LoanStatistics
{
    public int ActiveLoans { get; set; }
    public int OverdueLoans { get; set; }
    public int ReturnedLoans { get; set; }
    public decimal TotalFees { get; set; }
}

public class LoanHistoryStatistics
{
    public int TotalMemberLoans { get; set; }
    public int MemberActiveLoans { get; set; }
    public decimal MemberTotalFees { get; set; }
    public int TotalBooksLoaned { get; set; }
    public double AverageBookPopularity { get; set; }
    public DateTime MemberSince { get; set; }

    // Add the missing property to fix the error  
    public int DaysSinceMembership
    {
        get
        {
            return (DateTime.Now - MemberSince).Days;
        }
    }
}
 