using Library.Domain;
using Library.Infrastructure.Persistence;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Library.Application.Interfaces;

namespace Library.Infrastructure.Services
{
    public class BookCopyService : IBookCopyService
    {
        private readonly ApplicationDbContext _context;

        public BookCopyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICollection<BookCopy> GetAllBookCopies()
        {
            //return _context.BookCopies.Include(x => x.Details).Include(x => x.Loan).OrderBy(x => x.Id).ToList();
            return _context.BookCopies.ToList();
        }

        public BookCopy FindBookCopy(int id)
        {
            var bookCopy = _context.BookCopies.SingleOrDefault(a => a.BookCopyId == id);
            return bookCopy;
        }

        public void DeleteBookCopy(int id)
        {
            var bookCopy = FindBookCopy(id);

            _context.BookCopies.Remove(bookCopy);
            _context.SaveChanges();

        }
        /// <summary>
        /// This methods makes a bookcopy either available or unavailable for lending out
        /// </summary>
        /// <param name="id"></param>
        public void UpdateBookCopyDetails(int id)
        {
            var bookCopy = _context.BookCopies.Where(b => b.BookCopyId == id).First();

            var available = bookCopy.IsAvailable;

            if (available)
            {
                bookCopy.IsAvailable = false;
            }
            if (!available)
            {
                bookCopy.IsAvailable = true;
            }

            _context.SaveChanges();
        }

        public void UpdateBookCopyDetails(int id, int loanId)
        {
            var bookCopy = _context.BookCopies.Where(b => b.BookCopyId == id).First();

            bookCopy.IsAvailable = bookCopy.IsAvailable != true ? bookCopy.IsAvailable = false : bookCopy.IsAvailable = true;

            _context.SaveChanges();
        }
        public void AddBookCopy(BookCopy book)
        {
            _context.Add(book);
            _context.SaveChanges();
        }

        public void RemoveBookCopies(List<BookCopy> bookCopiesToBeRemoved)
        {
            foreach (var item in bookCopiesToBeRemoved)
            {
                _context.Remove(item);
                _context.SaveChanges();
            }
        }
    }
}

