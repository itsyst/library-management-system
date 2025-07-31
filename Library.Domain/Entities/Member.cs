using Library.Domain.Common;
using Library.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain.Entities;

public class Member : AuditableEntity
{
    [Display(Name = "Personnummer")]
    [Required(ErrorMessage = "Personnummer är obligatoriskt")]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "Personnummer måste vara exakt 13 tecken")]
    [RegularExpression(@"^\d{8}-\d{4}$", ErrorMessage = "Personnummer måste ha formatet: 19850315-1234")]
    public string SSN { get; set; } = string.Empty;

    [Display(Name = "Fullständigt namn")]
    [Required(ErrorMessage = "Namn är obligatoriskt")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Namnet måste vara mellan 2 och 100 tecken")]
    [RegularExpression(@"^[a-zA-ZåäöÅÄÖ\s\.\-\']+$", ErrorMessage = "Namnet får endast innehålla bokstäver, mellanslag, punkt, bindestreck och apostrof")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "E-postadress")]
    [Required(ErrorMessage = "E-postadress är obligatorisk")]
    [EmailAddress(ErrorMessage = "Ange en giltig e-postadress")]
    [StringLength(100, ErrorMessage = "E-postadressen får inte överstiga 100 tecken")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefonnummer")]
    [Phone(ErrorMessage = "Ange ett giltigt telefonnummer")]
    [StringLength(20, ErrorMessage = "Telefonnumret får inte överstiga 20 tecken")]
    [RegularExpression(@"^[\+]?[0-9\-\s\(\)]{10,20}$", ErrorMessage = "Telefonnummer måste innehålla 10-20 siffror och får innehålla +, -, mellanslag och parenteser")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Hemadress")]
    [StringLength(200, ErrorMessage = "Adressen får inte överstiga 200 tecken")]
    public string? Address { get; set; }

    [Display(Name = "Födelsedatum")]
    [DataType(DataType.Date, ErrorMessage = "Ange ett giltigt datum")]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Medlemskap sedan")]
    [Required(ErrorMessage = "Medlemskapsdatum är obligatoriskt")]
    [DataType(DataType.Date)]
    public DateTime MembershipStartDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Medlemsstatus")]
    [Required(ErrorMessage = "Medlemsstatus är obligatorisk")]
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;

    [Display(Name = "Maximalt antal lån")]
    [Required(ErrorMessage = "Maximalt antal lån är obligatoriskt")]
    [Range(1, 10, ErrorMessage = "Maximalt antal lån måste vara mellan 1 och 10")]
    public int MaxLoans { get; set; } = 3;

    [Display(Name = "Anteckningar")]
    [StringLength(500, ErrorMessage = "Anteckningarna får vara högst 500 tecken")]
    public string? Notes { get; set; }

    [ValidateNever]
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();

    // Beräknade egenskaper
    [NotMapped]
    [Display(Name = "Aktiva lån")]
    public int ActiveLoansCount => Loans?.Count(l => !l.ReturnDate.HasValue || l.ReturnDate == null) ?? 0;
    
    [NotMapped]
    [Display(Name = "Kan låna")]
    public bool CanBorrow => ActiveLoansCount < MaxLoans && Status == MembershipStatus.Active;

    [NotMapped]
    [Display(Name = "Försenade lån")]
    public int OverdueLoansCount => Loans?.Count(l => l.IsOverdue) ?? 0;

    [NotMapped]
    [Display(Name = "Totala avgifter")]
    public decimal TotalFees => Loans?.Sum(l => l.Fee) ?? 0;

    [NotMapped]
    [Display(Name = "Obetalda avgifter")]
    public decimal OutstandingFees => Loans?.Where(l => l.ReturnDate == null).Sum(l => l.Fee) ?? 0;

    [NotMapped]
    [Display(Name = "Ålder")]
    public int? Age => DateOfBirth.HasValue ?
        DateTime.Today.Year - DateOfBirth.Value.Year -
        (DateTime.Today.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0) : null;

    [NotMapped]
    [Display(Name = "Medlem i antal dagar")]
    public int MembershipDays => (DateTime.Today - MembershipStartDate.Date).Days;

    [NotMapped]
    [Display(Name = "Fullständig information")]
    public string FullDisplayName => $"{Name} ({Email})";

    // Validieringsmetoder
    public bool IsValidForLoan()
    {
        return Status == MembershipStatus.Active &&
               ActiveLoansCount < MaxLoans &&
               OutstandingFees <= 500; // Max 500 kr i obetalda avgifter
    }

    public bool HasOverdueBooks()
    {
        return Loans?.Any(l => l.IsOverdue) ?? false;
    }

    public DateTime? GetNextPaymentDue()
    {
        var overdueLoans = Loans?.Where(l => l.IsOverdue && l.Fee > 0);
        return overdueLoans?.Any() == true ?
               overdueLoans.Min(l => l.DueDate) : null;
    }
}

public class MemberStatistics
{
    [Display(Name = "Totalt antal medlemmar")]
    public int TotalMembers { get; set; }

    [Display(Name = "Aktiva medlemmar")]
    public int ActiveMembers { get; set; }

    [Display(Name = "Avstängda medlemmar")]
    public int SuspendedMembers { get; set; }

    [Display(Name = "Medlemmar med försenade böcker")]
    public int MembersWithOverdueBooks { get; set; }

    [Display(Name = "Totala obetalda avgifter")]
    public decimal TotalOutstandingFees { get; set; }

    [Display(Name = "Nya medlemmar denna månad")]
    public int NewMembersThisMonth { get; set; }

    [Display(Name = "Genomsnittligt antal lån per medlem")]
    public double AverageLoansPerMember { get; set; }

    [Display(Name = "Medlemmar som kan låna")]
    public int MembersWhoCanBorrow { get; set; }
}
