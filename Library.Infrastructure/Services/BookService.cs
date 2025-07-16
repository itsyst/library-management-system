using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Library.Infrastructure.Services;

#nullable disable
public class BookService : BaseService<BookDetails>, IBookService
{
    public BookService(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<BookDetails>> GetAllBookDetailsAsync(Expression<Func<BookDetails, bool>>? filter = null, Func<IQueryable<BookDetails>, IOrderedQueryable<BookDetails>>? orderBy = null, params Expression<Func<BookDetails, object>>[] includeProperties)
    {
        IQueryable<BookDetails> query = _table;
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

            return query.FirstOrDefault();
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

            return query.FirstOrDefault();
        }
    }
}
