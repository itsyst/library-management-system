using Library.Domain;
using System.Linq.Expressions;

namespace Library.Application.Interfaces
{
    public interface IBookCopyService : IAsyncGenericRepository<BookCopy>
    {
        Task<IReadOnlyList<BookCopy>> GetAllBookCopiesAsync(Expression<Func<BookCopy, bool>>? filter = null, Func<IQueryable<BookCopy>, IOrderedQueryable<BookCopy>>? orderBy = null, params Expression<Func<BookCopy, object>>[] includeProperties);
        Task<BookCopy> GetBookCopyOrDefaultAsync(Expression<Func<BookCopy, bool>> filter, string? includeProperties = null, bool tracked = true);
        void RemoveRange(IEnumerable<BookCopy> entities);
    }
}
