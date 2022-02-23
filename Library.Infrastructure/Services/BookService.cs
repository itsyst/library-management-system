using Library.Application.Interfaces;
using Library.Domain;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Library.Infrastructure.Services
{
    public class BookService : BaseService<BookDetails>, IBookService
    {
        public BookService(ApplicationDbContext context) : base(context)
        {
        }
        public async Task<BookDetails> GetBookOrDefaultAsync(Expression<Func<BookDetails, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            if (tracked)
            {
                IQueryable<BookDetails> query = _table;

                query = query.Where(filter);
                if (includeProperties != null)
                {
                    foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp);
                    }
                }

                return await query.FirstOrDefaultAsync();
            }
            else
            {
                IQueryable<BookDetails> query = _table.AsNoTracking();

                query = query.Where(filter);
                if (includeProperties != null)
                {
                    foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp);
                    }
                }

                return await query.FirstOrDefaultAsync();
            }
        }
    }
}
