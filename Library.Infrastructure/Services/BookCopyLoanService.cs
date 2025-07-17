using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Library.Infrastructure.Services;

public class BookCopyLoanService(ApplicationDbContext context) : BaseService<BookCopyLoan>(context), IBookCopyLoanService
{
    #region Core Query Methods
    public async Task<IReadOnlyList<BookCopyLoan>> GetAllBookCopyLoansAsync(Expression<Func<BookCopyLoan, bool>>? filter = null, Func<IQueryable<BookCopyLoan>, IOrderedQueryable<BookCopyLoan>>? orderBy = null, params Expression<Func<BookCopyLoan, object>>[] includeProperties)
    {
        IQueryable<BookCopyLoan> query = _table;
        
        if (filter != null) query = query.Where(filter);
        
        if (includeProperties?.Length > 0)
            foreach (var includeProp in includeProperties)
                query = query.Include(includeProp);

        if (orderBy != null) query = orderBy(query);

        return await query.ToListAsync();
    }

    public async Task<BookCopyLoan> GetBookCopyLoanOrDefaultAsync(Expression<Func<BookCopyLoan, bool>> filter, string? includeProperties = null, bool tracked = true)
    {
        if (filter == null) throw new ArgumentNullException(nameof(filter));

        IQueryable<BookCopyLoan> query = tracked ? _table : _table.AsNoTracking();

        // Apply filter
        query = query.Where(filter);

        // Include related properties
        if (includeProperties?.Length > 0)
        {
            foreach (var includeProp in includeProperties.Split([','] , StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProp);
            }
        }

        return await query.FirstOrDefaultAsync();
    }

    public void RemoveRange(IEnumerable<BookCopyLoan> entities)
    {
        _table.RemoveRange(entities);
    }

    #endregion
}
