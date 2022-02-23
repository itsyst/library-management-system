
using Library.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.Application.Interfaces;


namespace Library.MVC.Models
{
    public class LoanIndexVm

    {
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();

        public DateTime Now { get => DateTime.Now; }

        // Filter the active loans
        public bool onlyActiveLoans = false;
        public bool SelectedLoans = false;


    }
}

