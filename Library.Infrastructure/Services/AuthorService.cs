using Library.Application.Interfaces;
using Library.Domain;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Library.Infrastructure.Services
{
    public class AuthorService : IAuthorService
    {

        private readonly ApplicationDbContext _context;


        public AuthorService(ApplicationDbContext context)
        {
            _context = context;
        }


        public void AddAuthor(Author author)
        {
            _context.Authors.Add(author);
            _context.SaveChanges();
        }

        public Author FindAuthor(int id)
        {
            var author = _context.Authors.Include(a => a.Books).SingleOrDefault(a => a.Id == id);
            return author;
        }

        public void DeleteAuthor(int id)
        {
            var author = FindAuthor(id);

            _context.Authors.Remove(author);
            _context.SaveChanges();
        }

        public void UpdateAuthor(Author newAuthor)
        {
            _context.Update(newAuthor);
            _context.SaveChanges();
        }

        public IList<Author> GetAllAuthors()
        {
            return _context.Authors.Include(a => a.Books).ThenInclude(t => t.Copies).OrderBy(x => x.Id).ToList();
        }
    }
}
