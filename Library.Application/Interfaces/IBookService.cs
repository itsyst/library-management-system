using Library.Domain.Entities;
using System.Linq.Expressions;

namespace Library.Application.Interfaces;

public interface IBookService : IAsyncGenericRepository<BookDetails>
{
    Task<IReadOnlyList<BookDetails>> GetAllBookDetailsAsync(Expression<Func<BookDetails, bool>>? filter = null, Func<IQueryable<BookDetails>, IOrderedQueryable<BookDetails>>? orderBy = null, params Expression<Func<BookDetails, object>>[] includeProperties);
    Task<BookDetails> GetBookOrDefaultAsync(Expression<Func<BookDetails, bool>> filter, string? includeProperties = null, bool tracked = true);
}
