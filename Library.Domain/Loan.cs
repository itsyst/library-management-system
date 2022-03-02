using Library.Domain.Utilities;

namespace Library.Domain
{
    public class Loan
    {
        public int LoanId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public DateTime DueDate { get; set; }
        public double Fee { get; set; }

        public List<BookCopyLoan> BookCopyLoans { get; set; }

        public int MemberID { get; set; }
        public Member Member { get; set; }

        public Loan()
        {
            StartDate = DateTime.Now;
            DueDate = DateTime.Now.AddDays(FeeSettings.Days);
        }

        public double SetFee(int copies)
        {
            return Fee = (DateTime.Now > DueDate) ? (DateTime.Now - DueDate).Days * (FeeSettings.FeePerDayPerBook) * copies : 0;
        }

        public void SetReturnDate()
        {
            ReturnDate = DateTime.Now;
        }
    }
}

