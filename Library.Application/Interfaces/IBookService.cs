using Library.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Application.Interfaces
{
    public interface IBookService
    {
        /// <summary>
        /// Adds the book to DB
        /// </summary>
        /// <param name="book"></param>
        void AddBook(BookDetails book);

        /// <summary>
        /// Updates a book
        /// </summary>
        /// <param name="book"></param>
        void UpdateBookDetails(BookDetails book);

         /// <summary>
        /// Gets all books from the database
        /// </summary>
        /// <returns>list of books</returns>
        ICollection<BookDetails> GetAllBooks();
       
        /// <summary>
        /// Get only books that are not borrowed out
        /// </summary>
        /// <returns>A list of books</returns>
        ICollection<BookDetails> GetAvailableBooks();

        /// <summary>
        /// Get a book from the database
        /// </summary>
        /// <param name="id">Returns book</param>
        /// <returns></returns>
        BookDetails FindBook(int id);

        /// <summary>
        /// Deletes the book from the database
        /// </summary>
        /// <param name="id"></param>
        public void DeleteBookDetails(int id);

    }
}
