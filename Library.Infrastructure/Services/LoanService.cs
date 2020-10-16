using Library.Domain;
using Library.Infrastructure.Persistence;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using Library.Application.Interfaces;
using System.Linq;

namespace Library.Infrastructure.Services
{
    public class LoanService : ILoanService
    {
        private readonly ApplicationDbContext _context;

        public LoanService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddLoan(Loan loan)
        {
            _context.Add(loan);
            _context.SaveChanges();
        }

        public ICollection<Loan> GetAllLoans()
        {
            return _context.Loans
                .Include(x => x.BookCopyLoans)
                .ThenInclude(x => x.BookCopy)
                .ThenInclude(x => x.Details)
                .Include(m => m.Member)
                .OrderBy(x => x.ReturnDate).ToList();
        }

        public void UpdateLoan(Loan loan)
        {
            _context.Update(loan);
            _context.SaveChanges();
        }

        public Loan GetLoanByLoanId(int loanId)
        {
            return _context.Loans.FirstOrDefault(l => l.LoanId == loanId);
        }

        public void DeleteLoanByLoanId(int id)
        {
            var loan = _context.Loans.SingleOrDefault(l => l.LoanId == id);

            _context.Loans.Remove(loan);

            _context.SaveChanges();
        }
    }
}
