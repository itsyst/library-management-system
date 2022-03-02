using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Domain
{
    public class BookCopyLoan
    {
        public int BookCopyId { get; set; }
        public BookCopy? BookCopy{ get; set; }

        public int LoanId { get; set; }
        public Loan? Loan { get; set; }

    }
}
