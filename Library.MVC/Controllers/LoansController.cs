using Library.Application.Interfaces;
using Library.Domain;
using Library.Domain.Utilities;
using Library.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.Common;

namespace Library.MVC.Controllers
{
#pragma warning disable

    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IBookCopyLoanService _bookCopyLoanService;
        private readonly IBookCopyService _bookCopyService;
        private readonly IMemberService _memberService;

        public LoansController(
            ILoanService loanService,
            IMemberService memberService,
            IBookCopyLoanService bookCopyLoanService,
            IBookCopyService bookCopyService
            )
        {
            _loanService = loanService;
            _bookCopyService = bookCopyService;
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
                BookCopies = await _bookCopyService.GetAllBookCopiesAsync(filter: null, orderBy: null, b => b.Details, b => b.BookCopyLoans),
                BookCopyLoans = null,
            };

            switch (satatus)
            {
                case "isActive":
                    model.Loans = await _loanService.GetAllLoansAsync(filter: x => x.ReturnDate == DateTime.MinValue, orderBy: null, l => l.Member, l => l.BookCopyLoans);
                    break;
                default:
                    model.Loans = await _loanService.GetAllLoansAsync(filter: null, orderBy: x => x.OrderByDescending(x => x.LoanId), l => l.Member, l => l.BookCopyLoans);
                    break;
            }

            // Get every loan.
            foreach (var loan in model.Loans)
            {
                model.Loan = await _loanService.GetLoanOrDefaultAsync(x => x.LoanId == loan.LoanId, includeProperties: "BookCopyLoans");
                model.BookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(filter: l => l.LoanId == loan.LoanId, orderBy: null, b => b.Loan, b => b.BookCopy);

                // Update fees instantly.
                if (model.Loan.ReturnDate == DateTime.MinValue)
                    model.Loan.SetFee(model.BookCopyLoans.Count);

            }

            model.BookCopies = await _bookCopyService.GetAllBookCopiesAsync(filter: null, orderBy: null, b => b.Details, b => b.BookCopyLoans);

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            IEnumerable<BookCopy> bookCopies = await _bookCopyService.GetAllBookCopiesAsync(filter: x => x.IsAvailable == true, orderBy: x => x.OrderBy(x => x.Details.Title), b => b.Details, b => b.BookCopyLoans);
            IEnumerable<Member> members = await _memberService.GetAllAsync();

            LoanViewModel model = new()
            {
                Copies = null,
                Members = members.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };

            for (var i = 0; i < LoanStatus.Max; i++)
            {
                model.Copies = bookCopies.Select(i => new SelectListItem
                {
                    Text = i.Details.Title.ToString(),
                    Value = i.BookCopyId.ToString()
                }).Distinct(new SelectListItemComparer()); // Use custom distinct to filter book title.
            }


            return View(model);
        }

        public async Task<IActionResult> Return(int id)
        {
            if (id == 0)
                return NotFound();

            try
            {
                IReadOnlyList<BookCopyLoan> BookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(x => x.LoanId == id);

                foreach (var bookCopy in BookCopyLoans)
                {
                    BookCopy copy = await _bookCopyService.GetByIdAsync(bookCopy.BookCopyId);
                    copy.IsAvailable = true;
                    await _bookCopyService.UpdateAsync(copy);
                }

                Loan loanToBeUpdated = await _loanService.GetByIdAsync(id);
                loanToBeUpdated.Fee = loanToBeUpdated.SetFee(BookCopyLoans.Count);
                loanToBeUpdated.SetReturnDate();

                await _loanService.UpdateAsync(loanToBeUpdated);

                TempData["Success"] = "Books returned successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                TempData["Error"] = "Something went wrong.";
                return View();
            }

        }

        #region API CALLS
        [HttpPost]
        public async Task<IActionResult> Create(LoanViewModel model, int id, int[] ids)
        {
            try
            {
                var member = await _memberService.GetMemberByIdAsync(id, includeProperties: true);
                model.Member = member;


                IEnumerable<BookCopy> bookCopies = await _bookCopyService.GetAllBookCopiesAsync(filter: b => b.IsAvailable == true, orderBy: null, b => b.Details, b => b.BookCopyLoans);

                //We need to check how many bookcopies the member already has borrowed.
                int borrowedCopies = 0;
                if (member.Loans != null)
                {
                    foreach (var loan in member.Loans)
                    {
                        // it should only take into account active loans
                        if (loan.ReturnDate == DateTime.MinValue)
                        {
                            IEnumerable<BookCopyLoan> bookCopyLoans = loan.BookCopyLoans.Where(l => l.LoanId == loan.LoanId);

                            if (bookCopyLoans != null)
                                borrowedCopies += bookCopyLoans.Count();
                        }
                    }
                }

                // User might not have more then the max amount of books to borrow
                // but we also need to prevent that the new loan entry doesn't go over the max amount of book copies.
                int bookCopiesToBorrow = 0;
                foreach (var loan in member.Loans)
                {
                    var bookCopyLoans = loan.BookCopyLoans.Where(l => l.LoanId == loan.LoanId).Select(x => x.BookCopy.Details.Title).Count();
                    if (bookCopyLoans != null)
                        bookCopiesToBorrow += bookCopyLoans;
                }

                // So let's say the member gets to borrow 3 books per loan and 9 books max in total
                // So the member can add loans untill the total amount of loans reaches over 9 books
                // We therefore count the total amount of books in order to decide if this loan request is valid
                // Loan count stands for the books already borrowed, the nr of copies to borrow is the number of books he/she now wants to borrow
                var totalBookCount = borrowedCopies + bookCopiesToBorrow;

                if (totalBookCount <= BookStatus.Max)
                {
                    // Create new loan
                    if (model.Loan == null)
                    {
                        model.Loan = new Loan()
                        {
                            MemberID = id,
                        };

                        await _loanService.AddAsync(model.Loan);


                        // Get the newly created loan from  the database.
                        var loanInDB = await _loanService.GetByIdAsync(model.Loan.LoanId);

                        for (var i = 0; i < ids.Length; i++)
                        {

                            BookCopyLoan bookCopyLoan = new()
                            {
                                LoanId = loanInDB.LoanId,
                                BookCopyId = ids[i],
                            };

                            await _bookCopyLoanService.AddAsync(bookCopyLoan);

                            // Update Copy 
                            var bookCopy = await _bookCopyService.GetBookCopyOrDefaultAsync(x => x.BookCopyId == bookCopyLoan.BookCopyId, "Details");
                            bookCopy.IsAvailable = false;

                            await _bookCopyService.UpdateAsync(bookCopy);
                        }

                    }
                    return Json(new { success = true, message = "Loan created successfully." });
                }
                else
                {
                    return Json(new { error = true, message = "You have exceeded the allowed amount for borrowed books." });
                }

            }
            catch (Exception)
            {
                return Json(new { error = true, message = "Something went wrong!" });
            }

            return Json(new { error = true, message = "An unexpected error occurred!" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (id != 0)
            {
                try
                {

                    // Before have to set bookcopies availability to true.
                    IReadOnlyList<BookCopyLoan> BookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(x => x.LoanId == id);

                    foreach (var bookCopy in BookCopyLoans)
                    {
                        BookCopy copy = await _bookCopyService.GetByIdAsync(bookCopy.BookCopyId);
                        copy.IsAvailable = true;
                        await _bookCopyService.UpdateAsync(copy);
                    }

                    // Then we proceed thid action we have to clean the BookCopyLoans attached to this loan ID.
                    _bookCopyLoanService.RemoveRange(BookCopyLoans);
                    await _loanService.DeleteAsync(id);

                    return Json(new { success = true, message = "Loan deleted successfully." });
                }
                catch (Exception)
                {
                    return Json(new { error = true, message = "Something went wrong." });
                }
            }
            return Json(new { error = true, message = "An unexpected error occurred." });
        }
        #endregion
    }
}