using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Library.Infrastructure.Services
{
#nullable disable
    public class LoanService(ApplicationDbContext context) : BaseService<Loan>(context), ILoanService
    {
        public async Task<Loan> GetLoanOrDefaultAsync(
            Expression<Func<Loan, bool>> filter, 
            string includeProperties = null, 
            bool tracked = true)
        {
            IQueryable<Loan> query = tracked ? _table : _table.AsNoTracking();

            query = query.Where(filter);

            // Null check for includeProperties
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }
 
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<Loan>> GetAllLoansAsync(
            Expression<Func<Loan, bool>> filter = null,
            Func<IQueryable<Loan>, IOrderedQueryable<Loan>> orderBy = null,
            params Expression<Func<Loan, object>>[] includeProperties)
        {
            IQueryable<Loan> query = _table;

            if (filter != null)
                query = query.Where(filter);

            if (includeProperties?.Length > 0)
                foreach (var includeProp in includeProperties)
                    query = query.Include(includeProp);

            if (orderBy != null)
                query = orderBy(query);

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<Loan>> GetAllLoansAsync(
            Expression<Func<Loan, bool>> filter = null,
            Func<IQueryable<Loan>, IOrderedQueryable<Loan>> orderBy = null,
            string includeProperties = null)
        {
            IQueryable<Loan> query = _table;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProp in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.ToListAsync();
        }


        public async Task<int> CountActiveLoansAsync()
        {
            return await _table.CountAsync(l => l.Status == LoanStatus.Active);
        }

        public async Task<int> CountOverdueLoansAsync()
        {
            return await _table.CountAsync(l => l.Status == LoanStatus.Overdue);
        }

        public async Task<int> CountLoansByStatusAsync(LoanStatus status)
        {
            return await _table.CountAsync(l => l.Status == status);
        }

        public async Task<decimal> GetTotalFeesAsync()
        {
            return await _table.SumAsync(l => l.Fee);
        }

        public async Task<decimal> GetTotalOutstandingFeesAsync()
        {
            return await _table
                .Where(l => l.Fee > 0 && l.Status != LoanStatus.Returned)
                .SumAsync(l => l.Fee);
        }

        // Utility method for pagination support
        public async Task<IReadOnlyList<Loan>> GetPagedLoansAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<Loan, bool>> filter = null,
            Func<IQueryable<Loan>, IOrderedQueryable<Loan>> orderBy = null)
        {
            IQueryable<Loan> query = _table;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
