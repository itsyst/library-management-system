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
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IBookCopyService _bookCopyService;
        private readonly ILoanService _loanService;
        private readonly IBookCopyLoanService _bookCopyLoanService;

        public BooksController(
            IBookService bookservice,
            IAuthorService authorService,
            IBookCopyService bookCopyService,
            ILoanService loanService,
            IBookCopyLoanService bookCopyLoanService)
        {
            _bookService = bookservice;
            _authorService = authorService;
            _bookCopyService = bookCopyService;
            _loanService = loanService;
            _bookCopyLoanService = bookCopyLoanService;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAllBookDetailsAsync(filter: null, orderBy: null, a => a.Author, c => c.Copies);

            return View(books);
        }

        public async Task<IActionResult> Create()
        {
            IEnumerable<Author> authors = await _authorService.GetAllAuthorsAsync(filter: null, orderBy: null, includeProperties: a => a.Books);

            BookDetailsViewModel model = new()
            {
                BookDetails = new(),
                Authors = authors.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                Copies = 0
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookDetailsViewModel model)
        {
            try
            {
                // Handle cover image or default
                string? imageBinary = null;
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
                    // Use default image from wwwroot/uploads/9780555816023.png
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "9780555816023.png");
                    if (System.IO.File.Exists(path))
                    {
                        var bytes = await System.IO.File.ReadAllBytesAsync(path);
                        imageBinary = "data:image/png;base64," + Convert.ToBase64String(bytes);
                    }
                }

                // Generate random ISBN if not provided or invalid
                string isbn = model.BookDetails?.ISBN ?? string.Empty;

                if (string.IsNullOrWhiteSpace(isbn) ||
                    isbn == "0000000000000" ||
                    !IsbnGenerator.IsValidIsbn13(isbn))
                {
                    isbn = IsbnGenerator.GenerateRandomIsbn13();
                }

                // Check for duplicate ISBN (generate new one if duplicate)
                var books = await _bookService.GetAllAsync();
                int attempts = 0;
                const int maxAttempts = 10;

                while (attempts < maxAttempts)
                {
                    bool isDuplicate = false;
                    foreach (var item in books)
                    {
                        if (isbn.Equals(item.ISBN, StringComparison.OrdinalIgnoreCase))
                        {
                            isbn = IsbnGenerator.GenerateRandomIsbn13();
                            isDuplicate = true;
                            break;
                        }
                    }

                    if (!isDuplicate) break;
                    attempts++;
                }

                // Check for duplicate title
                foreach (var item in books)
                {
                    if (model.BookDetails.Title.Trim().Equals(item.Title.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["Error"] = "Book title already exists.";
                        return await ReloadCreateView(model);
                    }
                }

                // Create new book
                BookDetails bookDetails = new()
                {
                    AuthorID = model.BookDetails.AuthorID,
                    Description = model.BookDetails.Description,
                    ISBN = isbn,
                    Title = model.BookDetails.Title,
                    ImageBinary = imageBinary,
                    Copies = new List<BookCopy>()
                };

                var addedBook = await _bookService.AddAsync(bookDetails);

                // Add copies
                for (var i = 0; i < model.Copies; i++)
                {
                    addedBook.Copies.Add(new BookCopy
                    {
                        DetailsId = addedBook.ID,
                        IsAvailable = true
                    });
                }

                await _bookService.UpdateAsync(addedBook);
                TempData["Success"] = $"Book created successfully with ISBN: {isbn}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred: " + ex.Message;
                return await ReloadCreateView(model);
            }
        }

        private async Task<IActionResult> ReloadCreateView(BookDetailsViewModel model)
        {
            var authors = await _authorService.GetAllAuthorsAsync(filter: null, orderBy: null, includeProperties: a => a.Books);
            model.Authors = authors.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return View(model);
        }

        // Add this helper method for image processing
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
                return new ImageProcessResult
                {
                    Success = false,
                    ErrorMessage = "Error processing image: " + ex.Message
                };
            }
        }

        // Add this helper class
        public class ImageProcessResult
        {
            public bool Success { get; set; }
            public string? ImageBase64 { get; set; }
            public string? ErrorMessage { get; set; }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
                return NotFound();

            // Get book with Author and Copies included
            var bookDetails = await _bookService.GetBookOrDefaultAsync(
                b => b.ID == id,
                includeProperties: "Author,Copies");

            if (bookDetails == null)
                return NotFound();

            // Get authors for dropdown
            IEnumerable<Author> authors = await _authorService.GetAllAsync();
 
            BookDetailsViewModel model = new()
            {
                BookDetails = bookDetails,
                Authors = authors.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                Copies = bookDetails.Copies.Count

            };
 
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookDetailsViewModel model)
        {
            try
            {
                if (id == 0 || model.BookDetails == null)
                    return NotFound();

                // Get existing book (tracked entity)
                var existingBook = await _bookService.GetBookOrDefaultAsync(
                    b => b.ID == id,
                    includeProperties: "Author,Copies");

                if (existingBook == null)
                    return NotFound();

                // Handle cover image upload
                if(model.CoverImage != null && model.CoverImage.Length > 0)
                {
                    var imageResult = await ProcessCoverImageAsync(model.CoverImage);
                    if (imageResult != null) {
                        existingBook.ImageBinary = imageResult.ImageBase64;
                    }
                    else {
                        TempData["Error"] = imageResult.ErrorMessage;
                        return await ReloadEditView(model);
                    }
                    // If no new image uploaded, keep existing image
                }
                // Validate title uniqueness (excluding current book)
                var books = await _bookService.GetAllAsync();
                foreach (var item in books.Where(b => b.ID != id))
                {
                    if (model.BookDetails.Title.Trim().Equals(item.Title.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["Error"] = "A book with this title already exists.";
                        return await ReloadEditView(model);
                    }
                }

                // Update properties on EXISTING tracked entity
                existingBook.AuthorID = model.BookDetails.AuthorID;
                existingBook.Description = model.BookDetails.Description;
                existingBook.Title = model.BookDetails.Title;
                // ISBN is not updated as it's readonly in edit

                // Save changes using tracked entity (no need to call UpdateAsync)
                await _bookService.UpdateAsync(existingBook);

                TempData["Success"] = "Book updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred: " + ex.Message;
                return await ReloadEditView(model);
            }
        }

        private async Task<IActionResult> ReloadEditView(BookDetailsViewModel model)
        {
            var authors = await _authorService.GetAllAsync();
            model.Authors = authors.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            return View(model);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == 0)
                return NotFound();

            IReadOnlyList<BookCopy> bookCopies = await _bookCopyService.GetAllBookCopiesAsync(x => x.DetailsId == id);

            BookDetailsViewModel model = new()
            {
                BookDetails = await _bookService.GetBookOrDefaultAsync(x => x.ID == id, "Author"),
                BookCopies = bookCopies.Select(i => new SelectListItem
                {
                    Text = i.BookCopyId.ToString(),
                    Value = i.BookCopyId.ToString()
                }),
                Copies = bookCopies.Count
            };

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll(BookDetailsViewModel model)
        {
            return View(model);
        }

        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookService.GetBookOrDefaultAsync(
                b => b.ID == id,
                includeProperties: "Author,Copies");

            if (book == null)
                return NotFound();

            var result = new
            {
                id = book.ID,
                title = book.Title,
                author = book.Author?.Name ?? "Unknown",
                isbn = book.ISBN,
                description = book.Description ?? "No description",
                totalCopies = book.Copies.Count,
                availableCopies = book.Copies.Count(c => c.IsAvailable),
                imageBinary = book.ImageBinary ?? "/uploads/9780555816023.png"
            };

            return Json(result);
        }

        // GET: Authors/GetAll
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetAll(string status)
        {
            IEnumerable<BookDetails> books = await _bookService.GetAllAsync();

            switch (status)
            {
                case "isAvailable":
                    books = await _bookService.GetAllBookDetailsAsync(filter: b => b.Copies.Count != 0, orderBy: x => x.OrderBy(b => b.Copies.Count), a => a.Author, c => c.Copies);
                    break;
                case "all":
                    books = await _bookService.GetAllBookDetailsAsync(filter: null, orderBy: null, a => a.Author, c => c.Copies);
                    break;
                default:
                    break;
            }

            return Json(new { data = books });
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
                return Json(new { success = false, error = ex.Message });
            }
        }

        // POST: Books/AddBookCopy (id)
        [HttpPost]
        public async Task<IActionResult> AddBookCopy(int id)
        {
            try
            {
                // 1. Pull the book with its copies (tracked entity)
                var book = await _bookService.GetBookOrDefaultAsync(
                               b => b.ID == id,
                               includeProperties: "Author,Copies");

                if (book == null)
                    return Json(new { success = false, message = "Book not found." });

                if (book.Copies.Count >= 5)
                    return Json(new
                    {
                        success = false,
                        message = $"Cannot add more copies. Maximum limit of 5 copies reached for \"{book.Title}\"."
                    });


                // 2. Create & save the new copy (always available by default),
                var initialBookCopies = book.Copies.Count;
                var newCopy = await _bookCopyService.AddAsync(new BookCopy
                {
                    DetailsId = book.ID,
                    Details = book,
                    IsAvailable = true
                });

                // 3. Re-query only the copies list to get fresh counts
                var total = initialBookCopies + 1;                 // we know one was added
                var avail = book.Copies.Count(c => c.IsAvailable);
                var onLoan = total - avail;
                var remaining = 5 - total;

                // 4. Compose the response (ISBN is **not** modified)
                var msg = $"Book copy {newCopy.BookCopyId} added successfully to \"{book.Title}\". ";

                return Json(new
                {
                    success = true,
                    message = msg,
                    totalCopies = total,
                    availableCopies = avail,
                    onLoanCopies = onLoan,
                    maxLimitReached = total >= 5,
                    bookId = book.ID,
                    bookTitle = book.Title,
                 });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error adding copy: {ex.Message}"
                });
            }
        }


        //DELETE: Books/Delete /5
        [HttpDelete]
        public async Task<IActionResult> DeleteSingle(int id)
        {
            if (id != 0)
            {
                var bookCopyInDb = await _bookCopyService.GetBookCopyOrDefaultAsync(filter: b => b.BookCopyId == id);

                var boocopyLoan = await _bookCopyLoanService.GetBookCopyLoanOrDefaultAsync(x => x.BookCopyId == bookCopyInDb.BookCopyId);

                // Check if book copy is loaned
                if (boocopyLoan != null)
                    return Json(new { error = true, message = "This book copy could not be deleted. It first has to be returned(check loans)!" });

                // Delete book copy loan
                await _bookCopyService.DeleteAsync(id);

                return Json(new { success = true, message = "Book copy " + id + " deleted successfully." });
            }
            return Json(new { error = true, message = "An unexpected error occurred." });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll(int id)
        {
            if (id != 0)
            {

                var bookInDB = await _bookService.GetByIdAsync(id);
                var bookCopiesInDb = await _bookCopyService.GetAllBookCopiesAsync(filter: b => b.DetailsId == bookInDB.ID);
                List<BookCopy> bookCopiesTobeDeleted = new List<BookCopy>();

                foreach (var bookCopy in bookCopiesInDb)
                {
                    var boocopyLoan = await _bookCopyLoanService.GetBookCopyLoanOrDefaultAsync(x => x.BookCopyId == bookCopy.BookCopyId);

                    // Check if book copy is loaned
                    if (boocopyLoan == null)

                        bookCopiesTobeDeleted.Add(bookCopy);
                }

                if (bookCopiesTobeDeleted.Count > 0)
                {
                    _bookCopyService.RemoveRange(bookCopiesTobeDeleted);
                    await _bookService.UpdateAsync(bookInDB);

                    return Json(new { success = true, message = "Book copies deleted successfully." });
                }
                else
                {
                    return Json(new { error = true, message = "Book copies could not be deleted. They first has to be returned(check loans)!" });
                }
            }

            return Json(new { error = true, message = "An unexpected error occurred." });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (id != 0)
            {
                var bookInDB = await _bookService.GetBookOrDefaultAsync(b => b.ID == id);
                var bookCopiesInDb = await _bookCopyService.GetAllBookCopiesAsync(filter: c => c.DetailsId == bookInDB.ID);
                List<BookCopy> bookCopiesTobeDeleted = new List<BookCopy>();

                foreach (var bookCopy in bookCopiesInDb)
                {
                    var boocopyLoan = await _bookCopyLoanService.GetBookCopyLoanOrDefaultAsync(x => x.BookCopyId == bookCopy.BookCopyId);

                    // Check if book copy is loaned
                    if (boocopyLoan == null)
                        bookCopiesTobeDeleted.Add(bookCopy);
                }

                if (bookCopiesTobeDeleted.Count >= 0)
                {
                    // delete book copies
                    _bookCopyService.RemoveRange(bookCopiesTobeDeleted);
                    await _bookService.UpdateAsync(bookInDB);

                    // delete book
                    await _bookService.DeleteAsync(bookInDB.ID);

                    return Json(new { success = true, message = "Book and related copies deleted successfully." });
                }
                else
                {
                    return Json(new { error = true, message = "You can not delete this book as long it has loans refering to it (check loans)!" });
                }

            }
            else
            {
                return Json(new { error = true, message = "Something went wrong." });
            }
            return Json(new { error = true, message = "An unexpected error occurred." });
        }
        #endregion

    }
}

