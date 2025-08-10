using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Domain.Utilities;
using Library.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Text;

namespace Library.MVC.Controllers;

#nullable disable
public class BooksController : Controller
{
    private readonly IBookService _bookService;
    private readonly IAuthorService _authorService;
    private readonly IBookCopyService _bookCopyService;
    private readonly ILoanService _loanService;
    private readonly IBookCopyLoanService _bookCopyLoanService;
    private readonly ILogger<BooksController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BooksController(
        IBookService bookservice,
        IAuthorService authorService,
        IBookCopyService bookCopyService,
        ILoanService loanService,
        IBookCopyLoanService bookCopyLoanService,
        ILogger<BooksController> logger,
        IWebHostEnvironment webHostEnvironment)
    {
        _bookService = bookservice;
        _authorService = authorService;
        _bookCopyService = bookCopyService;
        _loanService = loanService;
        _bookCopyLoanService = bookCopyLoanService;
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }

    #region CRUD Operations

    public async Task<IActionResult> Index(string search, int? authorId, string genre, bool? available, string sort = "title", int page = 1, int pageSize = 12)
    {
        try
        {
            // Build filter expression
            var books = await _bookService.GetAllBookDetailsAsync(
                filter: b =>
                    (string.IsNullOrEmpty(search) ||
                     b.Title.Contains(search) ||
                     b.ISBN.Contains(search) ||
                     b.Author.Name.Contains(search)) &&
                    (!authorId.HasValue || b.AuthorId == authorId.Value) &&
                    (string.IsNullOrEmpty(genre) || b.Genre == genre) &&
                    (!available.HasValue || b.Copies.Any(c => c.IsAvailable) == available.Value),
                orderBy: GetSortExpression(sort),
                a => a.Author,
                c => c.Copies);

            // Get filter options
            var authors = await _authorService.GetAllAsync();
            var allBooks = await _bookService.GetAllAsync();
            var uniqueGenres = allBooks.Where(b => !string.IsNullOrEmpty(b.Genre))
                                      .Select(b => b.Genre)
                                      .Distinct()
                                      .OrderBy(g => g);

            // Calculate pagination
            var totalItems = books.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedBooks = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new BookDetailsViewModel
            {
                SearchTerm = search,
                AuthorFilter = authorId,
                GenreFilter = genre,
                AvailabilityFilter = available,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalItems,
                TotalPages = totalPages,

                // Filter dropdowns
                Authors = authors.Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString(),
                    Selected = a.Id == authorId
                }).Prepend(new SelectListItem { Text = "All Authors", Value = "" }),

                Genres = uniqueGenres.Select(g => new SelectListItem
                {
                    Text = g,
                    Value = g,
                    Selected = g == genre
                }).Prepend(new SelectListItem { Text = "All Genres", Value = "" }),

                Statistics = new BookStatistics
                {
                    TotalBooks = totalItems,
                    TotalCopies = books.Sum(b => b.TotalCopies),
                    AvailableCopies = books.Sum(b => b.AvailableCopies),
                    BorrowedCopies = books.Sum(b => b.TotalCopies - b.AvailableCopies),
                    TotalAuthors = authors.Count(),
                    UniqueGenres = uniqueGenres.Count(),
                    AveragePages = (decimal)(books.Where(b => b.Pages.HasValue).Any() ?
                                 books.Where(b => b.Pages.HasValue).Average(b => b.Pages.Value) : 0),
                    MostPopularGenre = uniqueGenres.FirstOrDefault()
                }
            };

            ViewBag.Books = pagedBooks;
            ViewBag.SortBy = sort;
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading books index");
            TempData["Error"] = "Unable to load books. Please try again.";
            return View(new BookDetailsViewModel());
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var authors = await _authorService.GetAllAuthorsAsync(
                filter: null,
                orderBy: x => x.OrderBy(a => a.Name),
                includeProperties: a => a.Books);

            var viewModel = new BookDetailsViewModel
            {
                BookDetails = new BookDetails
                {
                    Language = "English",
                    PublicationDate = DateTime.Now.Date
                },
                CopiesToAdd = 1,
                InitialCondition = BookCondition.New,
                PurchaseDate = DateTime.Now.Date,
                Authors = authors.Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString()
                }),
                Genres = GetGenreOptions(),
                Languages = GetLanguageOptions(),
                Publishers = GetPublisherOptions(),
                BookConditions = GetBookConditionOptions()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading create book page");
            TempData["Error"] = "Unable to load create page. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookDetailsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await ReloadCreateView(model);
        }

        try
        {
            // Handle cover image
            string imageBinary = null;
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var imageResult = await ProcessCoverImageAsync(model.CoverImage);
                if (imageResult.Success)
                {
                    imageBinary = imageResult.ImageBase64;
                }
                else
                {
                    TempData["Error"] = imageResult.ErrorMessage;
                    return await ReloadCreateView(model);
                }
            }
            else
            {
                imageBinary = await GetDefaultImageAsync();
            }

            // Generate and validate ISBN
            string isbn = await GenerateUniqueIsbnAsync(model.BookDetails?.ISBN);

            // Check for duplicate title
            var existingBook = await _bookService.GetBookOrDefault(
                b => b.Title.ToLower() == model.BookDetails.Title.ToLower().Trim());

            if (existingBook != null)
            {
                ModelState.AddModelError("BookDetails.Title", "A book with this title already exists.");
                return await ReloadCreateView(model);
            }

            // Create new book with all properties
            var bookDetails = new BookDetails
            {
                AuthorId = model.BookDetails.AuthorId,
                ISBN = isbn,
                Title = model.BookDetails.Title.Trim(),
                Subtitle = model.BookDetails.Subtitle?.Trim(),
                Description = model.BookDetails.Description?.Trim() ?? "",
                ImageBinary = imageBinary,
                PublicationDate = model.BookDetails.PublicationDate,
                Pages = model.BookDetails.Pages,
                Publisher = model.BookDetails.Publisher?.Trim(),
                Language = model.BookDetails.Language ?? "English",
                Genre = model.BookDetails.Genre?.Trim(),
                Edition = model.BookDetails.Edition?.Trim(),
                CreatedDate = DateTime.UtcNow
            };

            var addedBook = await _bookService.AddAsync(bookDetails);

            // Add book copies
            for (int i = 0; i < model.CopiesToAdd; i++)
            {
                var bookCopy = new BookCopy
                {
                    DetailsId = addedBook.Id,
                    IsAvailable = true,
                    Condition = model.InitialCondition,
                    Location = model.Location?.Trim(),
                    PurchaseDate = model.PurchaseDate,
                    PurchasePrice = model.PurchasePrice,
                    Notes = model.Notes?.Trim(),
                    Barcode = GenerateBarcode(model.BarcodePrefix, i + 1),
                    CreatedDate = DateTime.UtcNow
                };

                await _bookCopyService.AddAsync(bookCopy);
            }

            TempData["Success"] = $"Book '{addedBook.Title}' created successfully with {model.CopiesToAdd} copies. ISBN: {isbn}";
            return RedirectToAction(nameof(Edit), new { id = addedBook.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating book: {Title}", model.BookDetails?.Title);
            TempData["Error"] = "An error occurred while creating the book. Please try again.";
            return await ReloadCreateView(model);
        }
    }

    private string GenerateBarcode(object barcodePrefix, int v)
    {
        throw new NotImplementedException();
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (id <= 0)
            return NotFound();

        try
        {
            var book = await _bookService.GetBookOrDefault(
                b => b.Id == id,
                includeProperties: "Author,Copies");

            if (book == null)
                return NotFound();

            var bookCopies = await _bookCopyService.GetAllBookCopiesAsync(
                filter: c => c.DetailsId == id,
                orderBy: x => x.OrderBy(c => c.Id));

            var loans = await _loanService.GetAllLoansAsync(
                filter: l => l.BookCopyLoans.Any(bcl => bcl.BookCopy.DetailsId == id),
                orderBy: x => x.OrderByDescending(l => l.StartDate),
                l => l.Member, l => l.BookCopyLoans);

            var viewModel = new BookDetailsViewModel
            {
                BookDetails = book,
                BookCopies = bookCopies.Select(c => new SelectListItem
                {
                    Text = $"Copy #{c.Id} - {c.Condition} - {(c.IsAvailable ? "Available" : "On Loan")} - {c.Location}",
                    Value = c.Id.ToString()
                }),
                CopiesToAdd = bookCopies.Count(),
                Statistics = new BookStatistics
                {
                    TotalCopies = book.TotalCopies,
                    AvailableCopies = book.AvailableCopies,
                    BorrowedCopies = book.TotalCopies - book.AvailableCopies
                }
            };

            ViewBag.BookCopies = bookCopies;
            ViewBag.RecentLoans = loans.Take(5);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading book details for ID: {BookId}", id);
            return NotFound();
        }
    }


    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Edit(int id, BookDetailsViewModel model)
    //{
    //    if (id <= 0 || model.BookDetails == null)
    //        return NotFound();

    //    if (!ModelState.IsValid)
    //    {
    //        return await ReloadEditView(id, model);
    //    }

    //    try
    //    {
    //        var existingBook = await _bookService.GetBookOrDefaultAsync(
    //            b => b.Id == id,
    //            includeProperties: "Author,Copies");

    //        if (existingBook == null)
    //            return NotFound();

    //        // Handle cover image upload
    //        if (model.CoverImage != null && model.CoverImage.Length > 0)
    //        {
    //            var imageResult = await ProcessCoverImageAsync(model.CoverImage);
    //            if (imageResult.Success)
    //            {
    //                existingBook.ImageBinary = imageResult.ImageBase64;
    //            }
    //            else
    //            {
    //                TempData["Error"] = imageResult.ErrorMessage;
    //                return await ReloadEditView(id, model);
    //            }
    //        }

    //        // Check for duplicate title (excluding current book)
    //        var duplicateBook = await _bookService.GetBookOrDefaultAsync(
    //            b => b.Title.ToLower() == model.BookDetails.Title.ToLower().Trim() && b.Id != id);

    //        if (duplicateBook != null)
    //        {
    //            ModelState.AddModelError("BookDetails.Title", "A book with this title already exists.");
    //            return await ReloadEditView(id, model);
    //        }

    //        // Update all properties
    //        existingBook.AuthorId = model.BookDetails.AuthorId;
    //        existingBook.Title = model.BookDetails.Title.Trim();
    //        existingBook.Subtitle = model.BookDetails.Subtitle?.Trim();
    //        existingBook.Description = model.BookDetails.Description?.Trim() ?? "";
    //        existingBook.PublicationDate = model.BookDetails.PublicationDate;
    //        existingBook.Pages = model.BookDetails.Pages;
    //        existingBook.Publisher = model.BookDetails.Publisher?.Trim();
    //        existingBook.Language = model.BookDetails.Language ?? "English";
    //        existingBook.Genre = model.BookDetails.Genre?.Trim();
    //        existingBook.Edition = model.BookDetails.Edition?.Trim();
    //        existingBook.UpdatedDate = DateTime.UtcNow;

    //        await _bookService.UpdateAsync(existingBook);

    //        TempData["Success"] = $"Book '{existingBook.Title}' updated successfully.";
    //        return RedirectToAction(nameof(Edit), new { id = existingBook.Id });
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error updating book ID: {BookId}", id);
    //        TempData["Error"] = "An error occurred while updating the book. Please try again.";
    //        return await ReloadEditView(id, model);
    //    }
    //}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookDetailsViewModel model)
    {
        if (id <= 0 || model.BookDetails == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            return await ReloadEditView(id, model);
        }

        try
        {
            var existingBook = await _bookService.GetBookOrDefault(
                b => b.Id == id,
                includeProperties: "Author,Copies");

            if (existingBook == null)
                return NotFound();

            // Handle cover image upload
            if (model.CoverImage != null && model.CoverImage.Length > 0)
            {
                var imageResult = await ProcessCoverImageAsync(model.CoverImage);
                if (imageResult.Success)
                {
                    existingBook.ImageBinary = imageResult.ImageBase64;
                }
                else
                {
                    TempData["Error"] = imageResult.ErrorMessage;
                    return await ReloadEditView(id, model);
                }
            }

            // Check for duplicate title (excluding current book)
            var duplicateBook = await _bookService.GetBookOrDefault(
                b => b.Title.ToLower() == model.BookDetails.Title.ToLower().Trim() && b.Id != id);

            if (duplicateBook != null)
            {
                ModelState.AddModelError("BookDetails.Title", "A book with this title already exists.");
                return await ReloadEditView(id, model);
            }

            // Merge updates: Only set properties if provided in model
            if (!string.IsNullOrEmpty(model.BookDetails.Title))
                existingBook.Title = model.BookDetails.Title.Trim();

            if (!string.IsNullOrEmpty(model.BookDetails.Subtitle))
                existingBook.Subtitle = model.BookDetails.Subtitle.Trim();

            if (!string.IsNullOrEmpty(model.BookDetails.Description))
                existingBook.Description = model.BookDetails.Description.Trim();

            if (model.BookDetails.AuthorId > 0)
                existingBook.AuthorId = model.BookDetails.AuthorId;

            if (model.BookDetails.Pages.HasValue)
                existingBook.Pages = model.BookDetails.Pages;

            if (model.BookDetails.PublicationDate.HasValue)
                existingBook.PublicationDate = model.BookDetails.PublicationDate;

            if (!string.IsNullOrEmpty(model.BookDetails.Publisher))
                existingBook.Publisher = model.BookDetails.Publisher.Trim();

            if (!string.IsNullOrEmpty(model.BookDetails.Language))
                existingBook.Language = model.BookDetails.Language;

            if (!string.IsNullOrEmpty(model.BookDetails.Genre))
                existingBook.Genre = model.BookDetails.Genre.Trim();

            if (!string.IsNullOrEmpty(model.BookDetails.Edition))
                existingBook.Edition = model.BookDetails.Edition.Trim();

            existingBook.UpdatedDate = DateTime.UtcNow;

            await _bookService.UpdateAsync(existingBook);

            TempData["Success"] = $"Book '{existingBook.Title}' updated successfully.";
            return RedirectToAction(nameof(Edit), new { id = existingBook.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book ID: {BookId}", id);
            TempData["Error"] = "An error occurred while updating the book. Please try again.";
            return await ReloadEditView(id, model);
        }
    }


    public async Task<IActionResult> Details(int id)
    {
        if (id <= 0)
            return NotFound();

        try
        {
            var book = await _bookService.GetBookOrDefault(
                b => b.Id == id,
                includeProperties: "Author,Copies");

            if (book == null)
                return NotFound();

            var authors = await _authorService.GetAllAsync();

            var viewModel = new BookDetailsViewModel
            {
                BookDetails = book,
                Authors = authors.Select(a => new SelectListItem
                {
                    Text = a.Name,
                    Value = a.Id.ToString(),
                    Selected = a.Id == book.AuthorId
                }),
                Genres = GetGenreOptions(),
                Languages = GetLanguageOptions(),
                Publishers = GetPublisherOptions(),
                BookConditions = GetBookConditionOptions()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit page for book ID: {BookId}", id);
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> History(int bookId, int page = 1, int pageSize = 10)
    {
        try
        {
            // Get the book details with related data
            var book = await _bookService.GetByIdAsync(bookId);
            if (book == null)
                return NotFound();

            // Get all loans for this book
            var bookLoans = await _loanService.GetAllLoansAsync(
                filter: l => l.BookCopyLoans.Any(bcl => bcl.BookCopy.DetailsId == bookId),
                orderBy: x => x.OrderByDescending(l => l.StartDate),
                includeProperties: "Member,BookCopyLoans,BookCopyLoans.BookCopy,BookCopyLoans.BookCopy.Details"
            );

            // Get all book copies for this book
            var bookCopies = await _bookCopyService.GetAllBookCopiesAsync(
                filter: bc => bc.DetailsId == bookId,
                orderBy: x => x.OrderBy(bc => bc.Id),
                bc => bc.Details,
                bc => bc.BookCopyLoans
            );

            // Get related books (same author or genre)
            var relatedBooks = await _bookService.GetAllBookDetailsAsync(
                filter: b => b.Id != bookId &&
                            (b.AuthorId == book.AuthorId || b.Genre == book.Genre),
                orderBy: x => x.OrderBy(b => b.Title),
                b => b.Author
            );

            // Calculate book statistics
            var bookStats = new BookHistoryStatistics
            {
                TotalLoans = bookLoans.Count,
                ActiveLoans = bookLoans.Count(l => l.Status == LoanStatus.Active),
                OverdueLoans = bookLoans.Count(l => l.Status == LoanStatus.Overdue ||
                                              (l.Status == LoanStatus.Active && l.DueDate < DateTime.UtcNow)),
                ReturnedLoans = bookLoans.Count(l => l.Status == LoanStatus.Returned),
                UniqueBorrowers = bookLoans.Select(l => l.MemberId).Distinct().Count(),
                TotalFees = bookLoans.Sum(l => l.Fee),
                PopularityScore = CalculatePopularityScore(bookLoans),
                AverageLoanDays = CalculateAverageLoanDays(bookLoans),
                FirstLoanDate = bookLoans.Any() ? bookLoans.Min(l => l.StartDate) : DateTime.MinValue,
                LastLoanDate = bookLoans.Any() ? bookLoans.Max(l => l.StartDate) : DateTime.MinValue,
                TotalCopies = bookCopies.Count,
                AvailableCopies = bookCopies.Count(bc => bc.IsAvailable)
            };

            // Calculate pagination for loans
            var totalItems = bookLoans.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedLoans = bookLoans.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new BookHistoryViewModel
            {
                Book = book,
                Loans = pagedLoans,
                BookCopies = bookCopies,
                Statistics = bookStats
            };

            // Set ViewBag properties that the view expects
            ViewBag.BookLoans = bookLoans;
            ViewBag.BookStatistics = bookStats;
            ViewBag.BookCopies = bookCopies;
            ViewBag.RelatedBooks = relatedBooks.Take(6);
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.PageSize = pageSize;

            return View("History", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fel vid laddning av bokhistorik för ID: {BookId}", bookId);
            TempData["Error"] = "Kunde inte ladda bokhistorik.";
            return RedirectToAction("Index", "Books");
        }
    }
    #endregion

    #region File Operations

    [HttpGet]
    public async Task<IActionResult> DownloadCover(int id)
    {
        try
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book?.ImageBinary == null)
                return NotFound("Cover image not found");

            // Convert base64 to bytes
            byte[] imageBytes;
            string contentType = "image/jpeg";

            if (book.ImageBinary.StartsWith("data:image"))
            {
                var parts = book.ImageBinary.Split(',');
                if (parts.Length == 2)
                {
                    // Extract content type
                    var header = parts[0];
                    if (header.Contains("image/png")) contentType = "image/png";
                    else if (header.Contains("image/webp")) contentType = "image/webp";

                    imageBytes = Convert.FromBase64String(parts[1]);
                }
                else
                {
                    return BadRequest("Invalid image format");
                }
            }
            else
            {
                imageBytes = Convert.FromBase64String(book.ImageBinary);
            }

            var fileName = $"{SanitizeFileName(book.Title)}_cover.jpg";
            return File(imageBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading cover for book {BookId}", id);
            return BadRequest("Error downloading cover");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportToCsv()
    {
        try
        {
            var books = await _bookService.GetAllBookDetailsAsync(
                filter: null,
                orderBy: x => x.OrderBy(b => b.Title),
                a => a.Author,
                c => c.Copies);

            var csv = GenerateCsvContent(books);
            var bytes = Encoding.UTF8.GetBytes(csv);

            var fileName = $"books_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting books to CSV");
            return BadRequest("Export failed");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportToJson()
    {
        try
        {
            var books = await _bookService.GetAllBookDetailsAsync(
                filter: null,
                orderBy: x => x.OrderBy(b => b.Title),
                a => a.Author,
                c => c.Copies);

            var jsonData = books.Select(b => new
            {
                Id = b.Id,
                Title = b.Title,
                Subtitle = b.Subtitle,
                Author = b.Author?.Name,
                ISBN = b.ISBN,
                Genre = b.Genre,
                Language = b.Language,
                Publisher = b.Publisher,
                Pages = b.Pages,
                PublicationDate = b.PublicationDate?.ToString("yyyy-MM-dd"),
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                Description = b.Description
            });

            var json = System.Text.Json.JsonSerializer.Serialize(jsonData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var bytes = Encoding.UTF8.GetBytes(json);
            var fileName = $"books_export_{DateTime.Now:yyyyMMdd_HHmmss}.json";

            return File(bytes, "application/json", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting books to JSON");
            return BadRequest("Export failed");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GenerateBookReport(int id)
    {
        try
        {
            var book = await _bookService.GetBookOrDefault(
                b => b.Id == id,
                includeProperties: "Author,Copies");

            if (book == null)
                return NotFound();

            var bookCopies = await _bookCopyService.GetAllBookCopiesAsync(
                filter: c => c.DetailsId == id,
                orderBy: x => x.OrderBy(c => c.Id));

            var loans = await _loanService.GetAllLoansAsync(
                filter: l => l.BookCopyLoans.Any(bcl => bcl.BookCopy.DetailsId == id),
                orderBy: x => x.OrderByDescending(l => l.StartDate),
                l => l.Member, l => l.BookCopyLoans);

            var reportContent = GenerateBookReportContent(book, bookCopies, loans);
            var bytes = Encoding.UTF8.GetBytes(reportContent);

            var fileName = $"book_report_{SanitizeFileName(book.Title)}_{DateTime.Now:yyyyMMdd}.txt";
            return File(bytes, "text/plain", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating report for book {BookId}", id);
            return BadRequest("Report generation failed");
        }
    }

    #endregion

    #region Helper Methods

    private async Task<IActionResult> ReloadCreateView(BookDetailsViewModel model)
    {
        var authors = await _authorService.GetAllAuthorsAsync(
            filter: null,
            orderBy: x => x.OrderBy(a => a.Name),
            includeProperties: a => a.Books);

        model.Authors = authors.Select(a => new SelectListItem
        {
            Text = a.Name,
            Value = a.Id.ToString()
        });

        model.Genres = GetGenreOptions();
        model.Languages = GetLanguageOptions();
        model.Publishers = GetPublisherOptions();
        model.BookConditions = GetBookConditionOptions();

        return View(model);
    }

    private async Task<IActionResult> ReloadEditView(int id, BookDetailsViewModel model)
    {
        var authors = await _authorService.GetAllAsync();
        model.Authors = authors.Select(a => new SelectListItem
        {
            Text = a.Name,
            Value = a.Id.ToString()
        });

        model.Genres = GetGenreOptions();
        model.Languages = GetLanguageOptions();
        model.Publishers = GetPublisherOptions();
        model.BookConditions = GetBookConditionOptions();

        return View(model);
    }

    private async Task<ImageProcessResult> ProcessCoverImageAsync(IFormFile imageFile)
    {
        const int maxFileSize = 5 * 1024 * 1024; // 5MB
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };

        try
        {
            if (imageFile.Length > maxFileSize)
            {
                return new ImageProcessResult
                {
                    Success = false,
                    ErrorMessage = "Image file size must be less than 5MB."
                };
            }

            if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
            {
                return new ImageProcessResult
                {
                    Success = false,
                    ErrorMessage = "Only JPEG, PNG, and WEBP images are allowed."
                };
            }

            using var memoryStream = new MemoryStream();
            await imageFile.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();
            var base64String = Convert.ToBase64String(imageBytes);
            var dataUri = $"data:{imageFile.ContentType};base64,{base64String}";

            return new ImageProcessResult
            {
                Success = true,
                ImageBase64 = dataUri
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image");
            return new ImageProcessResult
            {
                Success = false,
                ErrorMessage = "Error processing image: " + ex.Message
            };
        }
    }

    private async Task<string> GetDefaultImageAsync()
    {
        try
        {
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "9780555816023.png");
            if (System.IO.File.Exists(path))
            {
                var bytes = await System.IO.File.ReadAllBytesAsync(path);
                return "data:image/png;base64," + Convert.ToBase64String(bytes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load default image");
        }
        return null;
    }

    private async Task<string> GenerateUniqueIsbnAsync(string providedIsbn = null)
    {
        string isbn = providedIsbn?.Trim();

        if (string.IsNullOrWhiteSpace(isbn) ||
            isbn == "0000000000000" ||
            !IsbnGenerator.IsValidIsbn13(isbn))
        {
            isbn = IsbnGenerator.GenerateRandomIsbn13();
        }

        var existingBooks = await _bookService.GetAllAsync();
        int attempts = 0;
        const int maxAttempts = 10;

        while (attempts < maxAttempts)
        {
            var duplicate = existingBooks.FirstOrDefault(b =>
                b.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase));

            if (duplicate == null) break;

            isbn = IsbnGenerator.GenerateRandomIsbn13();
            attempts++;
        }

        return isbn;
    }

    private string GenerateBarcode(string prefix, int copyNumber)
    {
        if (string.IsNullOrEmpty(prefix))
            prefix = "BC";

        return $"{prefix}{DateTime.Now:yyyyMMdd}{copyNumber:D3}";
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    private string GenerateCsvContent(IEnumerable<BookDetails> books)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Title,Subtitle,Author,ISBN,Genre,Language,Publisher,Pages,Publication Date,Total Copies,Available Copies,Average Purchase Price,Description");

        foreach (var book in books)
        {
            // Calculate average purchase price in Swedish currency
            var avgPrice = book.Copies?.Where(c => c.PurchasePrice.HasValue)
                                      .Average(c => c.PurchasePrice.Value) ?? 0;
            var avgPriceFormatted = avgPrice > 0 ? FormatSwedishCurrency(avgPrice) : "N/A";

            var line = $"\"{book.Title?.Replace("\"", "\"\"")}\",\"{book.Subtitle?.Replace("\"", "\"\"")}\",\"{book.Author?.Name?.Replace("\"", "\"\"")}\",\"{book.ISBN}\",\"{book.Genre?.Replace("\"", "\"\"")}\",\"{book.Language}\",\"{book.Publisher?.Replace("\"", "\"\"")}\",{book.Pages},\"{book.PublicationDate:yyyy-MM-dd}\",{book.TotalCopies},{book.AvailableCopies},\"{avgPriceFormatted}\",\"{book.Description?.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "")}\"";
            sb.AppendLine(line);
        }

        return sb.ToString();
    }
 
    private string GenerateBookReportContent(BookDetails book, IEnumerable<BookCopy> copies, IEnumerable<Loan> loans)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine($"BOOK REPORT - {book.Title.ToUpper()}");
        sb.AppendLine("=".PadRight(60, '='));
        sb.AppendLine();

        sb.AppendLine("BOOK INFORMATION:");
        sb.AppendLine($"Title: {book.Title}");
        if (!string.IsNullOrEmpty(book.Subtitle)) sb.AppendLine($"Subtitle: {book.Subtitle}");
        sb.AppendLine($"Author: {book.Author?.Name ?? "Unknown"}");
        sb.AppendLine($"ISBN: {book.ISBN}");
        sb.AppendLine($"Genre: {book.Genre ?? "Not specified"}");
        sb.AppendLine($"Language: {book.Language ?? "Not specified"}");
        sb.AppendLine($"Publisher: {book.Publisher ?? "Not specified"}");
        sb.AppendLine($"Pages: {book.Pages?.ToString() ?? "Not specified"}");
        sb.AppendLine($"Publication Date: {book.PublicationDate?.ToString("yyyy-MM-dd") ?? "Not specified"}");
        sb.AppendLine();

        sb.AppendLine("COPY INFORMATION:");
        sb.AppendLine($"Total Copies: {book.TotalCopies}");
        sb.AppendLine($"Available Copies: {book.AvailableCopies}");
        sb.AppendLine($"Borrowed Copies: {book.TotalCopies - book.AvailableCopies}");
        sb.AppendLine();

        if (copies.Any())
        {
            sb.AppendLine("COPY DETAILS:");
            foreach (var copy in copies.OrderBy(c => c.Id))
            {
                sb.AppendLine($"- Copy #{copy.Id}: {copy.Condition}, {(copy.IsAvailable ? "Available" : "On Loan")}, Location: {copy.Location ?? "Not specified"}");
                // ✅ Add purchase price in Swedish currency if available
                if (copy.PurchasePrice.HasValue)
                {
                    sb.AppendLine($"  Purchase Price: {FormatSwedishCurrency(copy.PurchasePrice.Value)}");
                }
            }
            sb.AppendLine();
        }

        if (loans.Any())
        {
            sb.AppendLine("RECENT LOAN HISTORY (Last 10):");
            decimal totalFees = 0;
            foreach (var loan in loans.Take(10))
            {
                // Format fees in Swedish currency
                var feeText = loan.Fee > 0 ? $" (Fee: {FormatSwedishCurrency(loan.Fee)})" : "";
                sb.AppendLine($"- {loan.Member?.Name}: {loan.StartDate:yyyy-MM-dd} to {(loan.ReturnDate?.ToString("yyyy-MM-dd") ?? "Not returned")}{feeText}");
                totalFees += loan.Fee;
            }

            // Total fees summary
            if (totalFees > 0)
            {
                sb.AppendLine();
                sb.AppendLine($"Total Outstanding Fees: {FormatSwedishCurrency(totalFees)}");
            }
            sb.AppendLine();
        }

        if (!string.IsNullOrEmpty(book.Description))
        {
            sb.AppendLine("DESCRIPTION:");
            sb.AppendLine(book.Description);
            sb.AppendLine();
        }

        sb.AppendLine($"Report generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

        return sb.ToString();
    }
 
    private Func<IQueryable<BookDetails>, IOrderedQueryable<BookDetails>> GetSortExpression(string sort)
    {
        return sort?.ToLower() switch
        {
            "title_desc" => q => q.OrderByDescending(b => b.Title),
            "author" => q => q.OrderBy(b => b.Author.Name),
            "author_desc" => q => q.OrderByDescending(b => b.Author.Name),
            "isbn" => q => q.OrderBy(b => b.ISBN),
            "isbn_desc" => q => q.OrderByDescending(b => b.ISBN),
            "date" => q => q.OrderBy(b => b.PublicationDate),
            "date_desc" => q => q.OrderByDescending(b => b.PublicationDate),
            "copies" => q => q.OrderBy(b => b.Copies.Count),
            "copies_desc" => q => q.OrderByDescending(b => b.Copies.Count),
            _ => q => q.OrderBy(b => b.Title)
        };
    }

    private IEnumerable<SelectListItem> GetGenreOptions()
    {
        var genres = new[]
        {
            "Fiction", "Non-Fiction", "Mystery", "Romance", "Thriller", "Science Fiction",
            "Fantasy", "Biography", "History", "Self-Help", "Technology", "Business",
            "Health", "Travel", "Children", "Young Adult", "Poetry", "Drama", "Comedy",
            "Horror", "Adventure", "Crime", "Philosophy", "Art", "Music", "Sports"
        };

        return genres.Select(g => new SelectListItem { Text = g, Value = g });
    }

    private IEnumerable<SelectListItem> GetLanguageOptions()
    {
        var languages = new[]
        {
            "English", "Swedish", "Spanish", "French", "German", "Italian",
            "Portuguese", "Dutch", "Russian", "Chinese", "Japanese", "Korean",
            "Arabic", "Hindi", "Norwegian", "Danish", "Finnish", "Other"
        };

        return languages.Select(l => new SelectListItem { Text = l, Value = l });
    }

    private IEnumerable<SelectListItem> GetPublisherOptions()
    {
        var publishers = new[]
        {
            "Penguin Random House", "HarperCollins", "Macmillan", "Simon & Schuster",
            "Hachette", "Pearson", "Wiley", "Oxford University Press", "Cambridge University Press",
            "Springer", "Elsevier", "McGraw-Hill", "Cengage Learning", "Scholastic"
        };

        return publishers.Select(p => new SelectListItem { Text = p, Value = p });
    }

    private IEnumerable<SelectListItem> GetBookConditionOptions()
    {
        return Enum.GetValues<BookCondition>()
                   .Select(c => new SelectListItem
                   {
                       Text = c.ToString(),
                       Value = c.ToString()
                   });
    }
 
    private static int CalculatePopularityScore(IEnumerable<Loan> loans)
    {
        if (!loans.Any()) return 0;

        var now = DateTime.UtcNow;
        var totalScore = 0;

        foreach (var loan in loans)
        {
            var daysSinceStart = (now - loan.StartDate).Days;
            var recencyScore = Math.Max(0, 365 - daysSinceStart) / 365.0;
            totalScore += (int)(10 * recencyScore);
        }

        return Math.Min(100, totalScore);
    }

    private static int CalculateAverageLoanDays(IEnumerable<Loan> loans)
    {
        var completedLoans = loans.Where(l => l.ReturnDate.HasValue).ToList();

        if (!completedLoans.Any()) return 0;

        var totalDays = completedLoans.Sum(l => (l.ReturnDate!.Value - l.StartDate).Days);
        return totalDays / completedLoans.Count;
    }

    #endregion

    #region Currency Formatting Helper

    private string FormatSwedishCurrency(decimal amount)
    {
        // Swedish Krona formatting: 1.234,56 kr
        var swedishCulture = new CultureInfo("sv-SE");

        // Customize to show currency symbol after the amount
        var numberFormat = (NumberFormatInfo)swedishCulture.NumberFormat.Clone();
        numberFormat.CurrencyPositivePattern = 3; // n $ (number space currency)
        numberFormat.CurrencyNegativePattern = 8; // -n $ (negative number space currency)
        numberFormat.CurrencySymbol = "kr";

        return amount.ToString("C", numberFormat);
    }

    private string FormatSwedishCurrency(decimal? amount)
    {
        return amount.HasValue ? FormatSwedishCurrency(amount.Value) : "0,00 kr";
    }

    #endregion

    #region API Endpoints

    [HttpGet]
    public async Task<IActionResult> GetBook(int id)
    {
        try
        {
            var book = await _bookService.GetBookOrDefault(
                b => b.Id == id,
                includeProperties: "Author,Copies");

            if (book == null)
                return NotFound();

            // Calculate total fees for this book from recent loans
            var recentLoans = await _loanService.GetAllLoansAsync(
                filter: l => l.BookCopyLoans.Any(bcl => bcl.BookCopy.DetailsId == id),
                orderBy: x => x.OrderByDescending(l => l.StartDate),
                l => l.Member, l => l.BookCopyLoans);

            var totalOutstandingFees = recentLoans.Where(l => l.Fee > 0).Sum(l => l.Fee);

            var result = new
            {
                id = book.Id,
                title = book.Title,
                subtitle = book.Subtitle,
                author = book.Author?.Name ?? "Unknown",
                isbn = book.ISBN,
                description = book.Description ?? "No description",
                genre = book.Genre,
                language = book.Language,
                publisher = book.Publisher,
                pages = book.Pages,
                publicationDate = book.PublicationDate?.ToString("yyyy-MM-dd"),
                totalCopies = book.TotalCopies,
                availableCopies = book.AvailableCopies,
                isAvailable = book.IsAvailable,
                imageBinary = book.ImageBinary ?? "/uploads/9780555816023.png",
                // Swedish currency formatted fees
                totalOutstandingFees = FormatSwedishCurrency(totalOutstandingFees),
                totalOutstandingFeesRaw = totalOutstandingFees
            };

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting book details for ID: {BookId}", id);
            return BadRequest(new { error = "Unable to retrieve book details" });
        }
    }


    [HttpGet]
    public async Task<IActionResult> GetAll(string status = "all", string search = "")
    {
        try
        {
            var books = await _bookService.GetAllBookDetailsAsync(
                filter: b =>
                    (string.IsNullOrEmpty(search) ||
                     b.Title.Contains(search) ||
                     b.Author.Name.Contains(search) ||
                     b.ISBN.Contains(search)) &&
                    (status == "all" ||
                     (status == "available" && b.Copies.Any(c => c.IsAvailable)) ||
                     (status == "unavailable" && !b.Copies.Any(c => c.IsAvailable))),
                orderBy: x => x.OrderBy(b => b.Title),
                a => a.Author,
                c => c.Copies);

            var result = books.Select(b => new
            {
                id = b.Id,
                title = b.Title,
                subtitle = b.Subtitle,
                author = b.Author?.Name ?? "Unknown",
                isbn = b.ISBN,
                genre = b.Genre,
                totalCopies = b.TotalCopies,
                availableCopies = b.AvailableCopies,
                isAvailable = b.IsAvailable,
                imageBinary = b.ImageBinary ?? "/uploads/9780555816023.png"
            });

            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all books");
            return BadRequest(new { success = false, error = "Unable to retrieve books" });
        }
    }

    [HttpPost]
    public IActionResult GenerateIsbn()
    {
        try
        {
            var isbn = IsbnGenerator.GenerateRandomIsbn13();
            return Json(new { success = true, isbn = isbn });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating ISBN");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddBookCopy(int id, BookCondition condition = BookCondition.New, string location = "", decimal? purchasePrice = null)
    {
        try
        {
            var book = await _bookService.GetBookOrDefault(
                b => b.Id == id,
                includeProperties: "Copies");

            if (book == null)
                return Json(new { success = false, message = "Book not found." });

            const int maxCopies = 10;
            if (book.Copies.Count >= maxCopies)
            {
                return Json(new
                {
                    success = false,
                    message = $"Cannot add more copies. Maximum limit of {maxCopies} copies reached for \"{book.Title}\"."
                });
            }

            var newCopy = new BookCopy
            {
                DetailsId = book.Id,
                IsAvailable = true,
                Condition = condition,
                Location = location?.Trim(),
                PurchasePrice = purchasePrice,
                PurchaseDate = DateTime.Now,
                Barcode = GenerateBarcode("BC", book.Copies.Count + 1),
                CreatedDate = DateTime.UtcNow
            };

            await _bookCopyService.AddAsync(newCopy);

            var updatedBook = await _bookService.GetBookOrDefault(
                b => b.Id == id,
                includeProperties: "Copies");

            return Json(new
            {
                success = true,
                message = $"Book copy #{newCopy.Id} added successfully to \"{book.Title}\".",
                copyId = newCopy.Id,
                totalCopies = updatedBook.TotalCopies,
                availableCopies = updatedBook.AvailableCopies,
                maxLimitReached = updatedBook.TotalCopies >= maxCopies,
                bookId = book.Id,
                bookTitle = book.Title
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book copy for book ID: {BookId}", id);
            return Json(new
            {
                success = false,
                message = $"Error adding copy: {ex.Message}"
            });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteSingle(int id)
    {
        try
        {
            var bookCopy = await _bookCopyService.GetBookCopyOrDefaultAsync(
                filter: b => b.Id == id);

            if (bookCopy == null)
                return Json(new { success = false, message = "Book copy not found." });

            var bookCopyLoan = await _bookCopyLoanService.GetBookCopyLoanOrDefaultAsync(
                x => x.BookCopyId == bookCopy.Id);

            if (bookCopyLoan != null)
            {
                return Json(new
                {
                    success = false,
                    message = "This book copy cannot be deleted because it is currently on loan. Please return it first."
                });
            }

            await _bookCopyService.DeleteAsync(id);

            return Json(new
            {
                success = true,
                message = $"Book copy #{id} deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book copy ID: {CopyId}", id);
            return Json(new
            {
                success = false,
                message = "An error occurred while deleting the book copy."
            });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var book = await _bookService.GetBookOrDefault(
                b => b.Id == id,
                includeProperties: "Copies");

            if (book == null)
            {
                return Json(new { success = false, message = "Book not found." });
            }

            var bookCopies = await _bookCopyService.GetAllBookCopiesAsync(
                filter: c => c.DetailsId == book.Id);

            var copiesToDelete = new List<BookCopy>();
            foreach (var bookCopy in bookCopies)
            {
                var bookCopyLoan = await _bookCopyLoanService.GetBookCopyLoanOrDefaultAsync(
                    x => x.BookCopyId == bookCopy.Id);

                if (bookCopyLoan == null)
                    copiesToDelete.Add(bookCopy);
            }

            if (copiesToDelete.Count < bookCopies.Count())
            {
                return Json(new
                {
                    success = false,
                    message = "Cannot delete this book because some copies are currently on loan. Please return all copies first."
                });
            }

            if (copiesToDelete.Any())
            {
                _bookCopyService.RemoveRange(copiesToDelete);
            }

            await _bookService.DeleteAsync(book.Id);

            return Json(new
            {
                success = true,
                message = $"Book '{book.Title}' and all related copies deleted successfully."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book ID: {BookId}", id);
            return Json(new
            {
                success = false,
                message = "An error occurred while deleting the book."
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new { success = true, data = new object[0] });

            var books = await _bookService.GetAllBookDetailsAsync(
                filter: b => b.Title.Contains(term) ||
                            b.ISBN.Contains(term) ||
                            b.Author.Name.Contains(term) ||
                            (b.Genre != null && b.Genre.Contains(term)),
                orderBy: x => x.OrderBy(b => b.Title),
                a => a.Author);

            var result = books.Take(10).Select(b => new
            {
                id = b.Id,
                title = b.Title,
                author = b.Author?.Name ?? "Unknown",
                isbn = b.ISBN,
                genre = b.Genre,
                isAvailable = b.IsAvailable
            });

            return Json(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching books with term: {SearchTerm}", term);
            return Json(new { success = false, error = "Search failed" });
        }
    }

    #endregion

    public class ImageProcessResult
    {
        public bool Success { get; set; }
        public string ImageBase64 { get; set; }
        public string ErrorMessage { get; set; }
    }
}
