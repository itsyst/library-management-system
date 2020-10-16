using Library.Application.Interfaces;
using Library.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.MVC.Models;
using System.Data.Common;

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

        public ActionResult Index()
        {
            var vm = new AuthorVm
            {
                Authors = _authorService.GetAllAuthors()
            };
            return View(vm);
        }

        public ActionResult Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var vm = new AuthorVm
            {
                Author = _authorService.FindAuthor(id)
            };

            return View(vm);
        }

        public ActionResult Create()
        {
            var vm = new AuthorVm();

            return View(vm);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AuthorVm vm)
        {

            try
            {
                var newAuthor = new Author
                {
                    Name = vm.Name
                };

                _authorService.AddAuthor(newAuthor);

                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                return View();
            }
        }

        public ActionResult Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var vm = new AuthorVm
            {
                Author = _authorService.FindAuthor(id)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AuthorVm vm)
        {
            try
            {
                var authorToBeEdited = new Author
                {
                    Id = vm.Id,
                    Name = vm.Name
                };
                _authorService.UpdateAuthor(authorToBeEdited);
                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                return View();
            }
        }

        public ActionResult Delete(int id, bool blockDelete)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var vm = new AuthorVm
            {
                Author = _authorService.FindAuthor(id),

                BlockDelete = blockDelete ? true : false
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(AuthorVm vm)
        {
  

            try
            {
                var booksWithAuthorToBeDeleted = _bookService.GetAllBooks().Where(b => b.AuthorID == vm.Id).ToList();
                if (booksWithAuthorToBeDeleted.Count() == 0)
                {
                    _authorService.DeleteAuthor(vm.Id);
                    return RedirectToAction(nameof(Index));
                }
                else
                    return RedirectToAction(nameof(Delete), new { blockDelete = true});
            }
            catch(DbException)
            {
                return View();
            }

          
        }
    }
}