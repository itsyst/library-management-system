using Library.Domain.Entities;

namespace Library.MVC.ViewModels;

public class BookHistoryViewModel
{
    public BookDetails Book { get; set; } = new();
    public IEnumerable<Loan> Loans { get; set; } = [];
    public IEnumerable<BookCopy> BookCopies { get; set; } = [];
    public BookHistoryStatistics Statistics { get; set; } = new();
}

public class BookHistoryStatistics
{
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public int PopularityScore { get; set; }
    public int UniqueBorrowers { get; set; }
    public int AverageLoanDays { get; set; }
    public DateTime FirstLoanDate { get; set; }
    public DateTime LastLoanDate { get; set; }
    public decimal TotalFees { get; set; }
    public int OverdueLoans { get; set; }
    public int ReturnedLoans { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}