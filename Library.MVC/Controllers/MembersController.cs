using CsvHelper;
using CsvHelper.Configuration;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Library.MVC.Controllers;

#nullable disable
public class MembersController(
    IMemberService memberService,
    ILoanService loanService,
    IBookCopyLoanService bookCopyLoanService,
    ILogger<MembersController> logger) : Controller
{
    #region Fields and Constructor

    private readonly ILoanService _loanService = loanService;
    private readonly IMemberService _memberService = memberService;
    private readonly IBookCopyLoanService _bookCopyLoanService = bookCopyLoanService;
    private readonly ILogger<MembersController> _logger;
    private static readonly string[] data = ["Dubblett personnummer"];
    private static readonly string[] dataArray = ["Dubblett e-post"];
    #endregion

    #region CRUD Operationer

    [HttpGet]
    public async Task<IActionResult> Index(string search, MembershipStatus? status, string sort = "date_desc", int page = 1, int pageSize = 12)
    {
        try
        {
            // Bygg filteruttryck
            var members = await _memberService.GetAllMembersAsync(
                filter: m =>
                    (string.IsNullOrEmpty(search) ||
                     m.Name.Contains(search) ||
                     m.Email.Contains(search) ||
                     m.SSN.Contains(search)) &&
                    (!status.HasValue || m.Status == status.Value),
                orderBy: GetSortExpression(sort),
                includeProperties: m => m.Loans);

            // Beräkna paginering
            var totalItems = members.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedMembers = members.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new MemberViewModel
            {
                Members = pagedMembers,
                SearchTerm = search,
                StatusFilter = status,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalItems,
                TotalPages = totalPages,

                // Dropdown-alternativ
                MembershipStatuses = Enum.GetValues<MembershipStatus>()
                    .Select(s => new SelectListItem
                    {
                        Text = GetStatusDisplayName(s),
                        Value = s.ToString(),
                        Selected = s == status
                    }).Prepend(new SelectListItem { Text = "Alla statusar", Value = "" }),

                Statistics = new ViewModels.MemberViewModelStatistics
                {
                    TotalMembers = totalItems,
                    ActiveMembers = members.Count(m => m.Status == MembershipStatus.Active),
                    SuspendedMembers = members.Count(m => m.Status == MembershipStatus.Suspended),
                    MembersWithOverdueBooks = members.Count(m => m.Loans.Any(l => l.IsOverdue)),
                    TotalOutstandingFees = members.Sum(m => m.Loans.Sum(l => l.Fee))
                }
            };

            ViewBag.SortBy = sort;
            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid laddning av medlemsindex");
            TempData["Error"] = "Kunde inte ladda medlemmar. Försök igen.";
            return View(new MemberViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, int page = 1, int pageSize = 10)
    {
        if (id <= 0)
            return NotFound();

        try
        {
            var loans = await _loanService.GetAllLoansAsync(
                filter: x => x.MemberId == id,
                orderBy: x => x.OrderByDescending(x => x.StartDate),
                l => l.Member, l => l.BookCopyLoans
             );

            if (loans == null)
                return NotFound();

            var bookCopyLoans = await _bookCopyLoanService.GetAllBookCopyLoansAsync(
                filter: l => l.Loan.MemberId == id,
                orderBy: null,
                b => b.BookCopy,
                b => b.BookCopy.Details,
                b => b.BookCopy.Details.Author
            );

            var memberLoans = await _loanService.GetAllLoansAsync(
                filter: l => l.MemberId == id
            );

            var statistics = new LoanStatistics
            {
                ActiveLoans = memberLoans.Count(l => l.Status == LoanStatus.Active),
                OverdueLoans = memberLoans.Count(l => l.Status == LoanStatus.Overdue ||
                                               (l.ReturnDate == null && l.DueDate < DateTime.UtcNow)),
                ReturnedLoans = memberLoans.Count(l => l.ReturnDate.HasValue),
                TotalFees = memberLoans.Where(l => l.Fee > 0 && l.Status != LoanStatus.Returned)
                                              .Sum(l => l.Fee)
            };

            //Return first försenat and then aktiv otherwise aktiv

            var loanViewModel = new LoanViewModel
            {
                Loans = loans,
                BookCopyLoans = bookCopyLoans,
                Statistics = statistics,
                Loan = loans.FirstOrDefault(x => x.Status == LoanStatus.Overdue)
                       ?? loans.FirstOrDefault(x => x.Status == LoanStatus.Active)
                       ?? loans.FirstOrDefault()
                       ?? new()
            };

            ViewBag.Member = await _memberService.GetMemberOrDefaultAsync(x => x.Id == id);
            ViewBag.TotalBookCopyLoans = bookCopyLoans?.Count ?? 0;

            return View(loanViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid laddning av medlemsdetaljer för ID {MemberId}", id);
            TempData["Error"] = "Kunde inte ladda medlemsdetaljer.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var activeMembers = await _memberService.GetActiveMembers();
            var totalMembers = await _memberService.GetAllAsync();
            var statistics = new MemberViewModelStatistics
            {
                TotalMembers = totalMembers?.Count ?? 0,
                ActiveMembers = activeMembers?.Count ?? 0,
                MembersWhoCanBorrow = totalMembers.Where(m => m.CanBorrow)?.Count() ?? 0,
                ActiveLoans = 0,
                TotalLoans = 0
            };

            var viewModel = new MemberViewModel
            {
                Member = new Member
                {
                    MembershipStartDate = DateTime.Now.Date,
                    Status = MembershipStatus.Active,
                    MaxLoans = 3
                },
                MembershipStatuses = GetMembershipStatusOptions(),
                MemberLoans = [],
                Statistics = statistics
            };

            return await Task.FromResult(View(viewModel));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid laddning av skapa medlem-sida");
            TempData["Error"] = "Kunde inte ladda skapa medlem-sida. Försök igen.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MemberViewModel model)
    {
        if (model.Member == null)
        {
            return Json(new { success = false, message = "Ogiltig förfrågan: Modell saknas." });
        }

        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                   .Where(ms => ms.Value.Errors.Count > 0)
                   .ToDictionary(
                       kvp => kvp.Key,
                       kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                   );

                return Json(new { success = false, message = "Valideringsfel", errors });
            }

            // Kontrollera dubbletter
            var existingMemberBySSN = await _memberService.GetMemberOrDefaultAsync(
                m => m.SSN.ToLower() == model.Member.SSN.ToLower().Trim());

            if (existingMemberBySSN != null)
            {
                return Json(new { success = false, message = "En medlem med detta personnummer finns redan.", errors = new { Member_SSN = data } });
            }

            var existingMemberByEmail = await _memberService.GetMemberOrDefaultAsync(
            m => m.Email.ToLower() == model.Member.Email.ToLower().Trim());

            if (existingMemberByEmail != null)
            {
                return Json(new { success = false, message = "En medlem med denna e-postadress finns redan.", errors = new { Member_Email = dataArray } });
            }

            // Skapa ny medlem
            var member = new Member
            {
                SSN = model.Member.SSN.Trim(),
                Name = model.Member.Name.Trim(),
                Email = model.Member.Email.ToLower().Trim(),
                PhoneNumber = model.Member.PhoneNumber?.Trim(),
                Address = model.Member.Address?.Trim(),
                DateOfBirth = model.Member.DateOfBirth,
                MembershipStartDate = model.Member.MembershipStartDate,
                Status = model.Member.Status,
                MaxLoans = model.Member.MaxLoans,
                Notes = model.Member.Notes,
                CreatedDate = DateTime.UtcNow,
            };

            await _memberService.AddAsync(member);

            return Json(new { success = true, redirectUrl = Url.Action(nameof(Details), "Members", new { id = member.Id }) });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid skapande av medlem: {Name}", model.Member?.Name);
            return Json(new { success = false, message = "Ett fel uppstod vid skapande av medlem. Försök igen." });
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (id <= 0)
            return NotFound();

        try
        {
            var member = await _memberService.GetMemberByIdAsync(id, true);

            if (member == null)
                return NotFound();

            var viewModel = new MemberViewModel
            {
                Member = member,
                MembershipStatuses = GetMembershipStatusOptions()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid laddning av redigera medlem-sida för ID: {MemberId}", id);
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [FromForm] MemberViewModel model)
    {
        if (id <= 0 || model.Member == null || id != model.Member.Id)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.MembershipStatuses = GetMembershipStatusOptions();
            return View(model);
        }

        try
        {
            var existingMember = await _memberService.GetByIdAsync(id);
            if (existingMember == null)
                return NotFound();

            // Kontrollera dubbletter (exkludera nuvarande medlem)
            var duplicateSSN = await _memberService.GetMemberOrDefaultAsync(
                m => m.SSN.ToLower() == model.Member.SSN.ToLower().Trim() && m.Id != id);

            if (duplicateSSN != null)
            {
                ModelState.AddModelError("Member.SSN", "En annan medlem med detta personnummer finns redan.");
                model.MembershipStatuses = GetMembershipStatusOptions();
                return View(model);
            }

            var duplicateEmail = await _memberService.GetMemberOrDefaultAsync(
                m => m.Email.ToLower() == model.Member.Email.ToLower().Trim() && m.Id != id);

            if (duplicateEmail != null)
            {
                ModelState.AddModelError("Member.Email", "En annan medlem med denna e-postadress finns redan.");
                model.MembershipStatuses = GetMembershipStatusOptions();
                return View(model);
            }

            // Uppdatera alla egenskaper
            existingMember.SSN = model.Member.SSN.Trim();
            existingMember.Name = model.Member.Name.Trim();
            existingMember.Email = model.Member.Email.ToLower().Trim();
            existingMember.PhoneNumber = model.Member.PhoneNumber?.Trim();
            existingMember.Address = model.Member.Address?.Trim();
            existingMember.DateOfBirth = model.Member.DateOfBirth;
            existingMember.Status = model.Member.Status;
            existingMember.MaxLoans = model.Member.MaxLoans;
            existingMember.Notes = model.Member.Notes;
            existingMember.UpdatedDate = DateTime.UtcNow;

            await _memberService.UpdateAsync(existingMember);

            TempData["Success"] = $"Medlem '{existingMember.Name}' uppdaterades framgångsrikt.";
            return Json(new { success = true, data = existingMember.Id });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid uppdatering av medlem ID: {MemberId}", id);
            TempData["Error"] = "Ett fel uppstod vid uppdatering av medlem. Försök igen.";
            model.MembershipStatuses = GetMembershipStatusOptions();
            return View(model);
        }
    }

    #endregion

    #region History & Statistics
    [HttpGet]
    public async Task<IActionResult> History(int memberId, int page = 1, int pageSize = 10)
    {
        try
        {  // Get member with detailed information
            var member = await _memberService.GetMemberOrDefaultAsync(m => m.Id == memberId);
            if (member == null) return NotFound();

            // Get all loans for this member with detailed book information
            var memberLoans = await _loanService.GetAllLoansAsync(
                filter: l => l.MemberId == memberId,
                orderBy: x => x.OrderByDescending(l => l.StartDate),
                includeProperties: "Member,BookCopyLoans,BookCopyLoans.BookCopy,BookCopyLoans.BookCopy.Details,BookCopyLoans.BookCopy.Details.Author"
                );

            // Calculate pagination
            var totalItems = memberLoans.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedLoans = memberLoans.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Calculate comprehensive statistics
            var statistics = new LoanStatistics
            {
                ActiveLoans = memberLoans.Count(l => l.Status == LoanStatus.Active),
                OverdueLoans = memberLoans.Count(l => l.Status == LoanStatus.Overdue ||
                                              (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow)),
                ReturnedLoans = memberLoans.Count(l => l.ReturnDate.HasValue &&
                                                l.ReturnDate.Value.Date == DateTime.Today),
                TotalFees = memberLoans.Where(l => l.Fee > 0 && l.Status != LoanStatus.Returned)
                                      .Sum(l => l.Fee)
            };

            // Create view model
            var viewModel = new LoanViewModel
            {
                Loans = pagedLoans,
                Statistics = statistics
            };

            // Set ViewBag properties for the view
            ViewBag.Member = member;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;
            ViewBag.MemberLoans = memberLoans; // All loans for statistics
            ViewBag.AllLoans = memberLoans; // For comprehensive calculations

            return View("History", viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid laddning av medlemshistorik för ID: {MemberId}", memberId);
            TempData["Error"] = "Kunde inte ladda medlemshistorik.";
            return RedirectToAction(nameof(Index));
        }
    }
 
    [HttpGet]
    public async Task<IActionResult> DeepAnalytics()
    {
        try
        {
            var members = await _memberService.GetAllMembersAsync(
                orderBy: x => x.OrderBy(m => m.Name),
                includeAllProperties: true
                );

            var viewModel = new MemberViewModel
            {
                Members = members,
                Statistics = new MemberViewModelStatistics
                {
                    TotalMembers = members.Count,
                    ActiveMembers = members.Count(m => m.Status == MembershipStatus.Active),
                    SuspendedMembers = members.Count(m => m.Status == MembershipStatus.Suspended),
                    MembersWithOverdueBooks = members.Count(m => m.Loans.Any(l => l.IsOverdue)),
                    TotalOutstandingFees = members.Sum(m => m.Loans.Sum(l => l.Fee)),
                    TotalBooksBorrowed = members.Sum(m => m.Loans.Sum(selector: x => x.BookCopyLoans.Count))
                }
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid laddning av fördjupad analys");
            TempData["Error"] = "Kunde inte ladda analysdata.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAnalyticsData(bool deep = false)
    {
        try
        {
            var members = await _memberService.GetAllMembersAsync(includeAllProperties: true);

            var loans = await _loanService.GetAllAsync();
            var now = DateTime.UtcNow;

            // Calculate monthly registrations (last 12 months)
            var monthlyRegistrations = Enumerable.Range(0, 12)
                .Select(i => now.AddMonths(-i))
                .OrderBy(d => d)
                .Select(d => new
                {
                    Month = d.ToString("MMM yyyy", new CultureInfo("sv-SE")),
                    Count = members.Count(m => m.MembershipStartDate.Year == d.Year && m.MembershipStartDate.Month == d.Month)
                })
                .ToList();

            // Calculate loan activity by month (last 12 months)
            var loanActivity = Enumerable.Range(0, 12)
                .Select(i => now.AddMonths(-i))
                .OrderBy(d => d)
                .Select(d => new
                {
                    Month = d.ToString("MMM yyyy", new CultureInfo("sv-SE")),
                    Active = loans.Count(l => l.StartDate.Year == d.Year && l.StartDate.Month == d.Month && l.ReturnDate == null),
                    Overdue = loans.Count(l => l.StartDate.Year == d.Year && l.StartDate.Month == d.Month && l.DueDate < now && l.ReturnDate == null),
                    Returned = loans.Count(l => l.StartDate.Year == d.Year && l.StartDate.Month == d.Month && l.ReturnDate != null)
                })
                .ToList();

            // Calculate status distribution
            var statusDistribution = new
            {
                Active = members.Count(m => m.Status == MembershipStatus.Active),
                Suspended = members.Count(m => m.Status == MembershipStatus.Suspended),
                Inactive = members.Count(m => m.Status == MembershipStatus.Inactive),
                Total = members.Count
            };

            // Calculate fee distribution (top 5 members with fees)
            var feeDistribution = members
                .Where(m => m.Loans.Any(l => l.Fee > 0))
                .OrderByDescending(m => m.Loans.Sum(l => l.Fee))
                .Take(5)
                .Select(m => new
                {
                    Name = m.Name,
                    TotalFees = m.Loans.Sum(l => l.Fee)
                })
                .ToList();

            // Calculate MemberViewModelStatistics
            var statistics = new
            {
                TotalMembers = members.Count,
                ActiveMembers = members.Count(m => m.Status == MembershipStatus.Active),
                SuspendedMembers = members.Count(m => m.Status == MembershipStatus.Suspended),
                MembersWithOverdueBooks = members.Count(m => m.Loans.Any(l => l.IsOverdue)),
                TotalOutstandingFees = members.Sum(m => m.Loans.Sum(selector: l => l.Fee)),
                NewMembersThisMonth = members.Count(m => m.MembershipStartDate.Year == now.Year && m.MembershipStartDate.Month == now.Month),
                AverageLoansPerMember = members.Any() ? members.Average(m => m.Loans.Count) : 0,
                MembersWhoCanBorrow = members.Count(m => m.CanBorrow),
                TotalLoans = loans.Count,
                ActiveLoans = loans.Count(l => l.Status == LoanStatus.Active),
                OverdueLoans = loans.Count(l => l.IsOverdue),
                ReturnedToday = loans.Count(l => l.ReturnDate?.Date == now.Date),
                TotalBooksBorrowed = loans.Sum(l => l.TotalBooks)
            };

            // Calculate LoanStatistics
            var loanStatistics = new
            {
                ActiveLoans = loans.Count(l => l.Status == LoanStatus.Active),
                OverdueLoans = loans.Count(l => l.IsOverdue),
                ReturnedLoans = loans.Count(l => l.Status == LoanStatus.Returned),
                TotalFees = loans.Sum(l => l.Fee)
            };

            // Initialize deep analytics data as null by default
            IEnumerable<object> topBorrowers = null;
            IEnumerable<object> feeTrends = null;
            IEnumerable<object> loanDuration = null;
            IEnumerable<object> loanHistoryStatistics = null;

            if (deep)
            {
                // Calculate top borrowers (top 10 members by total loans)
                topBorrowers = [.. members
                    .OrderByDescending(m => m.Loans.Count)
                    .Take(10)
                    .Select(m => new
                    { m.Name,
                        TotalLoans = m.Loans.Count
                    })];

                // Calculate fee trends (last 12 months)
                feeTrends = [.. Enumerable.Range(0, 12)
                    .Select(i => now.AddMonths(-i))
                    .OrderBy(d => d)
                    .Select(d => new
                    {
                        Month = d.ToString("MMM yyyy", new CultureInfo("sv-SE")),
                        TotalFees = loans
                            .Where(l => l.StartDate.Year == d.Year && l.StartDate.Month == d.Month)
                            .Sum(l => l.Fee)
                    })];

                // Calculate loan duration (histogram, binned by 5-day intervals)
                loanDuration = [.. loans
                    .Where(l => l.ReturnDate != null)
                    .Select(l => (l.ReturnDate.Value - l.StartDate).Days)
                    .GroupBy(d => Math.Min(d / 5, 10) * 5) // Bin by 5-day intervals, max 50 days
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        Range = $"{g.Key}-{g.Key + 4} dagar",
                        Count = g.Count()
                    })];

                // Calculate LoanHistoryStatistics for top 5 members
                loanHistoryStatistics = [.. members
                    .OrderByDescending(m => m.Loans.Count)
                    .Take(5)
                    .Select(m => new
                    {
                        MemberId = m.Id,
                        MemberName = m.Name,
                        TotalMemberLoans = m.Loans.Count,
                        MemberActiveLoans = m.Loans.Count(l => l.Status == LoanStatus.Active),
                        MemberTotalFees = m.Loans.Sum(l => l.Fee),
                        TotalBooksLoaned = m.Loans.Sum(l => l.TotalBooks),
                        AverageBookPopularity = m.Loans.Count != 0 ? m.Loans.Average(l => l.BookCopyLoans?.Count ?? 0) : 0,
                        MemberSince = m.MembershipStartDate,
                        DaysSinceMembership = (now - m.MembershipStartDate).Days
                    })];
            }

            // Declare analyticsData once with all properties
            var analyticsData = new
            {
                monthlyRegistrations = new
                {
                    labels = monthlyRegistrations.Select(m => m.Month).ToArray(),
                    data = monthlyRegistrations.Select(m => m.Count).ToArray()
                },
                loanActivity = new
                {
                    labels = loanActivity.Select(l => l.Month).ToArray(),
                    active = loanActivity.Select(l => l.Active).ToArray(),
                    overdue = loanActivity.Select(l => l.Overdue).ToArray(),
                    returned = loanActivity.Select(l => l.Returned).ToArray()
                },
                statusDistribution,
                feeDistribution = new
                {
                    labels = feeDistribution.Select(f => f.Name).ToArray(),
                    data = feeDistribution.Select(f => f.TotalFees).ToArray()
                },
                statistics,
                loanStatistics,
                topBorrowers = topBorrowers != null ? new
                {
                    labels = topBorrowers.Select(t => (string)t.GetType().GetProperty("Name").GetValue(t)).ToArray(),
                    data = topBorrowers.Select(t => (int)t.GetType().GetProperty("TotalLoans").GetValue(t)).ToArray()
                } : null,
                feeTrends = feeTrends != null ? new
                {
                    labels = feeTrends.Select(f => (string)f.GetType().GetProperty("Month").GetValue(f)).ToArray(),
                    data = feeTrends.Select(f => (decimal)f.GetType().GetProperty("TotalFees").GetValue(f)).ToArray()
                } : null,
                loanDuration = loanDuration != null ? new
                {
                    labels = loanDuration.Select(d => (string)d.GetType().GetProperty("Range").GetValue(d)).ToArray(),
                    data = loanDuration.Select(d => (int)d.GetType().GetProperty("Count").GetValue(d)).ToArray()
                } : null,
                loanHistoryStatistics = loanHistoryStatistics != null ? new
                {
                    data = loanHistoryStatistics.Select(h => new
                    {
                        MemberId = (int)h.GetType().GetProperty("MemberId").GetValue(h),
                        MemberName = (string)h.GetType().GetProperty("MemberName").GetValue(h),
                        TotalMemberLoans = (int)h.GetType().GetProperty("TotalMemberLoans").GetValue(h),
                        MemberActiveLoans = (int)h.GetType().GetProperty("MemberActiveLoans").GetValue(h),
                        MemberTotalFees = (decimal)h.GetType().GetProperty("MemberTotalFees").GetValue(h),
                        TotalBooksLoaned = (int)h.GetType().GetProperty("TotalBooksLoaned").GetValue(h),
                        AverageBookPopularity = (double)h.GetType().GetProperty("AverageBookPopularity").GetValue(h),
                        MemberSince = (DateTime)h.GetType().GetProperty("MemberSince").GetValue(h),
                        DaysSinceMembership = (int)h.GetType().GetProperty("DaysSinceMembership").GetValue(h)
                    }).ToArray()
                } : null
            };

            return Json(analyticsData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid hämtning av analysdata");
            return StatusCode(500, new { error = "Kunde inte hämta analysdata" });
        }
    }

    #endregion

    #region API-anrop

    [HttpGet]
    public async Task<IActionResult> GetMember(int id)
    {
        try
        {
            var member = await _memberService.GetMemberByIdAsync(id, includeProperties: true);
            if (member == null)
                return NotFound();

            var result = new
            {
                id = member.Id,
                name = member.Name,
                email = member.Email,
                ssn = member.SSN,
                phoneNumber = member.PhoneNumber,
                address = member.Address,
                status = member.Status.ToString(),
                statusDisplay = GetStatusDisplayName(member.Status),
                activeLoans = member.ActiveLoansCount,
                maxLoans = member.MaxLoans,
                canBorrow = member.CanBorrow,
                membershipStartDate = member.MembershipStartDate.ToString("yyyy-MM-dd"),
                totalFees = member.Loans.Sum(l => l.Fee)
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid hämtning av medlemsdetaljer för ID: {MemberId}", id);
            return BadRequest(new { error = "Kunde inte hämta medlemsdetaljer" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new { success = true, data = Array.Empty<object>() });

            var members = await _memberService.GetAllMembersAsync(
                filter: m => m.Name.Contains(term) ||
                            m.Email.Contains(term) ||
                            m.SSN.Contains(term),
                orderBy: x => x.OrderBy(m => m.Name));

            var result = members.Take(10).Select(m => new
            {
                id = m.Id,
                name = m.Name,
                email = m.Email,
                ssn = m.SSN,
                status = GetStatusDisplayName(m.Status),
                activeLoans = m.ActiveLoansCount
            });

            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid sökning av medlemmar med term: {SearchTerm}", term);
            return Json(new { success = false, error = "Sökning misslyckades" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, MembershipStatus status)
    {
        try
        {
            var member = await _memberService.GetByIdAsync(id);
            if (member == null)
                return Json(new { success = false, message = "Medlem hittades inte." });

            member.Status = status;
            member.UpdatedDate = DateTime.UtcNow;
            await _memberService.UpdateAsync(member);

            return Json(new
            {
                success = true,
                message = $"Medlemsstatus uppdaterad till {GetStatusDisplayName(status)}.",
                newStatus = status.ToString(),
                newStatusDisplay = GetStatusDisplayName(status)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid uppdatering av medlemsstatus för ID: {MemberId}", id);
            return Json(new { success = false, message = "Kunde inte uppdatera medlemsstatus." });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return Json(new { success = false, message = "Ogiltigt medlems-ID." });

        try
        {
            var member = await _memberService.GetMemberByIdAsync(id, includeProperties: true);
            if (member == null)
                return Json(new { success = false, message = "Medlem hittades inte." });

            // Kontrollera om medlemmen har aktiva lån
            var activeLoans = member.Loans.Where(l => l.ReturnDate == null).ToList();
            if (activeLoans.Count != 0)
            {
                return Json(new
                {
                    success = false,
                    message = $"Kan inte ta bort medlem. Medlemmen har {activeLoans.Count} aktiva lån som måste returneras först."
                });
            }

            // Kontrollera om medlemmen har obetalda avgifter
            var outstandingFees = member.Loans.Sum(l => l.Fee);
            if (outstandingFees > 0)
            {
                return Json(new
                {
                    success = false,
                    message = $"Kan inte ta bort medlem. Medlemmen har obetalda avgifter på {outstandingFees:C}."
                });
            }

            await _memberService.DeleteAsync(id);

            return Json(new
            {
                success = true,
                message = $"Medlem '{member.Name}' togs bort framgångsrikt."
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid borttagning av medlem ID: {MemberId}", id);
            return Json(new
            {
                success = false,
                message = "Ett fel uppstod vid borttagning av medlem."
            });
        }
    }

    #endregion

    #region File Operations

    [HttpGet]
    [Route("/Members/GenerateReport/{memberId}")]
    public async Task<IActionResult> GenerateReport(int memberId)
    {
        if (memberId <= 0)
            return NotFound();

        try
        {
            // Hämta medlem med inkluderade lån
            var member = await _memberService.GetMemberByIdAsync(memberId, includeProperties: true);
            if (member == null)
                return NotFound();

            // Hämta medlemmens lån med detaljer (begränsa till max 500 för att undvika overload)
            var memberLoans = await _loanService.GetAllLoansAsync(
                filter: l => l.MemberId == memberId,
                orderBy: x => x.OrderByDescending(l => l.StartDate),
                includeProperties: "BookCopyLoans,BookCopyLoans.BookCopy,BookCopyLoans.BookCopy.Details,BookCopyLoans.BookCopy.Details.Author"
            );

            if (memberLoans.Count > 500)
            {
                _logger.LogWarning("Too many loans ({Count}) for member {MemberId}; truncating report.", memberLoans.Count, memberId);
                memberLoans = [.. memberLoans.Take(500)]; // Begränsa för prestanda
            }

            // Beräkna statistik
            var statistics = new LoanStatistics
            {
                ActiveLoans = memberLoans.Count(l => l.Status == LoanStatus.Active),
                OverdueLoans = memberLoans.Count(l => l.Status == LoanStatus.Overdue ||
                                              (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow)),
                ReturnedLoans = memberLoans.Count(l => l.ReturnDate.HasValue &&
                                                l.ReturnDate.Value.Date == DateTime.Today),
                TotalFees = memberLoans.Where(l => l.Fee > 0 && l.Status != LoanStatus.Returned)
                                      .Sum(l => l.Fee)
            };

            // Generera rapportinnehåll
            var report = GenerateMemberReportContent(member, memberLoans, statistics);

            // Kontrollera rapportstorlek (t.ex. max 10MB för att undvika crash)
            var bytes = Encoding.UTF8.GetBytes(report);
            if (bytes.Length > 10 * 1024 * 1024) // 10MB gräns
            {
                _logger.LogWarning("Report too large ({Size} bytes) for member {MemberId}; returning error.", bytes.Length, memberId);
                TempData["Error"] = "Rapporten är för stor för att genereras. Kontakta support.";
                return RedirectToAction(nameof(Details), new { memberId });
            }

            var fileName = $"medlemsrapport_{member.Name.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            return File(bytes, "text/plain", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid generering av medlemsrapport för ID: {MemberId}", memberId);
            TempData["Error"] = "Misslyckades med att generera rapport. Försök igen senare.";
            return RedirectToAction(nameof(Details), new { memberId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportToCsv()
    {
        try
        {
            var members = await _memberService.GetAllMembersAsync(
                orderBy: x => x.OrderBy(m => m.Name),
                includeProperties: m => m.Loans);

            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(false)))
            {
                var config = GetCsvConfig(null, streamWriter);
                using var csv = new CsvWriter(streamWriter, config);
                // Write headers
                var headers = new[]
                {
                        "Namn", "E-post", "Personnummer", "Telefon", "Adress",
                        "Födelsedatum", "Medlemskap Startdatum", "Status", "MaxLån",
                        "Skapelsedatum", "Skapad av", "Uppdaterad av", "Raderad",
                        "Raderingsdatum", "Raderad av", "Anteckningar"
                    };

                foreach (var header in headers)
                {
                    csv.WriteField(header);
                }
                await csv.NextRecordAsync();

                // Write member data
                foreach (var member in members)
                {
                    csv.WriteField(member.Name ?? "");
                    csv.WriteField(member.Email ?? "");
                    csv.WriteField(member.SSN ?? "");
                    csv.WriteField(member.PhoneNumber ?? "");
                    csv.WriteField(member.Address ?? "");
                    csv.WriteField(member.DateOfBirth?.ToString("yyyy-MM-dd") ?? "");
                    csv.WriteField(member.MembershipStartDate.ToString("yyyy-MM-dd"));
                    csv.WriteField(member.Status == MembershipStatus.Active ? "Aktiv" : "Inaktiv");
                    csv.WriteField(member.MaxLoans.ToString());
                    csv.WriteField(member.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    csv.WriteField(member.CreatedBy ?? "");
                    csv.WriteField(member.UpdatedBy ?? "");
                    csv.WriteField(member.IsDeleted ? "True" : "False");
                    csv.WriteField(member.DeletedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                    csv.WriteField(member.DeletedBy ?? "");
                    csv.WriteField(member.Notes ?? "");

                    await csv.NextRecordAsync();
                }
            }

            var fileName = $"medlemmar_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(memoryStream.ToArray(), "text/csv; charset=utf-8", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting members to CSV");
            TempData["Error"] = "Export misslyckades. Försök igen senare.";
            return RedirectToAction("Index");
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, message = "Ingen fil vald eller filen är tom." });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Json(new { success = false, message = "Endast CSV-filer stöds." });
        }

        // File size validation (10MB limit)
        if (file.Length > 10 * 1024 * 1024)
        {
            return Json(new { success = false, message = "Filen är för stor. Maximal storlek är 10MB." });
        }

        try
        {
            var errors = new List<string>();
            var warnings = new List<string>();
            var validRecords = new List<Member>();
            var skippedRecords = new List<string>();
            var importedCount = 0;

            // Pre-fetch existing identifiers for duplicate checking
            var allMembers = await _memberService.GetAllMembersAsync();
            var existingSSNs = allMembers.Where(m => !string.IsNullOrEmpty(m.SSN))
                                       .Select(m => m.SSN).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existingEmails = allMembers.Where(m => !string.IsNullOrEmpty(m.Email))
                                         .Select(m => m.Email).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var currentFileSSNs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var currentFileEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var stream = file.OpenReadStream())
            using (var reader = new StreamReader(stream, new UTF8Encoding(true, true)))
            {
                var config = GetCsvConfig(reader, null);
                using var csv = new CsvReader(reader, config);
                try
                {
                    // Read and validate header
                    await csv.ReadAsync();
                    csv.ReadHeader();

                    if (csv.HeaderRecord == null || csv.HeaderRecord.Length == 0)
                    {
                        return Json(new { success = false, message = "Filen verkar vara tom eller har ingen rubrikrad." });
                    }

                    // Verify essential headers exist
                    var requiredHeaders = new[] { "Namn", "E-post", "Personnummer" };
                    var headerRecord = csv.HeaderRecord.Select(h => h.Replace("\"", "").Trim()).ToArray();
                    var missingHeaders = requiredHeaders.Where(h => !headerRecord.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();

                    if (missingHeaders.Count != 0)
                    {
                        _logger.LogWarning("Missing headers: {Missing}. Available headers: {Available}",
                            string.Join(", ", missingHeaders), string.Join(", ", headerRecord));

                        return Json(new
                        {
                            success = false,
                            message = $"Saknade obligatoriska rubriker: {string.Join(", ", missingHeaders)}",
                            details = $"Tillgängliga rubriker: {string.Join(", ", headerRecord)}"
                        });
                    }

                    // Create header index mapping (case-insensitive)
                    var headerIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < csv.HeaderRecord.Length; i++)
                    {
                        var cleanHeader = csv.HeaderRecord[i].Replace("\"", "").Trim();
                        headerIndexMap[cleanHeader] = i;
                    }

                    int processedRows = 0;
                    while (await csv.ReadAsync())
                    {
                        processedRows++;
                        var rowNumber = csv.Parser.Row;

                        // Limit processing to prevent memory issues
                        if (processedRows > 1000)
                        {
                            warnings.Add("Import begränsad till 1000 rader för prestanda.");
                            break;
                        }

                        try
                        {
                            // Validate field count matches header
                            if (csv.Parser.Count != csv.HeaderRecord.Length)
                            {
                                errors.Add($"Rad {rowNumber}: Antal fält ({csv.Parser.Count}) matchar inte rubriker ({csv.HeaderRecord.Length})");
                                continue;
                            }

                            // Get fields by header index (case-insensitive)
                            var getField = new Func<string, string>(header =>
                            {
                                if (headerIndexMap.TryGetValue(header, out int index) && index < csv.Parser.Count)
                                {
                                    var value = csv.Parser[index]?.Trim();
                                    return string.IsNullOrEmpty(value) ? null : value;
                                }
                                return null;
                            });

                            var name = getField("Namn");
                            var email = getField("E-post");
                            var ssn = getField("Personnummer");
                            var phone = getField("Telefon");
                            var address = getField("Adress");
                            var dateOfBirth = getField("Födelsedatum");
                            var startDateStr = getField("Medlemskap Startdatum");
                            var status = getField("Status") ?? "Aktiv";
                            var notes = getField("Anteckningar");

                            // Validate required fields
                            var fieldErrors = new List<string>();
                            if (string.IsNullOrWhiteSpace(ssn))
                                fieldErrors.Add("Personnummer saknas");
                            if (string.IsNullOrWhiteSpace(name))
                                fieldErrors.Add("Namn saknas");
                            if (string.IsNullOrWhiteSpace(email))
                                fieldErrors.Add("E-post saknas");

                            if (fieldErrors.Count != 0)
                            {
                                errors.Add($"Rad {rowNumber}: {string.Join(", ", fieldErrors)}");
                                continue;
                            }

                            // Create record
                            var record = new Member
                            {
                                Name = name,
                                Email = email,
                                SSN = ssn,
                                PhoneNumber = phone,
                                Address = address,
                                Status = status.Equals("Aktiv", StringComparison.OrdinalIgnoreCase)
                                    ? MembershipStatus.Active
                                    : MembershipStatus.Inactive,
                                Notes = notes
                            };

                            // Parse and validate dates
                            if (!string.IsNullOrEmpty(dateOfBirth))
                            {
                                if (DateTime.TryParseExact(dateOfBirth, ["yyyy-MM-dd", "yyyy/MM/dd", "dd/MM/yyyy", "dd-MM-yyyy"],
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
                                {
                                    if (dob > DateTime.Now.AddYears(-10) || dob < DateTime.Now.AddYears(-120))
                                    {
                                        errors.Add($"Rad {rowNumber}: Orealistiskt födelsedatum: {dateOfBirth}");
                                        continue;
                                    }
                                    record.DateOfBirth = dob;
                                }
                                else
                                {
                                    errors.Add($"Rad {rowNumber}: Ogiltigt format för födelsedatum: '{dateOfBirth}'");
                                    continue;
                                }
                            }

                            if (!string.IsNullOrEmpty(startDateStr))
                            {
                                if (DateTime.TryParseExact(startDateStr, ["yyyy-MM-dd", "yyyy/MM/dd", "dd/MM/yyyy", "dd-MM-yyyy"],
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                                {
                                    record.MembershipStartDate = startDate;
                                }
                                else
                                {
                                    errors.Add($"Rad {rowNumber}: Ogiltigt format för startdatum: '{startDateStr}'");
                                    continue;
                                }
                            }

                            // Validate SSN format
                            if (!Regex.IsMatch(record.SSN, @"^\d{6,8}-?\d{4}$"))
                            {
                                errors.Add($"Rad {rowNumber}: Ogiltigt personnummerformat: {record.SSN}");
                                continue;
                            }

                            // Validate email format
                            if (!IsValidEmail(record.Email))
                            {
                                errors.Add($"Rad {rowNumber}: Ogiltig e-postadress: {record.Email}");
                                continue;
                            }

                            // Check duplicates and skip if found
                            var duplicateErrors = new List<string>();
                            if (existingSSNs.Contains(record.SSN) || currentFileSSNs.Contains(record.SSN))
                            {
                                duplicateErrors.Add($"Personnummer {record.SSN}");
                            }
                            if (existingEmails.Contains(record.Email) || currentFileEmails.Contains(record.Email))
                            {
                                duplicateErrors.Add($"E-post {record.Email}");
                            }

                            if (duplicateErrors.Count != 0)
                            {
                                skippedRecords.Add($"Rad {rowNumber}: {record.Name} - {string.Join(", ", duplicateErrors)} finns redan");
                                continue;
                            }

                            // Set defaults for new records
                            record.CreatedDate = DateTime.UtcNow;
                            record.MembershipStartDate = record.MembershipStartDate == default
                                ? DateTime.UtcNow.Date
                                : record.MembershipStartDate;
                            record.MaxLoans = 3; // Default value
                            record.CreatedBy = User.Identity?.Name ?? "Import";

                            // Track identifiers for duplicates within file
                            currentFileSSNs.Add(record.SSN);
                            currentFileEmails.Add(record.Email);
                            validRecords.Add(record);
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Rad {rowNumber}: Fel vid bearbetning - {ex.Message}");
                        }
                    }
                }
                catch (CsvHelperException ex)
                {
                    return Json(new { success = false, message = $"CSV-format fel: {ex.Message}" });
                }

                // Batch save valid records
                if (validRecords.Count != 0)
                {
                    try
                    {
                        foreach (var record in validRecords)
                        {
                            await _memberService.AddAsync(record);
                            importedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Database error during import");
                        return Json(new { success = false, message = $"Databasfel: {ex.Message}" });
                    }
                }
            }

            // Prepare response
            var responseMessage = "";
            if (importedCount > 0)
            {
                responseMessage = $"{importedCount} medlemmar importerade framgångsrikt.";
            }
            if (skippedRecords.Count != 0)
            {
                responseMessage += $" {skippedRecords.Count} rader skippade pga dubbletter.";
            }
            if (warnings.Count != 0)
            {
                responseMessage += $" {warnings.Count} varningar.";
            }

            var success = importedCount > 0 || (errors.Count == 0 && skippedRecords.Count != 0);

            return Json(new
            {
                success = importedCount > 0 && skippedRecords.Count == 0, // Success only if records were imported and no duplicates
                message = importedCount > 0 && skippedRecords.Count == 0
                    ? $"{importedCount} medlemmar importerade framgångsrikt."
                    : null, // Message only for successful imports
                errors = skippedRecords.Count != 0
                    ? [$"Import misslyckades: {skippedRecords.Count} dubbletter hittades."]
                    : errors, // Add duplicate error message if any
                skipped = skippedRecords, // Still include skipped records for debugging
                importedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during import of file: {FileName}", file.FileName);
            return Json(new
            {
                success = false,
                message = "Ett oväntat fel uppstod vid import.",
                details = ex.Message
            });
        }
    }

    #endregion

    #region Hjälpmetoder

    private Func<IQueryable<Member>, IOrderedQueryable<Member>> GetSortExpression(string sort)
    {
        return sort?.ToLower() switch
        {
            "name_desc" => q => q.OrderByDescending(m => m.Name),
            "email" => q => q.OrderBy(m => m.Email),
            "email_desc" => q => q.OrderByDescending(m => m.Email),
            "status" => q => q.OrderBy(m => m.Status),
            "status_desc" => q => q.OrderByDescending(m => m.Status),
            "date" => q => q.OrderBy(m => m.MembershipStartDate),
            "date_desc" => q => q.OrderByDescending(m => m.MembershipStartDate),
            "loans" => q => q.OrderBy(m => m.Loans.Count),
            "loans_desc" => q => q.OrderByDescending(m => m.Loans.Count),
            _ => q => q.OrderBy(m => m.Name)
        };
    }

    private IEnumerable<SelectListItem> GetMembershipStatusOptions()
    {
        return Enum.GetValues<MembershipStatus>()
                   .Select(s => new SelectListItem
                   {
                       Text = GetStatusDisplayName(s),
                       Value = s.ToString()
                   });
    }

    private string GetStatusDisplayName(MembershipStatus status)
    {
        return status switch
        {
            MembershipStatus.Active => "Aktiv",
            MembershipStatus.Suspended => "Avstängd",
            MembershipStatus.Expired => "Utgången",
            MembershipStatus.Cancelled => "Avbruten",
            MembershipStatus.Inactive => "InAktiv",
            _ => status.ToString()
        };
    }

    private string GenerateMemberReportContent(Member member, IEnumerable<Loan> loans, LoanStatistics stats)
    {
        var sb = new StringBuilder();

        // Rapportrubrik
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine($"MEDLEMSRAPPORT - {member.Name.ToUpper()}");
        sb.AppendLine("=".PadRight(80, '='));
        sb.AppendLine();
        sb.AppendLine($"Genererad: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Medlemsinformation
        sb.AppendLine("MEDLEMSINFORMATION:");
        sb.AppendLine($"Medlems-ID: #{member.Id}");
        sb.AppendLine($"Namn: {member.Name}");
        sb.AppendLine($"Personnummer: {member.SSN}");
        sb.AppendLine($"E-post: {member.Email}");
        sb.AppendLine($"Telefon: {member.PhoneNumber ?? "Ej angivet"}");
        sb.AppendLine($"Adress: {member.Address ?? "Ej angivet"}");
        sb.AppendLine($"Födelsedatum: {member.DateOfBirth:yyyy-MM-dd}");
        sb.AppendLine($"Medlemskap startade: {member.MembershipStartDate:yyyy-MM-dd}");
        sb.AppendLine($"Status: {GetStatusDisplayName(member.Status)}");
        sb.AppendLine($"Maximala lån: {member.MaxLoans}");
        sb.AppendLine();

        // Sammanfattande statistik
        sb.AppendLine("SAMMANFATTNING:");
        sb.AppendLine($"Totalt antal lån: {loans.Count()}");
        sb.AppendLine($"Aktiva lån: {stats.ActiveLoans}");
        sb.AppendLine($"Försenade lån: {stats.OverdueLoans}");
        sb.AppendLine($"Återlämnade idag: {stats.ReturnedLoans}");
        sb.AppendLine($"Totala utestående avgifter: {FormatSwedishCurrency(stats.TotalFees)}");
        sb.AppendLine();

        // Detaljerad lånlista
        sb.AppendLine("DETALJERAD LÅNHISTORIK:");
        sb.AppendLine("-".PadRight(80, '-'));

        foreach (var loan in loans.Take(100)) // Begränsa till 100 lån för prestanda
        {
            sb.AppendLine($"Lån #{loan.Id}");
            sb.AppendLine($"  Status: {GetSwedishLoanStatus(loan.Status)}");
            sb.AppendLine($"  Startdatum: {loan.StartDate:yyyy-MM-dd}");
            sb.AppendLine($"  Förfallodatum: {loan.DueDate:yyyy-MM-dd}");
            if (loan.ReturnDate.HasValue)
                sb.AppendLine($"  Återlämningsdatum: {loan.ReturnDate.Value:yyyy-MM-dd}");
            sb.AppendLine($"  Antal böcker: {loan.BookCopyLoans.Count}");
            if (loan.Fee > 0)
                sb.AppendLine($"  Förseningsavgift: {FormatSwedishCurrency(loan.Fee)}");
            if (loan.IsOverdue)
                sb.AppendLine($"  Dagar försenat: {loan.DaysOverdue}");
            if (!string.IsNullOrEmpty(loan.Notes))
                sb.AppendLine($"  Anteckningar: {loan.Notes}");

            sb.AppendLine("  Lånade böcker:");
            foreach (var bcl in loan.BookCopyLoans)
            {
                sb.AppendLine($"    - {bcl.BookCopy.Details.Title} (Författare: {bcl.BookCopy.Details.Author?.Name ?? "Okänd"}, ISBN: {bcl.BookCopy.Details.ISBN})");
            }
            sb.AppendLine();
        }

        if (loans.Count() > 100)
        {
            sb.AppendLine($"[Visar de första 100 lånen av totalt {loans.Count()}]");
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

    private CsvConfiguration GetCsvConfig(StreamReader reader = null, StreamWriter writer = null)
    {
        // Default delimiter (use ';' for sv-SE culture, common in Europe for CSVs)
        string delimiter = ";";

        // Detect delimiter if reader is provided (for reading scenarios)
        if (reader != null)
        {
            var position = reader.BaseStream.Position;
            try
            {
                var firstLine = reader.ReadLine();
                Debug.WriteLine($"First line read: '{firstLine}'");
                if (!string.IsNullOrEmpty(firstLine))
                {
                    // Improved delimiter detection (integrated from DetectDelimiter)
                    var delimiters = new[] { ',', ';', '\t' };
                    var delimiterCounts = delimiters.ToDictionary(d => d, d => 0);

                    bool inQuotes = false;
                    foreach (char c in firstLine)
                    {
                        if (c == '"') inQuotes = !inQuotes;

                        if (!inQuotes && delimiterCounts.TryGetValue(c, out int value))
                        {
                            delimiterCounts[c] = ++value;
                        }
                    }

                    // Select most frequent delimiter
                    var detected = delimiterCounts.OrderByDescending(kv => kv.Value)
                                                 .FirstOrDefault(kv => kv.Value > 0)
                                                 .Key;

                    if (detected != default)
                    {
                        delimiter = detected.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error and fall back to default (don't throw to keep method robust)
                Debug.WriteLine($"Delimiter detection failed: {ex.Message}. Using default '{delimiter}'.");
            }
            finally
            {
                // Reset stream position
                if (reader.BaseStream.CanSeek)
                {
                    reader.BaseStream.Seek(position, SeekOrigin.Begin);
                    reader.DiscardBufferedData();
                }
            }
        }

        // If writer is provided and reader is null, align encoding with writer for consistency (vice versa handling)
        Encoding encoding = new UTF8Encoding(false); // Default no-BOM UTF-8
        if (writer != null && reader == null)
        {
            encoding = writer.Encoding; // Use writer's encoding if available
        }

        // Create and return the config (suitable for both reading and writing)
        return new CsvConfiguration(CultureInfo.GetCultureInfo("sv-SE"))
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
            Mode = CsvMode.RFC4180, // Use strict CSV mode
            Encoding = encoding, // Dynamic based on writer if provided
            BadDataFound = context =>
            {
                // Log bad data but don't throw (useful for reading)
                Debug.WriteLine($"Bad data found at row {context.Field}: {context.RawRecord}");
            },
            MissingFieldFound = null, // Ignore missing fields
            HeaderValidated = null, // Disable header validation
            IgnoreBlankLines = true,
            PrepareHeaderForMatch = args => args.Header.Replace("\"", "").Trim(),
            TrimOptions = TrimOptions.Trim, // Trim whitespace from fields
            ShouldQuote = args => true, // Quote all fields (useful for writing)
            Quote = '"',
            Escape = '"',
            //WhiteSpaceChars = new[] { ' ', '\t', ';' }, // Treat semicolons as whitespace (uncomment if needed)
        };
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Currency Formatting Helper
    private string FormatSwedishCurrency(decimal? amount)
    {
        return amount.HasValue ? FormatSwedishCurrency(amount.Value) : "0,00 kr";  // Calls the non-nullable method
    }

    // Non-nullable overload (included for completeness)
    private static string FormatSwedishCurrency(decimal amount)
    {
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

    #endregion
}
