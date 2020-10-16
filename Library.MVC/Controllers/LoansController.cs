using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Library.Application.Interfaces;
using Library.Domain;
using Library.Infrastructure.Services;
using Library.MVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace Library.MVC.Controllers
{
    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IBookCopyService _bookcopyService;
        private readonly IMemberService _memberService;
        private readonly IBookCopyLoanService _bookCopyLoanService;

        public LoansController(ILoanService loanService, IBookCopyService bookcopyService, IMemberService memberService, IBookCopyLoanService bookCopyLoanService)
        {
            _loanService = loanService;
            _bookcopyService = bookcopyService;
            _memberService = memberService;
            _bookCopyLoanService = bookCopyLoanService;
        }

        public ActionResult Index(bool onlyActiveLoans)
        {
            var vm = new LoanIndexVm();

            if (onlyActiveLoans)
            {
                vm.Loans = _loanService.GetAllLoans().Where(l => l.ReturnDate < l.StartDate).ToList();
                vm.onlyActiveLoans = true;

            }
            else
            {
                vm.Loans = _loanService.GetAllLoans();
            }

            foreach (var loan in vm.Loans)
            {
                var bookCopies = loan.BookCopyLoans.Where(l => l.LoanId == loan.LoanId);

                if (loan.ReturnDate < loan.StartDate)
                {
                    loan.CalculateFee(bookCopies.Count());
                    _loanService.UpdateLoan(loan);
                }
            }


            return View(vm);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            return View();
        }

        public ActionResult Create()
        {
            var vm = new LoanCreateVm();

            vm.SelectListBookCopy1 = new SelectList(_bookcopyService.GetAllBookCopies().Where(b => b.IsAvailable == true), "BookCopyId", "BookCopyId");
            vm.SelectListBookCopy2 = new SelectList(_bookcopyService.GetAllBookCopies().Where(b => b.IsAvailable == true), "BookCopyId", "BookCopyId");
            vm.SelectListBookCopy3 = new SelectList(_bookcopyService.GetAllBookCopies().Where(b => b.IsAvailable == true), "BookCopyId", "BookCopyId");

            vm.SelectListMemberId = new SelectList(_memberService.GetAllmembers(), "Id", "Id");

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LoanCreateVm vm)
        {
            if (ModelState.IsValid)
            {
                var memberToCheck = _memberService.FindMember(vm.MemberId);

                // We need to check how many bookcopies the member already has borrowed
                int loanCount = 0;
                if (memberToCheck.Loans != null)
                {

                    foreach (var loan in memberToCheck.Loans)
                    {
                        // it should only take into account active loans
                        if (loan.ReturnDate < loan.StartDate)
                        {
                            var bookCopies = loan.BookCopyLoans.Where(l => l.LoanId == loan.LoanId);

                            if (bookCopies != null)
                            {
                                loanCount += bookCopies.Count();
                            }
                        }
                    }
                }

                // okay the user might not have more then the max amount of books to borrow
                // but we also need to prevent that the new loan entry doesn't go over the max amount

                vm.BookCopyID1 ??= 0;
                vm.BookCopyID2 ??= 0;
                vm.BookCopyID3 ??= 0;
                int[] bookCopyIDs = new[] { (int)vm.BookCopyID1, (int)vm.BookCopyID2, (int)vm.BookCopyID3 };

                var nrBookCopiesToBorrow = 0;

                for (var i = 0; i < bookCopyIDs.Length; i++)
                {
                    if (bookCopyIDs[i] != 0)
                    {
                        nrBookCopiesToBorrow++;
                    }
                }

                // So let's say the member gets to borrow 3 books per loan and 9 books max in total
                // So the member can add loans untill the total amount of loans reaches over 9 books
                // We therefore count the total amount of books in order to decide if this loan request is valid
                // Loan count stands for the books already borrowed, the nr of copies to borrow is the number of books he/she now wants to borrow
                var totalBookCountToCheck = loanCount + nrBookCopiesToBorrow;

                // If the totalBookCountToCheck is higher then 9 no new loan, we return the view. The member might need to take away books.
                // We also perform a check if the entered books to borrow are not the same - we also take into account the situation where the member borrows only 
                // one  book - results in two zero's, which should not lead to redirect to view...
                if (memberToCheck.Loans != null && totalBookCountToCheck > 9 || (vm.BookCopyID1 == vm.BookCopyID2 && vm.BookCopyID1 != 0 || vm.BookCopyID1 == vm.BookCopyID3 && vm.BookCopyID1 != 0 || vm.BookCopyID2 == vm.BookCopyID3 && vm.BookCopyID2 != 0) || (vm.BookCopyID1 == 0 && vm.BookCopyID2 == 0 && vm.BookCopyID3 == 0))
                {
                    // We need to declare the viewmodel again , because else we'll get an exception saying the vm the bookcopyId of vm is of value int and not type selectlist
                    vm.SelectListBookCopy1 = new SelectList(_bookcopyService.GetAllBookCopies().Where(b => b.IsAvailable == true), "BookCopyId", "BookCopyId");
                    vm.SelectListBookCopy2 = new SelectList(_bookcopyService.GetAllBookCopies().Where(b => b.IsAvailable == true), "BookCopyId", "BookCopyId");
                    vm.SelectListBookCopy3 = new SelectList(_bookcopyService.GetAllBookCopies().Where(b => b.IsAvailable == true), "BookCopyId", "BookCopyId");
                    vm.SelectListMemberId = new SelectList(_memberService.GetAllmembers(), "Id", "Id");

                    return View(vm);
                }
                else
                {
                    //Create new loan
                    var newLoan = new Loan();
                    newLoan.MemberID = Convert.ToInt32(vm.MemberId);

                    _loanService.AddLoan(newLoan);

                    var allLoansIncludingTheNewLoan = _loanService.GetAllLoans();
                    var allLoansOrdered = allLoansIncludingTheNewLoan.OrderBy(x => x.LoanId);
                    var lastItem = allLoansOrdered.Last();

                    var LoanId = lastItem.LoanId;

                    // Needs a sort in descending order here if the first index is 0 - if first bookcopy is not filled in but 2 and 3 for instance are
                    // If we don't do this it will generate exception concerning a problem in db 'bookcopyloans' 
                    if (bookCopyIDs[0] == 0)
                    {
                        Array.Sort(bookCopyIDs);
                        Array.Reverse(bookCopyIDs);
                    }

                    for (var i = 0; i < nrBookCopiesToBorrow; i++)
                    {
                        var newBookCopyLoan = new BookCopyLoan
                        {
                            LoanId = LoanId,
                            BookCopyId = bookCopyIDs[i]
                        };

                        _bookCopyLoanService.AddBookCopyLoan(newBookCopyLoan);

                        _bookcopyService.UpdateBookCopyDetails(bookCopyIDs[i]);
                    }

                    return RedirectToAction(nameof(Index));
                }
            }

            return RedirectToAction("Error", "Home", "");
        }

        public ActionResult Delete()
        {
            var vm = new LoanDeleteVm();
            var listOfActiveLoans = _loanService.GetAllLoans().Where(l => l.ReturnDate > l.StartDate).OrderBy(l => l.LoanId);
            vm.LoanIdList = new SelectList(listOfActiveLoans, "LoanId", "LoanId");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(LoanDeleteVm vm)
        {
            try
            {
                // Before we delete a loan we als clean the BookCopyLoans attached to this loan ID in the db
                _bookCopyLoanService.DeleteBookCopyLoansByLoanId(vm.LoanId);
                _loanService.DeleteLoanByLoanId(vm.LoanId);

                return RedirectToAction(nameof(Index));
            }
            // we use general exception to also catch the null exception 
            catch (Exception)
            {
                return RedirectToAction(nameof(Delete));
            }
        }

        public ActionResult Return(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }


            try
            {


                var bookCopiesToBeReturned = _bookCopyLoanService.GetBookCopiesOfALoan(id);

                foreach (var bookCopy in bookCopiesToBeReturned)
                {
                    _bookcopyService.UpdateBookCopyDetails(bookCopy.BookCopyId);
                }

                var loanToBeUpdated = _loanService.GetLoanByLoanId(id);

                loanToBeUpdated.SetReturnDate();

                _loanService.UpdateLoan(loanToBeUpdated);

                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                return View();
            }

        }

        public ActionResult AvailableBooks()
        {
            return View();
        }

    }
}