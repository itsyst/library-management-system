using Library.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Application.Interfaces
{
    public interface IAuthorService
    {
        /// <summary>
        /// Gets all Authors
        /// </summary>
        /// <returns>List of authors</returns>
        IList<Author> GetAllAuthors();

        Author FindAuthor(int id);

        void AddAuthor(Author author);

        void UpdateAuthor(Author author);

        void DeleteAuthor(int id);
 
    }
}
