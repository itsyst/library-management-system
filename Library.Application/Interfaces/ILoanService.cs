using Library.Domain.Entities;
using Library.Domain.Enums;
using System.Linq.Expressions;

namespace Library.Application.Interfaces;

public interface ILoanService : IAsyncGenericRepository<Loan>
{
    Task<IReadOnlyList<Loan>> GetAllLoansAsync(Expression<Func<Loan, bool>>? filter = null, Func<IQueryable<Loan>, IOrderedQueryable<Loan>>? orderBy = null, params Expression<Func<Loan, object>>[] includeProperties);
    Task<IReadOnlyList<Loan>> GetAllLoansAsync(Expression<Func<Loan, bool>> filter, Func<IQueryable<Loan>, IOrderedQueryable<Loan>> orderBy, string includeProperties);
    Task<Loan> GetLoanOrDefaultAsync(Expression<Func<Loan, bool>> filter, string? includeProperties = null, bool tracked = true);
    Task<decimal> GetTotalFeesAsync();
    Task<decimal> GetTotalOutstandingFeesAsync();
    Task<int> CountActiveLoansAsync();
    Task<int> CountLoansByStatusAsync(LoanStatus status);
    Task<int> CountOverdueLoansAsync();
}
