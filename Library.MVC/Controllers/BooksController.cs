using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Library.Domain;
using Library.MVC.Models;
using Library.Application.Interfaces;
using System.Data.Common;

namespace Library.MVC.Controllers
{
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IBookCopyService _bookCopyService;

        public BooksController(IBookService bookservice, IAuthorService authorService, IBookCopyService bookCopyService)
        {
            _bookService = bookservice;
            _authorService = authorService;
            _bookCopyService = bookCopyService;
        }

        public ActionResult Index(string searchString, bool availability, bool selectedAuthor, bool bookCopyAdded)
        {
            var vm = new BookIndexVm();
            IQueryable<BookDetails> books;

            if (availability || selectedAuthor)
            {
                books = _bookService.GetAvailableBooks().AsQueryable();
                vm.ShowOnlyAvailableCopies = true;
            }
            else
            {
                books = _bookService.GetAllBooks().AsQueryable();
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                vm.Books = books.Where(b => b.Author.Name.Contains(searchString)).ToList();
                vm.SearchString = searchString;
                vm.SelectedOnAuthors = true;
            }
            else
                vm.Books = books.ToList();

            // code needed to notify the user of the id of the new bookcopy  
            vm.BookCopyAdded = bookCopyAdded ? true : false;
            var listOfBookCopies = _bookCopyService.GetAllBookCopies();
            var lastItem = listOfBookCopies.Last();
            ViewBag.msg = $"Bookcopy succesfully created. Its ID is: {lastItem.BookCopyId}";

            return View(vm);
        }

        public ActionResult Create()
        {
            var vm = new BookCreateVm
            {
                AuthorList = new SelectList(_authorService.GetAllAuthors(), "Id", "Name")
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BookCreateVm vm)
        {
            if (ModelState.IsValid)
            {
                //Create new book
                var newBook = new BookDetails();
                newBook.AuthorID = vm.AuthorId;
                newBook.Description = vm.Description;
                newBook.ISBN = vm.ISBN;
                newBook.Title = vm.Title;
                _bookService.AddBook(newBook);

                if (vm.NumberOfCopies != 0)
                {
                    var allBooksIncludingNewBookDetails = _bookService.GetAllBooks();
                    var allBooksOrdered = allBooksIncludingNewBookDetails.OrderBy(x => x.ID);
                    var lastItem = allBooksOrdered.Last();

                    for (var i = 0; i < vm.NumberOfCopies; i++)
                    {
                        var newBookCopy = new BookCopy();

                        var BookDetailsID = lastItem.ID;
                        newBookCopy.DetailsId = BookDetailsID;

                        _bookCopyService.AddBookCopy(newBookCopy);
                        newBookCopy = null;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Error", "Home", "");
        }

        /// <summary>
        /// Action to add a new copy to the databas and then redirect to index
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddBookCopy(int id, string authorName, bool showOnlyAvailable, bool showSelectedAuthor)
        {
            if (id == 0)
            {
                return NotFound();
            }

            if (id >= 0)
            {
                //Create new bookcopy
                var newBookCopy = new BookCopy();

                newBookCopy.DetailsId = id;

                _bookCopyService.AddBookCopy(newBookCopy);

                return RedirectToAction(nameof(Index), new { searchString = authorName, availability = showOnlyAvailable, selectedAuthor = showSelectedAuthor, bookCopyAdded = true });
            }

            return RedirectToAction("Error", "Home", "");
        }

        public ActionResult Edit(int id)
        {

            if (id == 0)
            {
                return NotFound();
            }

            var vm = new BookEditVm
            {
                AuthorList = new SelectList(_authorService.GetAllAuthors(), "Id", "Name"),
                Book = _bookService.FindBook(id)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, BookEditVm vm)
        {
            try
            {
                if (id == 0)
                {
                    return NotFound();
                }


                BookDetails updatedBook = new BookDetails
                {
                    ID = vm.Id,
                    AuthorID = vm.AuthorId,
                    Description = vm.Description,
                    ISBN = vm.ISBN,
                    Title = vm.Title
                };

                _bookService.UpdateBookDetails(updatedBook);
                return RedirectToAction(nameof(Index));
            }
            catch (DbException)
            {
                return View();
            }
        }

        public ActionResult Delete(int id, bool bookCopiesBlocked, bool bookCopyBlocked)
        {
            if (id == 0)
            {
                return NotFound();
            }


            var vm = new BookDeleteVm();
            vm.Book = _bookService.FindBook(id);
            vm.BookCopyList = new SelectList(_bookCopyService.GetAllBookCopies().Where(b => b.DetailsId == id).ToList(), "BookCopyId", "BookCopyId");

            vm.BookCopiesBlocked = bookCopiesBlocked ? true : false;
            vm.BookCopyBlocked = bookCopyBlocked ? true : false;

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(BookDeleteVm vm)
        {

            try
            {
                // Here we first check which of the delete options the user has clicked on
                // We also prevent the user from deleting a bookcopy that is part of a loan event

                if (vm.BookCopyId != 0 && vm.DeleteAll == false)
                {
                    if (_bookCopyService.FindBookCopy(vm.BookCopyId).IsAvailable == false)
                    {
                        return RedirectToAction(nameof(Delete), new { id = vm.BookDetailsID, bookCopyBlocked = true });
                    }
                    else
                    {
                        _bookCopyService.DeleteBookCopy(vm.BookCopyId);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else if (vm.BookCopyId == 0 && vm.DeleteAll)
                {
                    var bookCopiesToBeRemoved = _bookCopyService.GetAllBookCopies().Where(b => b.DetailsId == vm.BookDetailsID).ToList();

                    var bookCopiesToBlock = bookCopiesToBeRemoved.Where(b => b.IsAvailable == false).ToList();

                    bookCopiesToBeRemoved.RemoveAll(b => b.IsAvailable == false);

                    _bookCopyService.RemoveBookCopies(bookCopiesToBeRemoved);

                    if (bookCopiesToBlock.Count() != 0)
                    {
                        return RedirectToAction(nameof(Delete), new { id = vm.BookDetailsID, bookCopiesBlocked = true });
                    }
                    else
                    {
                        _bookService.DeleteBookDetails(vm.BookDetailsID);
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                    return RedirectToAction(nameof(Delete), vm.Book.ID);
            }
            catch (DbException)
            {
                return View();
            }
        }

        public ActionResult Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var vm = new BookDetailsVm
            {
                Book = _bookService.FindBook(id),
                BookCopies = _bookCopyService.GetAllBookCopies().Where(b => b.DetailsId == id).ToList()
            };

            return View(vm);
        }

    }
}

