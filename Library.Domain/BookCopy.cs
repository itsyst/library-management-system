using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Domain
{
    public class BookCopy
    {
        public int BookCopyId { get; set; }

        public int DetailsId { get; set; }
        public BookDetails Details { get; set; }

        public List<BookCopyLoan> BookCopyLoans { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}