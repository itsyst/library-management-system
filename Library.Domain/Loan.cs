using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Library.Domain
{
    public class Loan
    {
        public int LoanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public DateTime DueDate { get; set; }
        public int Fee { get; set; }

        public List<BookCopyLoan> BookCopyLoans { get; set; }

        public int MemberID { get; set; }
        public Member Member { get; set; }

        public Loan()
        {
            StartDate = DateTime.Now;
            DueDate = DateTime.Now.AddDays(14);
        }

        /// <summary>
        /// Calculates the fees of the bookcopies that are overdue 
        /// </summary>
        /// 
        public void CalculateFee(int nrBookcopies)
        {
            var FeePerDayPerBook = 2;

            //if (ReturnDate < StartDate)
            //{
            //int newFee;

            if (DateTime.Now > DueDate)
            {
                Fee = (DateTime.Now - DueDate).Days * FeePerDayPerBook * nrBookcopies;
            }
            else
                Fee = 0;

            //Fee = newFee;
            // Fee = (DateTime.Now > DueDate) ? (DateTime.Now - DueDate).Days * FeePerDayPerBook * nrBookcopies : 0;
            //}
            //else
            //{
           //     Fee = (ReturnDate > DueDate) ? (ReturnDate - DueDate).Days * FeePerDayPerBook * nrBookcopies : 0;
            //}
        }

        /// <summary>
        /// Sets the returndate 
        /// </summary>
        public void SetReturnDate()
        {
            ReturnDate = DateTime.Now;
        }
    }
}

