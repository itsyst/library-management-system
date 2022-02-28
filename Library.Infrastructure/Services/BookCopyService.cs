using Library.Domain;
using Library.Infrastructure.Persistence;
using Library.Application.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Services
{
    public class BookCopyService : BaseService<BookCopy>, IBookCopyService
    {
        public BookCopyService(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<BookCopy>> GetAllBookCopiesAsync(Expression<Func<BookCopy, bool>>? filter = null, Func<IQueryable<BookCopy>, IOrderedQueryable<BookCopy>>? orderBy = null, params Expression<Func<BookCopy, object>>[] includeProperties)
        {
            IQueryable<BookCopy> query = _table;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties)
                {
                    query = query.Include(includeProp);
                }

            }
            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }

        public async Task<BookCopy> GetBookCopyOrDefaultAsync(Expression<Func<BookCopy, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            if (tracked)
            {
                IQueryable<BookCopy> query = _table;

                query = query.Where(filter);
                if (includeProperties != null)
                {
                    foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp);
                    }
                }

                return await query.FirstAsync();
            }
            else
            {
                IQueryable<BookCopy> query = _table.AsNoTracking();

                query = query.Where(filter);
                if (includeProperties != null)
                {
                    foreach (var includeProp in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp);
                    }
                }

                return await query.FirstAsync();
            }
        }

        public void RemoveRange(IEnumerable<BookCopy> entities)
        {
            _table.RemoveRange(entities);
        }
    }
}

