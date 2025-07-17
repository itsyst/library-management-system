using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Domain.Utilities;
using Library.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;
using System.Text;

namespace Library.MVC.Controllers;

#nullable disable
public class LoansController(
    ILoanService loanService,
    IMemberService memberService,
    IBookService bookService,
    IBookCopyLoanService bookCopyLoanService,
    IBookCopyService bookCopyService,
    ILogger<LoansController> logger) : Controller
{
    #region Fields and Constructor

    private readonly ILoanService _loanService = loanService;
    private readonly IBookCopyLoanService _bookCopyLoanService = bookCopyLoanService;
    private readonly IBookService _bookService = bookService;
    private readonly IBookCopyService _bookCopyService = bookCopyService;
    private readonly IMemberService _memberService = memberService;
    private readonly ILogger<LoansController> _logger = logger;

    #endregion

    #region Main CRUD Operations

    public async Task<IActionResult> Index(string search, string status, int? memberId,
        string dateRange, string sort = "dueDate")
    {
        try
        {
            // Normalisera sökterm för bättre matchning
            var normalizedSearch = search?.Trim().ToLowerInvariant();

            // Bygg upp filteruttryck
            Expression<Func<Loan, bool>> filter = loan => true;

            // Sökfilter med case-insensitive matching
            if (!string.IsNullOrEmpty(normalizedSearch))
            {
                // Kontrollera om söktermen är numerisk (för medlems-ID eller personnummer)
                bool isNumericSearch = normalizedSearch.All(char.IsDigit);

                if (isNumericSearch)
                {
                    // Sök på medlems-ID eller personnummer
                    filter = filter.AndAlso(x =>
                        x.MemberId.ToString().Contains(normalizedSearch) ||
                        x.Member.SSN.Contains(normalizedSearch) ||
                        x.Id.ToString().Contains(normalizedSearch));
                }
                else
                {
                    // Sök på namn eller boktitel
                    filter = filter.AndAlso(x =>
                        x.Member.Name.Contains(normalizedSearch) ||
                        x.Member.Email.Contains(normalizedSearch) ||
                        x.BookCopyLoans.Any(bcl =>
                            bcl.BookCopy.Details.Title.Contains(normalizedSearch) ||
                            bcl.BookCopy.Details.Author.Name.Contains(normalizedSearch) ||
                            bcl.BookCopy.Details.ISBN.Contains(normalizedSearch)));
                }
            }

            // Statusfilter
            if (!string.IsNullOrEmpty(status))
            {
                switch (status.ToLower())
                {
                    case "active":
                        filter = filter.AndAlso(x => x.ReturnDate == null && x.Status == LoanStatus.Active);
                        break;
                    case "overdue":
                        filter = filter.AndAlso(x => x.ReturnDate == null && x.DueDate < DateTime.UtcNow);
                        break;
                    case "returned":
                        filter = filter.AndAlso(x => x.ReturnDate != null && x.Status == LoanStatus.Returned);
                        break;
                    case "lost":
                        filter = filter.AndAlso(x => x.Status == LoanStatus.Lost);
                        break;
                    case "cancelled":
                        filter = filter.AndAlso(x => x.Status == LoanStatus.Cancelled);
                        break;
                }
            }

            // Medlemsfilter
            if (memberId.HasValue)
            {
                filter = filter.AndAlso(x => x.MemberId == memberId.Value);
            }

            // Datumintervallfilter
            if (!string.IsNullOrEmpty(dateRange))
            {
                var today = DateTime.Today;
                switch (dateRange.ToLower())
                {
                    case "today":
                        filter = filter.AndAlso(x => x.StartDate.Date == today);
                        break;
                    case "week":
                        var weekStart = today.AddDays(-(int)today.DayOfWeek);
                        filter = filter.AndAlso(x => x.StartDate.Date >= weekStart);
                        break;
                    case "month":
                        var monthStart = new DateTime(today.Year, today.Month, 1);
                        filter = filter.AndAlso(x => x.StartDate.Date >= monthStart);
                        break;
                    case "year":
                        var yearStart = new DateTime(today.Year, 1, 1);
                        filter = filter.AndAlso(x => x.StartDate.Date >= yearStart);
                        break;
                    default:
                        var monthStartDefault = new DateTime(today.Year, 1, 1);
                        filter = filter.AndAlso(x => x.StartDate.Date >= monthStartDefault);
                        break;
                }
            }

            // Modify to use double conversion for SQLite compatibility
            Func<IQueryable<Loan>, IOrderedQueryable<Loan>> orderBy = sort?.ToLower() switch
            {
                "duedate" => q => q.OrderByDescending(x => x.ReturnDate == null && x.DueDate < DateTime.UtcNow)
                  .ThenByDescending(x => x.DueDate),
                "startdate" => q => q.OrderByDescending(x => x.ReturnDate == null && x.DueDate < DateTime.UtcNow)
                  .ThenBy(x => x.StartDate),
                "startdate_desc" => q => q.OrderByDescending(x => x.ReturnDate == null && x.DueDate < DateTime.UtcNow)
                  .ThenByDescending(x => x.StartDate),
                "member" => q => q.OrderBy(x => x.Member.Name),
                "fee_desc" => q => q.OrderByDescending(x => x.ReturnDate == null && x.DueDate < DateTime.UtcNow)
                  .ThenByDescending(x => (double)x.Fee), // Cast to double for SQLite
                _ => q => q.OrderByDescending(x => x.DueDate),
            };

            // Hämta lån med inkluderade relationer - because sql does note support deciaml calucation in the database level we need to do it in memory
            var loans = await _loanService.GetAllLoansAsync(
                filter: filter,
                orderBy: orderBy,
                includeProperties: "Member,BookCopyLoans,BookCopyLoans.BookCopy," +
                                   "BookCopyLoans.BookCopy.Details,BookCopyLoans.BookCopy.Details.Author"
            );

            // Beräkna statistik
            var allLoans = await _loanService.GetAllLoansAsync();
            var statistics = new LoanStatistics
            {
                ActiveLoans = allLoans.Count(l => l.Status == LoanStatus.Active),
                OverdueLoans = allLoans.Count(l => l.Status == LoanStatus.Overdue || (l.ReturnDate == null && l.DueDate < DateTime.UtcNow)),
                ReturnedLoans = allLoans.Count(l => l.ReturnDate?.Date == DateTime.Today),
                TotalFees = allLoans.Where(l => l.Fee > 0 || l.Status != LoanStatus.Returned).Sum(l => l.Fee)
            };

            // Hämta alla medlemmar för dropdown
            var members = await _memberService.GetAllMembersAsync(
                filter: m => m.Status == MembershipStatus.Active,
                orderBy: m => m.OrderBy(x => x.Name));

            var model = new LoanViewModel
            {
                Loans = loans,
                Statistics = statistics
            };

            ViewBag.Members = members;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentMemberId = memberId;
            ViewBag.CurrentDateRange = dateRange;
            ViewBag.CurrentSort = sort;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid laddning av lån med sökparametrar: {Search}", search);
            TempData["Error"] = "Kunde inte ladda lån. Vänligen försök igen.";
            return View(new LoanViewModel { Loans = [] });
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var availableBookCopies = await _bookCopyService.GetAllBookCopiesAsync(
                filter: x => x.IsAvailable == true,
                orderBy: x => x.OrderBy(x => x.Details.Title),
                b => b.Details,
                b => b.BookCopyLoans);

            // Hämta alla medlemmar
            var members = await _memberService.GetAllAsync();

            // Hämta alla aktiva lån (eller använd en effektivare fråga om du har många lån)
            var allLoans = await _loanService.GetAllLoansAsync(l => l.Status != LoanStatus.Returned);

            LoanViewModel model = new()
            {
                Loan = new Loan(),
                Copies = availableBookCopies
                    .GroupBy(bc => bc.Details.Title)
                    .Select(g => new SelectListItem
                    {
                        Text = $"{g.Key} ({g.Count()} {(g.Count() > 1 ? "tillgängliga" : "tillgänglig")})",
                        Value = g.First().Id.ToString()
                    }),
                Members = members.Select(m =>
                {
                    var activeLoans = allLoans.Count(l => l.MemberId == m.Id);
                    return new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = $"{m.Name} ({activeLoans}/{m.MaxLoans} lån)"
                    };
                })
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid laddning av sidan för att skapa lån");
            TempData["Error"] = "Kunde inte ladda sidan för att skapa lån. Vänligen försök igen.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int memberId, int[] bookCopyIds)
    {
        try
        {
            if (bookCopyIds == null || bookCopyIds.Length == 0)
            {
                return Json(new { error = true, message = "Vänligen välj minst en bok att låna." });
            }

            // Hämta medlem med aktiva lån
            var member = await _memberService.GetMemberByIdAsync(memberId, includeProperties: true);
            if (member == null)
            {
                return Json(new { error = true, message = "Medlem hittades inte." });
            }

            if (member.Status != MembershipStatus.Active)
            {
                return Json(new { error = true, message = "Medlemskontot är inte aktivt." });
            }

            // Kontrollera om medlemmen kan låna fler böcker
            var activeLoansCount = member.ActiveLoansCount;
            var totalBooksAfterLoan = activeLoansCount + bookCopyIds.Length;

            if (totalBooksAfterLoan > member.MaxLoans)
            {
                return Json(new
                {
                    error = true,
                    message = $"Medlemmen kan endast låna {member.MaxLoans} böcker totalt. Har för närvarande {activeLoansCount} aktiva lån."
                });
            }

            // Verifiera att alla valda bokexemplar är tillgängliga
            var selectedBookCopies = new List<BookCopy>();
            foreach (var bookCopyId in bookCopyIds)
            {
                var bookCopy = await _bookCopyService.GetBookCopyOrDefaultAsync(
                    x => x.Id == bookCopyId,
                    "Details");

                if (bookCopy == null || !bookCopy.IsAvailable)
                {
                    return Json(new
                    {
                        error = true,
                        message = $"En eller flera valda böcker är inte längre tillgängliga."
                    });
                }
                selectedBookCopies.Add(bookCopy);
            }

            // Skapa nytt lån
            var newLoan = new Loan
            {
                MemberId = memberId,
                StartDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(FeeSettings.Days),
                Status = LoanStatus.Active,
                Fee = 0
            };

            await _loanService.AddAsync(newLoan);

            // Skapa bokexemplarlån och uppdatera tillgänglighet
            foreach (var bookCopyId in bookCopyIds)
            {
                var bookCopyLoan = new BookCopyLoan
                {
                    LoanId = newLoan.Id,
                    BookCopyId = bookCopyId
                };

                await _bookCopyLoanService.AddAsync(bookCopyLoan);

                // Markera bokexemplar som otillgängligt
                var bookCopy = await _bookCopyService.GetByIdAsync(bookCopyId);
                bookCopy.IsAvailable = false;
                await _bookCopyService.UpdateAsync(bookCopy);
            }

            var bookTitles = selectedBookCopies.Select(bc => bc.Details.Title).Distinct();
            return Json(new
            {
                success = true,
                message = $"Lån skapat framgångsrikt för {member.Name}. Böcker: {string.Join(", ", bookTitles)}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid skapande av lån för medlem: {MemberId}", memberId);
            return Json(new { error = true, message = "Ett oväntat fel inträffade när lånet skulle skapas." });
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
            return NotFound();

        try
        {
            var loan = await _loanService.GetLoanOrDefaultAsync(
                x => x.Id == id,
                includeProperties: "Member, BookCopyLoans,BookCopyLoans.BookCopy,BookCopyLoans.BookCopy.Details"
            );

            if (loan == null)
                return NotFound();

            var bookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(
                filter: l => l.LoanId == id,
                orderBy: null,
                b => b.BookCopy,
                b => b.BookCopy.Details);

            var allLoans = await _loanService.GetAllLoansAsync(
                    filter: l => l.MemberId == loan.MemberId
                );

            LoanViewModel model = new()
            {
                Loan = loan,
                BookCopyLoans = bookCopyLoans,
                Statistics = new LoanStatistics
                {
                    ActiveLoans = allLoans.Count(l => l.Status != LoanStatus.Returned),
                    OverdueLoans = allLoans.Count(l => l.Status == LoanStatus.Overdue || (l.ReturnDate == null && l.DueDate < DateTime.UtcNow)),
                    ReturnedLoans = allLoans.Count(l => l.ReturnDate.HasValue && l.ReturnDate.Value.Date == DateTime.Today),
                    TotalFees = allLoans.Where(l => l.Fee > 0 && l.Status != LoanStatus.Returned).Sum(l => l.Fee)
                }
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid laddning av lånedetaljer för Id: {LoanId}", id);
            return NotFound();
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
        {
            return Json(new { error = true, message = "Ogiltigt lån-Id." });
        }

        try
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null)
            {
                return Json(new { error = true, message = "Lånet hittades inte." });
            }

            if (loan.ReturnDate != null)
            {
                return Json(new { error = true, message = "Kan inte ta bort ett återlämnat lån." });
            }

            // Hämta alla bokexemplarlån för detta lån
            var bookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(
                x => x.LoanId == id);

            // Markera alla bokexemplar som tillgängliga
            foreach (var bookCopyLoan in bookCopyLoans)
            {
                var bookCopy = await _bookCopyService.GetByIdAsync(bookCopyLoan.BookCopyId);
                if (bookCopy != null)
                {
                    bookCopy.IsAvailable = true;
                    await _bookCopyService.UpdateAsync(bookCopy);
                }
            }

            // Ta bort bokexemplarlån
            _bookCopyLoanService.RemoveRange(bookCopyLoans);

            // Ta bort lånet
            await _loanService.DeleteAsync(id);

            return Json(new { success = true, message = "Lånet togs bort framgångsrikt." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid borttagning av lån-Id: {LoanId}", id);
            return Json(new { error = true, message = "Kunde inte ta bort lånet. Vänligen försök igen." });
        }
    }

    #endregion

    #region Edit Operations

    public async Task<IActionResult> Edit(int id)
    {
        if (id <= 0)
            return NotFound();

        try
        {
            var loan = await _loanService.GetLoanOrDefaultAsync(
                x => x.Id == id,
                includeProperties: "Member,BookCopyLoans");

            if (loan == null)
                return NotFound();

            var bookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(
                filter: l => l.LoanId == id,
                orderBy: null,
                b => b.BookCopy,
                b => b.BookCopy.Details,
                b => b.BookCopy.Details.Author);

            var members = await _memberService.GetAllMembersAsync(
                filter: m => m.Status == MembershipStatus.Active,
                orderBy: m => m.OrderBy(x => x.Name));

            var availableBooks = await _bookCopyService.GetAllBookCopiesAsync(
                filter: x => x.IsAvailable,
                orderBy: x => x.OrderBy(x => x.Details.Title),
                b => b.Details,
                b => b.Details.Author);

            // Hämta alla lån och beräkna i minnet
            var allLoans = await _loanService.GetAllLoansAsync(
                    filter: l => l.MemberId == loan.MemberId
                );

            LoanViewModel model = new()
            {
                Loan = loan,
                BookCopyLoans = bookCopyLoans,
                Loans = allLoans,
                Statistics = new LoanStatistics
                {
                    ActiveLoans = allLoans.Count(l => l.Status != LoanStatus.Active && l.Id != id),
                    OverdueLoans = allLoans.Count(l => l.Status == LoanStatus.Overdue && l.Id != id),
                    ReturnedLoans = allLoans.Count(l =>
                        l.ReturnDate.HasValue &&
                        l.ReturnDate.Value.Date == DateTime.Today && l.Id != id),
                    TotalFees = allLoans.Where(l => l.Fee > 0 && l.Status != LoanStatus.Returned && l.Id != id)
                                      .Sum(l => l.Fee)
                }
            };

            ViewBag.Members = members;
            ViewBag.AvailableBooks = availableBooks;
            ViewBag.CurrentBooks = bookCopyLoans;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid laddning av redigeringssida för lån-ID: {LoanId}", id);
            TempData["Error"] = "Kunde inte ladda lånet för redigering.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("/Loans/Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind(Prefix = "Loan")] Loan model)
    {
        if (id <= 0 || model == null || model == null)
        {
            _logger.LogWarning("Invalid request for Edit: ID {Id}, Model null: {IsNull}", id, model == null);
            return BadRequest(new { success = false, message = "Ogiltig förfrågan." });
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
            _logger.LogWarning("Model validation errors: {Errors}", errors);
            return Json(new { success = false, message = "Valideringsfel.", errors });
        }

        try
        {
            var existingLoan = await _loanService.GetLoanOrDefaultAsync(
                x => x.Id == id,
                includeProperties: "BookCopyLoans,BookCopyLoans.BookCopy,BookCopyLoans.BookCopy.Details");

            if (existingLoan == null)
                return NotFound(new { success = false, message = "Lånet hittades inte." });

            // Update properties from model
            existingLoan.MemberId = model.MemberId;
            existingLoan.StartDate = model.StartDate;
            existingLoan.DueDate = model.DueDate;
            existingLoan.Status = model.Status;
            existingLoan.Fee = model.Fee;
            existingLoan.Notes = model.Notes?.Trim();
            existingLoan.UpdatedDate = DateTime.UtcNow;

            // Handle returned status
            if (model.Status == LoanStatus.Returned && !existingLoan.ReturnDate.HasValue)
            {
                existingLoan.ReturnDate = DateTime.UtcNow;
                await UpdateBookCopyAvailability(existingLoan.Id, true);
            }

            await _loanService.UpdateAsync(existingLoan);

            var redirectUrl = Url.Action("Details", "Loans", new { id = existingLoan.Id }, Request.Scheme);

            return Json(new
            {
                success = true,
                message = $"Lån #{existingLoan.Id} uppdaterat framgångsrikt.",
                redirectUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loan ID: {LoanId}", id);
            return StatusCode(500, new { success = false, message = "Ett internt fel inträffade." });
        }
    }

    #endregion

    #region History Operations

    public async Task<IActionResult> History(int id)
    {
        if (id <= 0)
            return NotFound();

        try
        {
            var loan = await _loanService.GetLoanOrDefaultAsync(
                x => x.Id == id,
                includeProperties: "Member,BookCopyLoans");

            if (loan == null)
                return NotFound();

            // Hämta alla bokexemplarlån med detaljerad information
            var bookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(
                filter: l => l.LoanId == id,
                orderBy: null,
                b => b.BookCopy,
                b => b.BookCopy.Details,
                b => b.BookCopy.Details.Author);

            // Hämta relaterade lån för samma medlem (lånehistorik)
            var memberLoanHistory = await _loanService.GetAllLoansAsync(
                filter: l => l.MemberId == loan.MemberId && l.Id != id,
                orderBy: x => x.OrderByDescending(l => l.StartDate),
                l => l.Member,
                l => l.BookCopyLoans);

            // Hämta relaterade lån för samma böcker (boklänehistorik)
            var bookIds = bookCopyLoans.Select(bcl => bcl.BookCopy.DetailsId).Distinct().ToList();
            var bookLoanHistory = new List<Loan>();

            foreach (var bookId in bookIds)
            {
                var bookLoans = await _loanService.GetAllLoansAsync(
                    filter: l => l.Id != id && l.BookCopyLoans.Any(bcl => bcl.BookCopy.DetailsId == bookId),
                    orderBy: x => x.OrderByDescending(l => l.StartDate),
                    l => l.Member,
                    l => l.BookCopyLoans);
                bookLoanHistory.AddRange(bookLoans);
            }

            // Ta bort dubbletter och sortera
            bookLoanHistory = [.. bookLoanHistory.Distinct().OrderByDescending(l => l.StartDate)];

            // Beräkna historikstatistik
            var historyStats = new LoanHistoryStatistics
            {
                TotalMemberLoans = memberLoanHistory.Count + 1, // +1 för nuvarande lån
                MemberActiveLoans = memberLoanHistory.Count(l => l.Status == LoanStatus.Active),
                MemberTotalFees = memberLoanHistory.Sum(l => l.Fee) + loan.Fee,
                TotalBooksLoaned = bookCopyLoans.Count,
                AverageBookPopularity = bookLoanHistory.Count / Math.Max(bookIds.Count, 1),
                MemberSince = loan.Member?.MembershipStartDate ?? DateTime.Now
            };

            LoanViewModel model = new()
            {
                Loan = loan,
                BookCopyLoans = bookCopyLoans,
                Statistics = null // Vi använder en annan statistikmodell för historik
            };

            ViewBag.MemberLoanHistory = memberLoanHistory.Take(10); // Senaste 10 lånen av medlemmen
            ViewBag.BookLoanHistory = bookLoanHistory.Take(15); // Senaste 15 lånen för dessa böcker
            ViewBag.HistoryStatistics = historyStats;
            ViewBag.BookCopyLoans = bookCopyLoans;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid laddning av lånehistorik för ID: {LoanId}", id);
            TempData["Error"] = "Kunde inte ladda lånehistorik.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpGet]
    public async Task<IActionResult> MemberHistory(int memberId, int page = 1, int pageSize = 10)
    {
        try
        {
            var member = await _memberService.GetByIdAsync(memberId);
            if (member == null)
                return NotFound();

            var memberLoans = await _loanService.GetAllLoansAsync(
                filter: l => l.MemberId == memberId,
                orderBy: x => x.OrderByDescending(l => l.StartDate),
                l => l.Member,
                l => l.BookCopyLoans);

            // Beräkna sidnumrering
            var totalItems = memberLoans.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedLoans = memberLoans.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new LoanViewModel
            {
                Loans = pagedLoans,
                Statistics = new LoanStatistics
                {
                    ActiveLoans = memberLoans.Count(l => l.Status == LoanStatus.Active),
                    OverdueLoans = memberLoans.Count(l => l.Status == LoanStatus.Overdue),
                    TotalFees = memberLoans.Sum(l => l.Fee),
                    ReturnedLoans = memberLoans.Count(l =>
                        l.ReturnDate.HasValue &&
                        l.ReturnDate.Value.Date == DateTime.Today)
                }
            };

            ViewBag.Member = member;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View("MemberHistory", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid laddning av medlemshistorik för ID: {MemberId}", memberId);
            TempData["Error"] = "Kunde inte ladda medlemshistorik.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> BookHistory(int bookId, int page = 1, int pageSize = 10)
    {
        try
        {
            var book = await _bookService.GetByIdAsync(bookId);
            if (book == null)
                return NotFound();

            var bookLoans = await _loanService.GetAllLoansAsync(
                filter: l => l.BookCopyLoans.Any(bcl => bcl.BookCopy.DetailsId == bookId),
                orderBy: x => x.OrderByDescending(l => l.StartDate),
                l => l.Member,
                l => l.BookCopyLoans);

            // Beräkna sidnumrering
            var totalItems = bookLoans.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedLoans = bookLoans.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new LoanViewModel
            {
                Loans = pagedLoans,
                Statistics = new LoanStatistics
                {
                    ActiveLoans = bookLoans.Count(l => l.Status == LoanStatus.Active),
                    OverdueLoans = bookLoans.Count(l => l.Status == LoanStatus.Overdue),
                    TotalFees = bookLoans.Sum(l => l.Fee),
                    ReturnedLoans = bookLoans.Count(l =>
                        l.ReturnDate.HasValue &&
                        l.ReturnDate.Value.Date == DateTime.Today)
                }
            };

            ViewBag.Book = book;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View("BookHistory", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid laddning av bokhistorik för ID: {BookId}", bookId);
            TempData["Error"] = "Kunde inte ladda bokhistorik.";
            return RedirectToAction("Index", "Books");
        }
    }
 
    #endregion

    #region Loan Operations

    [HttpPost]
    public async Task<IActionResult> Return(int id)
    {
        if (id <= 0)
            return Json(new { success = false, message = "Ogiltigt lån-ID." });

        try
        {
            var loan = await _loanService.GetLoanOrDefaultAsync(
                x => x.Id == id,
                includeProperties: "BookCopyLoans,Member");

            if (loan == null)
                return Json(new { success = false, message = "Lånet hittades inte." });

            if (loan.ReturnDate != null)
                return Json(new { success = false, message = "Detta lån har redan återlämnats." });

            // Hämta alla bokexemplar för detta lån
            var bookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(
                x => x.LoanId == id,
                orderBy: null,
                bcl => bcl.BookCopy);

            // Markera alla bokexemplar som tillgängliga
            foreach (var bookCopyLoan in bookCopyLoans)
            {
                var bookCopy = await _bookCopyService.GetByIdAsync(bookCopyLoan.BookCopyId);
                if (bookCopy != null)
                {
                    bookCopy.IsAvailable = true;
                    await _bookCopyService.UpdateAsync(bookCopy);
                }
            }

            // Uppdatera lånestatus och beräkna slutlig avgift
            loan.MarkAsReturned();
            await _loanService.UpdateAsync(loan);

            var message = $"Böcker återlämnade framgångsrikt för {loan.Member?.Name}.";
            if (loan.Fee > 0)
                message += $" Förseningsavgift: {FormatSwedishCurrency(loan.Fee)}";

            return Json(new { success = true, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid återlämning av lån-ID: {LoanId}", id);
            return Json(new { success = false, message = "Kunde inte återlämna böcker. Vänligen försök igen." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Extend(int id, int days = 7)
    {
        try
        {
            // Validera input
            if (id <= 0)
            {
                return Json(new { success = false, message = "Ogiltigt lån-ID." });
            }

            if (days < 1 || days > 30)
            {
                return Json(new { success = false, message = "Antal dagar måste vara mellan 1 och 30." });
            }

            // Hämta lånet med relaterade data
            var loan = await _loanService.GetLoanOrDefaultAsync(
                x => x.Id == id,
                includeProperties: "Member,BookCopyLoans");

            if (loan == null)
            {
                return Json(new { success = false, message = "Lånet hittades inte." });
            }

            // Kontrollera om lånet kan förlängas
            if (loan.ReturnDate.HasValue)
            {
                return Json(new { success = false, message = "Kan inte förlänga ett återlämnat lån." });
            }

            if (loan.Status == LoanStatus.Lost || loan.Status == LoanStatus.Cancelled)
            {
                return Json(new { success = false, message = "Kan inte förlänga ett förlorat eller avbrutet lån." });
            }

            // Kontrollera medlemsrättigheter
            if (loan.Member?.Status != MembershipStatus.Active)
            {
                return Json(new { success = false, message = "Medlemmen är inte aktiv och kan inte förlänga lån." });
            }

            // Beräkna nytt förfallodatum
            var oldDueDate = loan.DueDate;
            var newDueDate = loan.DueDate.AddDays(days);

            // Kontrollera om förlängningen är rimlig (max 90 dagar från original startdatum)
            var totalLoanDays = (newDueDate - loan.StartDate).Days;
            if (totalLoanDays > 90)
            {
                return Json(new { success = false, message = "Lånet kan inte förlängas mer än 90 dagar totalt." });
            }

            // Uppdatera lånet
            loan.DueDate = newDueDate;
            loan.Status = LoanStatus.Active; // Säkerställ att status är aktiv
            loan.UpdatedDate = DateTime.UtcNow;

            // Om lånet var försenat, nollställ inte avgiften men ändra status
            if (loan.Status == LoanStatus.Overdue)
            {
                loan.Status = LoanStatus.Active;
            }

            await _loanService.UpdateAsync(loan);

            // Logga förlängningen
            _logger.LogInformation("Lån #{LoanId} förlängt med {Days} dagar av användare. Nytt förfallodatum: {NewDueDate}",
                id, days, newDueDate.ToString("yyyy-MM-dd"));

            return Json(new
            {
                success = true,
                message = $"Lånet förlängt med {days} dagar framgångsrikt.",
                data = new
                {
                    oldDueDate = oldDueDate.ToString("yyyy-MM-dd"),
                    newDueDate = newDueDate.ToString("yyyy-MM-dd"),
                    totalDays = days,
                    newStatus = GetSwedishLoanStatus(loan.Status)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid förlängning av lån-ID: {LoanId} med {Days} dagar", id, days);
            return Json(new { success = false, message = "Ett oväntat fel inträffade vid förlängning av lånet." });
        }
    }

    #endregion

    #region Book Copy Management

    [HttpPost("/Loans/AddBookToLoan/{loanId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddBookToLoan(int loanId, [FromBody] AddBookToLoanRequest request)
    {
        try
        {
            var loan = await _loanService.GetLoanOrDefaultAsync(
                l => l.Id == loanId,
                includeProperties: "BookCopyLoans,BookCopyLoans.BookCopy,BookCopyLoans.BookCopy.Details"
            );

            if (loan == null)
                return Json(new { success = false, message = "Lånet hittades inte." });

            if (loan.BookCopyLoans.Count > 5)
                return Json(new { success = false, message = "You riched." });

            if (loan.Status == LoanStatus.Returned)
                return Json(new { success = false, message = "Kan inte lägga till böcker i ett återlämnat lån." });

            // Kontrollera om boken redan finns i detta lån
            if (loan.BookCopyLoans.Any(bcl => bcl.BookCopyId == request.BookCopyId))
            {
                return Json(new { success = false, message = "Denna bok finns redan i lånet." });
            }

            // Lägg till bok i lånet
            var bookCopyLoan = new BookCopyLoan
            {
                LoanId = loanId,
                BookCopyId = request.BookCopyId,
                BookCopy = await _bookCopyService.GetByIdAsync(request.BookCopyId) ?? null
            };

            loan.BookCopyLoans.Add(bookCopyLoan);
            await _loanService.UpdateAsync(loan);

            // Markera bokexemplar som otillgängligt
            var bookCopy = bookCopyLoan.BookCopy ?? await _bookCopyService.GetByIdAsync(request.BookCopyId);
            if (bookCopy != null)
            {
                bookCopy.IsAvailable = false;
                await _bookCopyService.UpdateAsync(bookCopy);
            }

            return Json(new
            {
                success = true,
                message = $"Boken '{bookCopyLoan.BookCopy?.Details?.Title}' tillagd i lånet framgångsrikt."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid tillägg av bok till lån {LoanId}", loanId);
            return Json(new { success = false, message = "Ett fel inträffade när boken skulle läggas till." });
        }
    }

    [HttpDelete("/Loans/RemoveBookFromLoan/{loanId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveBookFromLoan(int loanId, [FromBody] RemoveBookFromLoanRequest request)
    {
        try
        {
            var loan = await _loanService.GetLoanOrDefaultAsync(
                l => l.Id == loanId,
                includeProperties: "BookCopyLoans,BookCopyLoans.BookCopy,BookCopyLoans.BookCopy.Details"
            );

            if (loan == null)
                return Json(new { success = false, message = "Lånet hittades inte." });

            if (loan.Status == LoanStatus.Returned)
                return Json(new { success = false, message = "Kan inte ta bort böcker från ett återlämnat lån." });

            var bookCopyLoan = loan.BookCopyLoans.FirstOrDefault(bcl => bcl.BookCopyId == request.BookCopyId);

            if (bookCopyLoan == null)
            {
                return Json(new { success = false, message = "Boken finns inte i detta lån." });
            }

            // Ta bort bok från lånet
            loan.BookCopyLoans.Remove(bookCopyLoan);
            await _loanService.UpdateAsync(loan);

            // Markera bokexemplar som tillgängligt
            var bookCopy = bookCopyLoan.BookCopy ?? await _bookCopyService.GetByIdAsync(request.BookCopyId);
            if (bookCopy != null)
            {
                bookCopy.IsAvailable = true;
                await _bookCopyService.UpdateAsync(bookCopy);
            }

            return Json(new { success = true, message = "Bok borttagen från lånet framgångsrikt." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid borttagning av bok från lån {LoanId}", loanId);
            return Json(new { success = false, message = "Ett fel inträffade när boken skulle tas bort." });
        }
    }

    #endregion

    #region API Endpoints

    [HttpGet]
    public async Task<IActionResult> GetAvailableBooks()
    {
        try
        {
            var bookCopies = await _bookCopyService.GetAllBookCopiesAsync(
                filter: x => x.IsAvailable == true,
                orderBy: x => x.OrderBy(x => x.Details.Title),
                b => b.Details,
                b => b.Details.Author);

            var result = bookCopies.Select(bc => new
            {
                copyId = bc.Id,
                title = bc.Details.Title,
                author = bc.Details.Author?.Name ?? "Okänd",
                isbn = bc.Details.ISBN,
                condition = bc.Condition.ToString(),
                location = bc.Location ?? "Ej specificerad",
                imageBinary = bc.Details.ImageBinary ?? "/uploads/9780555816023.png"
            });

            return Json(new { success = true, books = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av tillgängliga böcker");
            return Json(new { success = false, message = "Kunde inte ladda tillgängliga böcker." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> SearchAvailable(string term)
    {
        try
        {
            var normalizedTerm = term?.Trim().ToLower(); // Normalize search term

            var bookCopies = await _bookCopyService.GetAllBookCopiesAsync(
                filter: x => x.IsAvailable == true &&
                              (string.IsNullOrEmpty(normalizedTerm) ||   
                              (x.Details.Title != null && x.Details.Title.ToLower().Contains(normalizedTerm)) ||
                              (x.Details.Author != null && x.Details.Author.Name != null &&
                               x.Details.Author.Name.Contains(normalizedTerm)) ||
                              (x.Details.ISBN != null && x.Details.ISBN.ToLower().Contains(normalizedTerm))),
                orderBy: x => x.OrderBy(x => x.Details.Title),
                b => b.Details,
                b => b.Details.Author
            );

            var result = bookCopies.Select(bc => new
            {
                copyId = bc.Id,
                title = bc.Details.Title,
                author = bc.Details.Author?.Name ?? "Okänd",
                isbn = bc.Details.ISBN,
                condition = bc.Condition.ToString(),
                location = bc.Location ?? "Ej specificerad",
                imageBinary = bc.Details.ImageBinary ?? "/uploads/9780555816023.png"
            });

            return Json(new { success = true, books = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av tillgängliga böcker");
            return Json(new { success = false, message = "Kunde inte ladda tillgängliga böcker." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableBookCopies(string search = "")
    {
        try
        {
            var bookCopies = await _bookCopyService.GetAllBookCopiesAsync(
                filter: x => x.IsAvailable == true &&
                           (string.IsNullOrEmpty(search) || x.Details.Title.Contains(search)),
                orderBy: x => x.OrderBy(x => x.Details.Title),
                b => b.Details);

            var result = bookCopies.Select(bc => new
            {
                id = bc.Id,
                title = bc.Details.Title,
                author = bc.Details.Author?.Name,
                isbn = bc.Details.ISBN,
                condition = bc.Condition.ToString(),
                location = bc.Location
            });

            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av tillgängliga bokexemplar");
            return Json(new { error = true, message = "Kunde inte ladda tillgängliga böcker." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMemberDetails(int memberId)
    {
        try
        {
            var member = await _memberService.GetMemberByIdAsync(memberId, includeProperties: true);
            if (member == null)
            {
                return Json(new { error = true, message = "Medlem hittades inte." });
            }

            var result = new
            {
                success = true,
                member = new
                {
                    id = member.Id,
                    name = member.Name,
                    email = member.Email,
                    status = member.Status.ToString(),
                    activeLoans = member.ActiveLoansCount,
                    maxLoans = member.MaxLoans,
                    canBorrow = member.CanBorrow
                }
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av medlemsdetaljer för ID: {MemberId}", memberId);
            return Json(new { error = true, message = "Kunde inte ladda medlemsdetaljer." });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateNotes(int id, [FromBody] UpdateNotesRequest request)
    {
        try
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null)
                return Json(new { success = false, message = "Lånet hittades inte." });

            loan.Notes = request.Notes?.Trim();
            loan.UpdatedDate = DateTime.UtcNow;

            await _loanService.UpdateAsync(loan);

            return Json(new { success = true, message = "Anteckningar uppdaterade framgångsrikt." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid uppdatering av anteckningar för lån {LoanId}", id);
            return Json(new { success = false, message = "Kunde inte uppdatera anteckningar." });
        }
    }

    [HttpPost("Loans/CalculateFee/{id}")]
    public async Task<IActionResult> CalculateFee(int id)
    {
        try
        {
            // Include BookCopyLoans for TotalBooks calculation
            var loan = await _loanService.GetLoanOrDefaultAsync(
                x => x.Id == id,
                includeProperties: "BookCopyLoans"
            );

            if (loan == null)
                return Json(new { success = false, message = "Lånet hittades inte." });

            // Use consistent timezone (e.g., CEST) for calculations
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));

            // Normalize DueDate to same timezone if needed (assume it's stored in UTC)
            var dueDateLocal = TimeZoneInfo.ConvertTimeFromUtc(loan.DueDate, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));

            var daysOverdue = dueDateLocal < now ? (int)(now - dueDateLocal).TotalDays : 0;

            if (daysOverdue > 0)
            {
                // Explicitly calculate TotalBooks
                var totalBooks = loan.BookCopyLoans?.Count ?? 0;
                if (totalBooks == 0)
                {
                    _logger.LogWarning("Loan {LoanId} has 0 books, fee will be 0.", id);
                    return Json(new { success = true, message = "Ingen avgift - inga böcker i lånet.", fee = 0m, formattedFee = "0,00 kr", daysOverdue });
                }

                var calculatedFee = Math.Min(daysOverdue * FeeSettings.FeePerDayPerBook * totalBooks, FeeSettings.MaxFee);

                loan.Fee = calculatedFee;
                loan.Status = LoanStatus.Overdue;
                await _loanService.UpdateAsync(loan);

                var formattedFee = FormatSwedishCurrency(calculatedFee);

                return Json(new
                {
                    success = true,
                    message = "Avgift beräknad framgångsrikt.",
                    fee = calculatedFee,
                    formattedFee,
                    daysOverdue
                });
            }
            else
            {
                return Json(new
                {
                    success = true,
                    message = "Ingen avgift - lånet är inte försenat.",
                    fee = 0m,
                    formattedFee = "0,00 kr",
                    daysOverdue = 0
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid beräkning av avgift för lån {LoanId}", id);
            return Json(new { success = false, message = "Kunde inte beräkna avgift." });
        }
    }


    [HttpPost]
    public async Task<IActionResult> UpdateFees()
    {
        try
        {
            var activeLoans = await _loanService.GetAllLoansAsync(
                filter: l => l.ReturnDate == null,
                orderBy: null,
                l => l.BookCopyLoans);

            int updatedCount = 0;
            foreach (var loan in activeLoans)
            {
                if (loan.IsOverdue)
                {
                    var previousFee = loan.Fee;
                    loan.Fee = loan.CalculateFee();
                    loan.Status = LoanStatus.Overdue;

                    if (loan.Fee != previousFee)
                    {
                        await _loanService.UpdateAsync(loan);
                        updatedCount++;
                    }
                }
            }

            return Json(new
            {
                success = true,
                message = $"Uppdaterade avgifter för {updatedCount} försenade lån."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid uppdatering av låneavgifter");
            return Json(new { error = true, message = "Kunde inte uppdatera avgifter." });
        }
    }

    #endregion

    #region File Operations

    [HttpGet]
    public async Task<IActionResult> ExportToCsv(string status = "", string search = "", int? memberId = null)
    {
        try
        {
            // Hämta lån baserat på filter
            var loans = await GetFilteredLoans(status, search, memberId);

            var csv = new StringBuilder();

            // CSV-huvud på svenska
            csv.AppendLine("Lån-ID,Medlemsnamn,Personnummer,Status,Startdatum,Förfallodatum,Återlämningsdatum,Antal böcker,Förseningsavgift,Dagar försenat,Anteckningar");

            // Lägg till data
            foreach (var loan in loans)
            {
                csv.AppendLine($"{loan.Id}," +
                              $"\"{loan.Member?.Name ?? "Okänd"}\"," +
                              $"\"{loan.Member?.SSN ?? ""}\"," +
                              $"\"{GetSwedishStatusName(loan.Status.ToString())}\"," +
                              $"\"{loan.StartDate:yyyy-MM-dd}\"," +
                              $"\"{loan.DueDate:yyyy-MM-dd}\"," +
                              $"\"{(loan.ReturnDate?.ToString("yyyy-MM-dd") ?? "")}\"," +
                              $"{loan.TotalBooks}," +
                              $"{loan.Fee:F2}," +
                              $"{(loan.IsOverdue ? loan.DaysOverdue : 0)}," +
                              $"\"{loan.Notes?.Replace("\"", "\"\"") ?? ""}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"lån_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid CSV-export av lån");
            TempData["Error"] = "Misslyckades med att exportera lån till CSV.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateReport(string status = "", string search = "", int? memberId = null)
    {
        try
        {
            // Hämta lån och statistik
            var loans = await GetFilteredLoans(status, search, memberId);
            var allLoans = await _loanService.GetAllLoansAsync();

            // Beräkna statistik
            var stats = CalculateLoanStatistics(allLoans);

            // Generera rapport
            var report = GenerateLoansReportContent(loans, stats, status, search, memberId);
            var bytes = Encoding.UTF8.GetBytes(report);

            var fileName = $"lånerapport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            return File(bytes, "text/plain", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid generering av lånerapport");
            TempData["Error"] = "Misslyckades med att generera rapport.";
            return RedirectToAction(nameof(Index));
        }
    }

    private static string GenerateLoanReportContent(Loan loan, IEnumerable<BookCopyLoan> bookCopyLoans)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine($"LÅNERAPPORT - #{loan.Id}");
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine();

        sb.AppendLine("LÅNEINFORMATION:");
        sb.AppendLine($"Lån-ID: #{loan.Id}");
        sb.AppendLine($"Medlem: {loan.Member?.Name}");
        sb.AppendLine($"Medlems personnummer: {loan.Member?.SSN}");
        sb.AppendLine($"Status: {loan.Status}");
        sb.AppendLine($"Startdatum: {loan.StartDate:yyyy-MM-dd}");
        sb.AppendLine($"Återlämningsdatum: {loan.DueDate:yyyy-MM-dd}");
        if (loan.ReturnDate.HasValue)
            sb.AppendLine($"Återlämnat datum: {loan.ReturnDate.Value:yyyy-MM-dd}");
        sb.AppendLine($"Förseningsavgift: {FormatSwedishCurrency(loan.Fee)}");
        if (loan.IsOverdue)
            sb.AppendLine($"Dagar försenat: {loan.DaysOverdue}");
        if (!string.IsNullOrEmpty(loan.Notes))
            sb.AppendLine($"Anteckningar: {loan.Notes}");
        sb.AppendLine();

        sb.AppendLine("LÅNADE BÖCKER:");
        foreach (var bookCopyLoan in bookCopyLoans)
        {
            sb.AppendLine($"- {bookCopyLoan.BookCopy.Details.Title}");
            sb.AppendLine($"  Författare: {bookCopyLoan.BookCopy.Details.Author?.Name ?? "Okänd"}");
            sb.AppendLine($"  ISBN: {bookCopyLoan.BookCopy.Details.ISBN}");
            sb.AppendLine($"  Exemplar-ID: #{bookCopyLoan.BookCopyId}");
            sb.AppendLine($"  Skick: {bookCopyLoan.BookCopy.Condition}");
            sb.AppendLine();
        }

        sb.AppendLine($"Rapport genererad: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        return sb.ToString();
    }

    private static string GenerateLoansReportContent(List<Loan> loans, LoanStatistics stats, string status, string search, int? memberId)
    {
        var sb = new StringBuilder();

        // Rapportrubrik
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine("LÅNERAPPORT - BIBLIOTEKSSYSTEM");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();
        sb.AppendLine($"Genererad: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Filterinformation
        sb.AppendLine("FILTERKRITERIER:");
        sb.AppendLine($"Status: {(string.IsNullOrEmpty(status) ? "Alla" : GetSwedishStatusName(status))}");
        sb.AppendLine($"Sökterm: {(string.IsNullOrEmpty(search) ? "Ingen" : search)}");
        sb.AppendLine($"Medlem-ID: {(memberId?.ToString() ?? "Alla medlemmar")}");
        sb.AppendLine();

        // Sammanfattande statistik
        sb.AppendLine("SAMMANFATTNING:");
        sb.AppendLine($"Totalt antal lån: {loans.Count}");
        sb.AppendLine($"Aktiva lån: {stats.ActiveLoans}");
        sb.AppendLine($"Försenade lån: {stats.OverdueLoans}");
        sb.AppendLine($"Återlämnade idag: {stats.ReturnedLoans}");
        sb.AppendLine($"Totala utestående avgifter: {FormatSwedishCurrency(stats.TotalFees)}");
        sb.AppendLine();

        // Detaljerad lånlista
        sb.AppendLine("DETALJERAD LÅNLISTA:");
        sb.AppendLine("-".PadRight(80, '-'));

        foreach (var loan in loans.Take(100)) // Begränsa till 100 lån för prestanda
        {
            sb.AppendLine($"Lån #{loan.Id}");
            sb.AppendLine($"  Medlem: {loan.Member?.Name ?? "Okänd"} ({loan.Member?.SSN ?? "N/A"})");
            sb.AppendLine($"  Status: {GetSwedishStatusName(loan.Status.ToString())}");
            sb.AppendLine($"  Startdatum: {loan.StartDate:yyyy-MM-dd}");
            sb.AppendLine($"  Förfallodatum: {loan.DueDate:yyyy-MM-dd}");

            if (loan.ReturnDate.HasValue)
            {
                sb.AppendLine($"  Återlämningsdatum: {loan.ReturnDate.Value:yyyy-MM-dd}");
            }

            sb.AppendLine($"  Antal böcker: {loan.TotalBooks}");

            if (loan.Fee > 0)
            {
                sb.AppendLine($"  Förseningsavgift: {FormatSwedishCurrency(loan.Fee)}");
            }

            if (loan.IsOverdue)
            {
                sb.AppendLine($"  Dagar försenat: {loan.DaysOverdue}");
            }

            if (!string.IsNullOrEmpty(loan.Notes))
            {
                sb.AppendLine($"  Anteckningar: {loan.Notes}");
            }

            sb.AppendLine();
        }

        if (loans.Count > 100)
        {
            sb.AppendLine($"[Visar de första 100 lånen av totalt {loans.Count}]");
            sb.AppendLine();
        }

        // Månadsstatistik
        sb.AppendLine("MÅNADSSTATISTIK:");
        sb.AppendLine("-".PadRight(40, '-'));

        var monthlyStats = loans.GroupBy(l => new { l.StartDate.Year, l.StartDate.Month })
                               .OrderByDescending(g => g.Key.Year)
                               .ThenByDescending(g => g.Key.Month)
                               .Take(12);

        foreach (var month in monthlyStats)
        {
            var monthName = new DateTime(month.Key.Year, month.Key.Month, 1)
                               .ToString("MMMM yyyy", new System.Globalization.CultureInfo("sv-SE"));
            sb.AppendLine($"{monthName}: {month.Count()} lån");
        }

        sb.AppendLine();
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine("SLUT PÅ RAPPORT");
        sb.AppendLine("=".PadRight(80, '='));

        return sb.ToString();
    }

    #endregion

    #region Helper Methods

    private async Task UpdateBookCopyAvailability(int loanId, bool isAvailable)
    {
        var bookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(
            filter: x => x.LoanId == loanId);

        foreach (var bookCopyLoan in bookCopyLoans)
        {
            var bookCopy = await _bookCopyService.GetByIdAsync(bookCopyLoan.BookCopyId);
            if (bookCopy != null)
            {
                bookCopy.IsAvailable = isAvailable;
                await _bookCopyService.UpdateAsync(bookCopy);
            }
        }
    }

    private static string FormatSwedishCurrency(decimal amount)
    {
        // Svensk krona formatering: 1.234,56 kr
        var swedishCulture = new System.Globalization.CultureInfo("sv-SE");
        var numberFormat = (System.Globalization.NumberFormatInfo)swedishCulture.NumberFormat.Clone();
        numberFormat.CurrencyPositivePattern = 3; // n $ (nummer mellanslag valuta)
        numberFormat.CurrencyNegativePattern = 8; // -n $ (negativt nummer mellanslag valuta)
        numberFormat.CurrencySymbol = "kr";

        return amount.ToString("C", numberFormat);
    }

    private static string GetSwedishLoanStatus(LoanStatus status)
    {
        return status switch
        {
            LoanStatus.Active => "Aktiv",
            LoanStatus.Returned => "Återlämnad",
            LoanStatus.Overdue => "Försenad",
            LoanStatus.Lost => "Förlorad",
            LoanStatus.Cancelled => "Avbruten",
            _ => "Okänd"
        };
    }

    private static string GetSwedishStatusName(string status) => status.ToLower() switch
    {
        "active" => "Aktiva",
        "overdue" => "Försenade",
        "returned" => "Återlämnade",
        "lost" => "Förlorade",
        "cancelled" => "Avbrutna",
        _ => status
    };

    private async Task<List<Loan>> GetFilteredLoans(string status, string search, int? memberId)
    {
        var query = await _loanService.GetAllLoansAsync(
            filter: null,
            orderBy: x => x.OrderByDescending(l => l.StartDate),
            l => l.Member,
            l => l.BookCopyLoans);

        var filteredLoans = query.AsQueryable();

        // Statusfilter
        if (!string.IsNullOrEmpty(status))
        {
            switch (status.ToLower())
            {
                case "active":
                    filteredLoans = filteredLoans.Where(l => l.Status == LoanStatus.Active);
                    break;
                case "overdue":
                    filteredLoans = filteredLoans.Where(l => l.Status == LoanStatus.Overdue ||
                                                       (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow));
                    break;
                case "returned":
                    filteredLoans = filteredLoans.Where(l => l.Status == LoanStatus.Returned);
                    break;
                case "lost":
                    filteredLoans = filteredLoans.Where(l => l.Status == LoanStatus.Lost);
                    break;
                case "cancelled":
                    filteredLoans = filteredLoans.Where(l => l.Status == LoanStatus.Cancelled);
                    break;
            }
        }

        // Medlemsfilter
        if (memberId.HasValue)
        {
            filteredLoans = filteredLoans.Where(l => l.MemberId == memberId.Value);
        }

        // Sökfilter
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            filteredLoans = filteredLoans.Where(l =>
                l.Member.Name.Contains(searchLower) ||
                l.Member.SSN.Contains(searchLower) ||
                l.Id.ToString().Contains(searchLower));
        }

        return [.. filteredLoans];
    }

    private static LoanStatistics CalculateLoanStatistics(IEnumerable<Loan> allLoans) => new()
    {
        ActiveLoans = allLoans.Count(l => l.Status == LoanStatus.Active),
        OverdueLoans = allLoans.Count(l => l.Status == LoanStatus.Overdue ||
                                     (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow)),
        ReturnedLoans = allLoans.Count(l => l.ReturnDate.HasValue &&
                                      l.ReturnDate.Value.Date == DateTime.Today),
        TotalFees = allLoans.Where(l => l.Fee > 0 && l.Status != LoanStatus.Returned)
                               .Sum(l => l.Fee)
    };

    #endregion

    #region Request Models

    public class AddBookToLoanRequest
    {
        public int BookCopyId { get; set; }
    }

    public class RemoveBookFromLoanRequest
    {
        public int BookCopyId { get; set; }
    }

    public class UpdateNotesRequest
    {
        public string Notes { get; set; }
    }

    #endregion
}

#region Expression Extensions

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);
        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
    }

    private class ReplaceExpressionVisitor(Expression oldValue, Expression newValue) : ExpressionVisitor
    {
        private readonly Expression _oldValue = oldValue;
        private readonly Expression _newValue = newValue;

        public override Expression Visit(Expression node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}

#endregion
