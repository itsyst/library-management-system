using Library.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Application.Interfaces
{
    public interface IBookCopyService
    {
        void UpdateBookCopyDetails(int id);

        void UpdateBookCopyDetails(int id, int loanId);

        ICollection<BookCopy> GetAllBookCopies();

        void AddBookCopy(BookCopy book);

        void RemoveBookCopies(List<BookCopy> bookCopiesToBeRemoved);

        BookCopy FindBookCopy(int id);

        void DeleteBookCopy(int id);

    }
}
