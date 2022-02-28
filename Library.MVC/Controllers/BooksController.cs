using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Library.Domain;
using Library.Application.Interfaces;
using System.Data.Common;
using Library.MVC.ViewModels;

namespace Library.MVC.Controllers
{
#pragma warning disable
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IBookCopyService _bookCopyService;
        private readonly ILoanService _loanService;

        public BooksController(
            IBookService bookservice, 
            IAuthorService authorService, 
            IBookCopyService bookCopyService,
            ILoanService loanService)
        {
            _bookService = bookservice;
            _authorService = authorService;
            _bookCopyService = bookCopyService;
            _loanService = loanService;
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
                //Create new book
                BookDetails bookDetails = new()
                {
                    AuthorID = model.BookDetails.AuthorID,
                    Description = model.BookDetails.Description,
                    ISBN = model.BookDetails.ISBN,
                    Title = model.BookDetails.Title,
                    Copies = new List<BookCopy>()
                };

                var books = await _bookService.GetAllAsync();

                // Chech if the book already exists
                foreach (var item in books)
                {
                    if (model.BookDetails.ISBN.ToLower().Trim() == item.ISBN.ToLower().Trim() || model.BookDetails.Title.ToLower().Trim() == item.Title.ToLower().Trim())
                    {
                        TempData["Error"] = "Book already exists.";
                        return View(model);
                    }
                }

                //Get the newqly new book
                var addedBook = await _bookService.AddAsync(bookDetails);

                for (var i = 0; i < model.Copies; i++)
                {
                    //Add book copies 
                    addedBook.Copies.Add(
                        new BookCopy
                        {
                            DetailsId = addedBook.ID,
                            IsAvailable = true
                        }
                    );
                }
                await _bookService.UpdateAsync(addedBook);

                TempData["Success"] = "Book created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "Something went wrong.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            IEnumerable<Author> authors = await _authorService.GetAllAsync();

            if (id == 0)
                return NotFound();

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

            // update
            model.BookDetails = await _bookService.GetBookOrDefaultAsync(b => b.ID == id, includeProperties: "Copies");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookDetailsViewModel model)
        {
            try
            {
                if (id == 0)
                    return NotFound();

                BookDetails book = new()
                {
                    ID = model.BookDetails!.ID,
                    AuthorID = model.BookDetails.AuthorID,
                    Description = model.BookDetails.Description,
                    ISBN = model.BookDetails.ISBN,
                    Title = model.BookDetails.Title
                };

                await _bookService.UpdateAsync(book);
                TempData["Success"] = "Book copy created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                TempData["Error"] = "an unexpected error has occurred.";
                return View();
            }
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
        // GET: Authors/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll(string status)
        {
            IEnumerable<BookDetails> books = await _bookService.GetAllAsync();

            // TODO if book loan or book no loan.
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

            return Json(new { data = books});
        }

        //POST: Books/AddBookCopy /5
        [HttpPost]
        public async Task<IActionResult> AddBookCopy(int id)
        {
            try
            {
                BookDetails book = await _bookService.GetBookOrDefaultAsync(c => c.ID == id, includeProperties: "Author");

                //Create new book
                BookCopy copy = new()
                {
                    DetailsId = book.ID,
                    Details = await _bookService.GetBookOrDefaultAsync(b => b.ID == id),
                    IsAvailable = true,
                };

                BookCopy addedCopy = await _bookCopyService.AddAsync(copy);

                return Json(new { success = true, message = "Book copy " + addedCopy.BookCopyId + " added successfully." });

            }
            catch (Exception)
            {
                return Json(new { error = true, message = "Something went wrong!" });
            }

            return Json(new { error = true, message = "An unexpected error occurred!" });
        }

        //DELETE: Books/Delete /5
        [HttpDelete]
        public async Task<IActionResult> DeleteSingle(int id)
        {
            if (id != 0)
            {
                var bookCopyInDb = await _bookCopyService.GetBookCopyOrDefaultAsync(filter: b => b.BookCopyId == id);

                await _bookCopyService.DeleteAsync(id);

                TempData["Success"] = "Book copy deleted successfully.";
                return RedirectToAction(nameof(Index));
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

                _bookCopyService.RemoveRange(bookCopiesInDb);

                await _bookService.UpdateAsync(bookInDB);

                return Json(new { success = true, message = "Book copies deleted successfully." });
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

                if (bookCopiesInDb != null)
                    _bookCopyService.RemoveRange(bookCopiesInDb);

                await _bookService.DeleteAsync(bookInDB.ID);

                return Json(new { success = true, message = "Book and copies deleted successfully." });
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

