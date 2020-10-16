using Library.Application.Interfaces;
using Library.Domain;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library.Infrastructure.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddBook(BookDetails book)
        {
            _context.Add(book);
            _context.SaveChanges();
        }

        public ICollection<BookDetails> GetAllBooks()
        {
            return _context.BookDetails.Include(x => x.Author).Include(b => b.Copies).OrderBy(x => x.Title).ToList();
        }

        /// <summary>
        /// Method to get books that are available for lending out 
        /// </summary>
        /// <returns>Collection of books available </returns>
        public ICollection<BookDetails> GetAvailableBooks()
        {
            var listOfBooks = _context.BookDetails.Include(x => x.Author).Include(b => b.Copies).OrderBy(x => x.Title).ToList();

            List<BookDetails> listOfBooksWithCopies = new List<BookDetails>();
            List<BookDetails> listOfBooksWithCopiesAvailable = new List<BookDetails>();

            // Books that currently have no copies must be filtered out
            foreach (var book in listOfBooks)
            {
                if (book.Copies.Count != 0)
                {
                    listOfBooksWithCopies.Add(book);
                }
            }
            // Then we make sure to only select books that have at least one copy available
            foreach (var book in listOfBooksWithCopies)
            {
                bool available = false;
                foreach (var item in book.Copies)
                {
                    if (item.IsAvailable == true)
                    {
                        available = true;
                        break;
                    }
                }
                if (available)
                {
                    listOfBooksWithCopiesAvailable.Add(book);
                }
            }
            return listOfBooksWithCopiesAvailable;
        }


        public BookDetails FindBook(int id)
        {
            var book = _context.BookDetails.SingleOrDefault(b => b.ID == id);
            return book;
        }

        public void UpdateBookDetails(BookDetails book)
        {
            _context.Update(book);
            _context.SaveChanges();
        }

        public void DeleteBookDetails(int id)
        {
            var book = FindBook(id);

            _context.BookDetails.Remove(book);
            _context.SaveChanges();
        }
    }
}
