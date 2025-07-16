using System.ComponentModel.DataAnnotations;

namespace Library.Domain.Entities;

public class BookCopyLoan
{

    [Required(ErrorMessage = "Välj minst ett exemplar.")]
    public int BookCopyId { get; set; }
    public BookCopy? BookCopy { get; set; }


    [Required(ErrorMessage = "Välj minst ett exemplar.")]
    public int LoanId { get; set; }
    public Loan? Loan { get; set; }

}
