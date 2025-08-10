using Library.Application.Interfaces;
using Library.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Library.MVC.Controllers
{
#nullable disable  
    public class AuthorsController : Controller
    {
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;

        public AuthorsController(IAuthorService authorService, IBookService bookService)
        {
            _authorService = authorService;
            _bookService = bookService;
        }

        public async Task<IActionResult> Index()
        {
            var authors = await _authorService.GetAllAuthorsAsync(includeProperties: a => a.Books!);
            return View(authors);
        }

        public async Task<IActionResult> Create()
        {
            await Task.CompletedTask;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Author author)
        {
            try
            {
                await _authorService.AddAsync(author);
                TempData["Success"] = "Author created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInline([FromBody] Author model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                return Json(new { success = false, error = "Name required." });

            var author = new Author { Name = model.Name };
            await _authorService.AddAsync(author);
            return Json(new { success = true, authorId = author.Id, authorName = author.Name });
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
                return NotFound();

            var author = await _authorService.GetByIdAsync(id);

            if (author == null)
                return NotFound();

            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Author author)
        {
            if (id != author.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _authorService.UpdateAsync(author);
                    TempData["Success"] = "Author updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await AuthorExists(author.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        TempData["Error"] = "An Unexpected Error Occurred!";
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(author);
        }

        private async Task<bool> AuthorExists(int id)
        {
            return await _authorService.GetByIdAsync(id) != null;
        }

        public async Task<IActionResult> Details(int id)
        {
            var author = await _authorService.GetAuthorOrDefaultAsync(
                a => a.Id == id,
                includeProperties: "Books");

            if (author == null)
                return NotFound();

            return View(author);
        }

        //[HttpGet]
        //public async Task<IActionResult> Search(string searchTerm)
        //{
        //    var authors = await _authorService.SearchAuthorsAsync(searchTerm);
        //    return Json(authors.Select(a => new { id = a.Id, name = a.Name }));
        //}

        #region API CALLS
        // GET: Authors/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var authors = await _authorService.GetAllAuthorsAsync(includeProperties: a => a.Books);
            return Json(new { data = authors });
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var author = await _authorService.GetAuthorOrDefaultAsync(
                a => a.Id == id,
                includeProperties: "Books");

            if (author == null)
                return NotFound();

            var result = new
            {
                id = author.Id,
                name = author.Name,
                books = author.Books?.Select(b => new
                {
                    id = b.Id,
                    title = b.Title
                })
            };

            return Json(result);
        }

        // DELETE: Authors/Delete/5
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var authorInDb = await _authorService.GetByIdAsync(id);

            var bookInDb = await _bookService.GetBookOrDefault(filter: b => b.Author.Id == authorInDb.Id);

            if (bookInDb != null)
                return Json(new { error = true, message = "You can not delete this author as long it has books refering to it (check books)!" });

            await _authorService.DeleteAsync(id);
            return Json(new { success = true, message = "Author deleted successfully." });

        }
        #endregion
    }
}