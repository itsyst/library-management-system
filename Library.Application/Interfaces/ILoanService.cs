using Library.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Application.Interfaces
{
    public interface ILoanService
    {
        public ICollection<Loan> GetAllLoans();

        void AddLoan(Loan loan);
        
        void UpdateLoan(Loan loan);

        public Loan GetLoanByLoanId(int loanId);

        public void DeleteLoanByLoanId(int loanId);

    }
}
