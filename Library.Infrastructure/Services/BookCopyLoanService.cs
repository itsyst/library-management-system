using Library.Application.Interfaces;
using Library.Domain;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library.Infrastructure.Services
{
    public class BookCopyLoanService : IBookCopyLoanService
    {
        private readonly ApplicationDbContext _context;

        public BookCopyLoanService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddBookCopyLoan(BookCopyLoan bookCopyLoan)
        {
            _context.Add(bookCopyLoan);
            _context.SaveChanges();
        }

        public List<BookCopyLoan> GetBookCopiesOfALoan(int loanId)
        {
            return _context.BookCopyLoans.Include(b => b.BookCopy).Where(b => b.LoanId == loanId).OrderBy(b => b.BookCopyId).ToList();
        }

        public void DeleteBookCopyLoansByLoanId(int loanId)
        {
            var bookCopyLoansToRemove = _context.BookCopyLoans.Where(b => b.LoanId == loanId);
            foreach (var bookCopyLoan in bookCopyLoansToRemove)
            {
                _context.BookCopyLoans.Remove(bookCopyLoan);
            }
            _context.SaveChanges();
        }
    }
}
