using Library.Application.Interfaces;
using Library.Domain;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Library.Infrastructure.Services
{
    public class BookCopyLoanService : BaseService<BookCopyLoan>, IBookCopyLoanService
    {
        public BookCopyLoanService(ApplicationDbContext context):base(context)
        {
        }

        public async Task<IReadOnlyList<BookCopyLoan>> GetAllBookCopyLoansAsync(Expression<Func<BookCopyLoan, bool>>? filter = null, Func<IQueryable<BookCopyLoan>, IOrderedQueryable<BookCopyLoan>>? orderBy = null, params Expression<Func<BookCopyLoan, object>>[] includeProperties)
        {
            IQueryable<BookCopyLoan> query = _table;
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

        public async Task<BookCopyLoan> GetBookCopyLoanOrDefaultAsync(Expression<Func<BookCopyLoan, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            if (tracked)
            {
                IQueryable<BookCopyLoan> query = _table;

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
                IQueryable<BookCopyLoan> query = _table.AsNoTracking();

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
}
