using Library.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;
using Library.Domain;
using Microsoft.EntityFrameworkCore;

namespace Library.MVC.Controllers
{
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
            var authors = await _authorService.GetAllAuthorsAsync(includeProperties: a => a.Books);
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

        #region API CALLS
        // GET: Authors/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var authors = await _authorService.GetAllAuthorsAsync(includeProperties: a => a.Books);
            return Json(new { data = authors });
        }

        // DELETE: Authors/Delete/5
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var authorInDb = await _authorService.GetByIdAsync(id);

            var bookInDb = await _bookService.GetBookOrDefaultAsync(filter: b => b.Author.Id == authorInDb.Id);

            if (bookInDb != null)
                return Json(new { error = true, message = "You can not delete this author as long it has books refering to it (check books)!" });

            await _authorService.DeleteAsync(id);
            return Json(new { success = true, message = "Author deleted successfully." });

        }
        #endregion
    }
}