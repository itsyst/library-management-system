using System.Data.Common;
using Library.Application.Interfaces;
using Library.Domain;
using Library.MVC.Models;
using Library.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Library.MVC.Controllers
{
#pragma warning disable

    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IBookCopyService _bookcopyService;
        private readonly IMemberService _memberService;
        private readonly IBookCopyLoanService _bookCopyLoanService;
        private readonly IBookCopyService _bookCopyService;

        public LoansController(
            ILoanService loanService,
            IBookCopyService bookcopyService,
            IMemberService memberService,
            IBookCopyLoanService bookCopyLoanService,
            IBookCopyService bookCopyService
            )
        {
            _loanService = loanService;
            _bookcopyService = bookcopyService;
            _memberService = memberService;
            _bookCopyLoanService = bookCopyLoanService;
            _bookCopyService = bookCopyService;
        }

        public async Task<IActionResult> Index(string satatus)
        {
            LoanViewModel model = new()
            {
                Loan = new(),
                Loans = null,
                BookCopies = await _bookcopyService.GetAllBookCopiesAsync(filter: null, orderBy: null, b => b.Details, b => b.BookCopyLoans),
                BookCopyLoans = null
            };

            switch (satatus)
            {
                case "isActive":
                    model.Loans = await _loanService.GetAllLoansAsync(filter: x => x.ReturnDate == DateTime.MinValue, orderBy:null, l => l.Member, l => l.BookCopyLoans);
                    break;
                default:
                    model.Loans = await _loanService.GetAllLoansAsync(filter: null, orderBy: null, l => l.Member, l => l.BookCopyLoans);
                    break;
            }

            // Get every loan.
            foreach (var loan in model.Loans)
            {
                model.Loan = await _loanService.GetLoanOrDefaultAsync(x => x.LoanId == loan.LoanId, includeProperties: "BookCopyLoans");
                model.BookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(filter: l => l.LoanId == loan.LoanId, orderBy: null, b => b.Loan, b => b.BookCopy);

                // Update free instantly.
                if (model.Loan.ReturnDate > model.Loan.DueDate || model.Loan.ReturnDate == DateTime.MinValue)
                    model.Loan.SetFee(model.BookCopyLoans.Count);
            }

            model.BookCopies = await _bookcopyService.GetAllBookCopiesAsync(filter: null, orderBy: null, b => b.Details, b => b.BookCopyLoans);



            return View(model);
        }
    }
}