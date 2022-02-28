using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Library.Domain
{
    public class BookCopy
    {
        public int BookCopyId { get; set; }

        public int DetailsId { get; set; }
        [ValidateNever]
        public BookDetails? Details { get; set; }
        [ValidateNever]
        public List<BookCopyLoan>? BookCopyLoans { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}