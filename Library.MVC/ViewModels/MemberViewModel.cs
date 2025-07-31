using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Library.MVC.ViewModels;

public class MemberViewModel
{
    public Member? Member { get; set; } = new();

    [ValidateNever]
    public IReadOnlyList<Member>? Members { get; set; } = [];

    [ValidateNever]
    public IReadOnlyList<Loan>? MemberLoans { get; set; } = [];

    // Dropdown-listor
    [ValidateNever]
    [Display(Name = "Medlemsstatus")]
    public IEnumerable<SelectListItem>? MembershipStatuses { get; set; }

    // Sök- och filteregenskaper
    [Display(Name = "Sökterm")]
    public string? SearchTerm { get; set; }

    [Display(Name = "Filtrera efter status")]
    public MembershipStatus? StatusFilter { get; set; }

    [Display(Name = "Startdatum")]
    [DataType(DataType.Date)]
    public DateTime? StartDateFilter { get; set; }

    [Display(Name = "Slutdatum")]
    [DataType(DataType.Date)]
    public DateTime? EndDateFilter { get; set; }

    // Statistik
    public MemberViewModelStatistics? Statistics { get; set; }

    // Paginering
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int PageSize { get; set; } = 12;
    public int TotalRecords { get; set; }

    // Hjälpegenskaper
    public bool HasActiveMembers => Members?.Any(m => m.Status == MembershipStatus.Active) ?? false;
    public bool HasSuspendedMembers => Members?.Any(m => m.Status == MembershipStatus.Suspended) ?? false;
    public decimal TotalOutstandingFees => Members?.Sum(m => m.OutstandingFees) ?? 0;
}

public class MemberViewModelStatistics
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

    [Display(Name = "Totalt antal lån")]
    public int TotalLoans { get; set; }

    [Display(Name = "Aktiva lån")]
    public int ActiveLoans { get; set; }

    [Display(Name = "Förfallna lån")]
    public int OverdueLoans { get; set; }

    [Display(Name = "Återlämnad idag")]
    public int ReturnedToday { get; set; }

    [Display(Name = "Totalt utlånade böcker")]
    public int TotalBooksBorrowed { get; set; }
}
