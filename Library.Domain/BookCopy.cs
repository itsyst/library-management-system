using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Library.Domain
{
    public class BookCopy
    {
        [Display(Name="Book Copy")]
        public int BookCopyId { get; set; }

        public int DetailsId { get; set; }
        [ValidateNever]
        public BookDetails? Details { get; set; }
        [ValidateNever]
        public List<BookCopyLoan>? BookCopyLoans { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}