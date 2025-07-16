using Library.Domain.Entities;
using System.Linq.Expressions;

namespace Library.Application.Interfaces;

public interface IBookCopyLoanService : IAsyncGenericRepository<BookCopyLoan>
{
    Task<BookCopyLoan> GetBookCopyLoanOrDefaultAsync(Expression<Func<BookCopyLoan, bool>> filter, string? includeProperties = null, bool tracked = true);
    Task<IReadOnlyList<BookCopyLoan>> GetAllBookCopyLoansAsync(Expression<Func<BookCopyLoan, bool>>? filter = null, Func<IQueryable<BookCopyLoan>, IOrderedQueryable<BookCopyLoan>>? orderBy = null, params Expression<Func<BookCopyLoan, object>>[] includeProperties);
    void RemoveRange(IEnumerable<BookCopyLoan> entities);
}
