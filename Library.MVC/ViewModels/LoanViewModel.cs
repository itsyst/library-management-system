using Library.Domain;
using Library.Domain.Utilities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.MVC.ViewModels
{
    public class LoanViewModel
    {

        [ValidateNever]
        public Loan? Loan { get; set; }

        [ValidateNever]
        public IReadOnlyList<Loan>? Loans { get; set; }


        [ValidateNever]
        public IReadOnlyList<BookCopy>? BookCopies { get; set; }

        [ValidateNever]
        public IReadOnlyList<BookCopyLoan>? BookCopyLoans { get; set; }




        [ValidateNever]
        public BookCopy? BookCopy { get; set; }       
 
        [ValidateNever]
        public IEnumerable<SelectListItem>? Copies { get; set; }

        [ValidateNever]
        public Member? Member { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem>? Members { get; set; }
    }
}
