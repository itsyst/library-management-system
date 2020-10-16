using Library.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Application.Interfaces
{
    public interface IBookCopyLoanService
    {
        void AddBookCopyLoan(BookCopyLoan bookCopyLoan);

        public List <BookCopyLoan> GetBookCopiesOfALoan(int loanId);

        public void DeleteBookCopyLoansByLoanId(int loanId);
}
}
