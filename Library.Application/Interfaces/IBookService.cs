using Library.Domain;
using System.Linq.Expressions;

namespace Library.Application.Interfaces
{
    public interface IBookService : IAsyncGenericRepository<BookDetails>
    {
        Task<BookDetails> GetBookOrDefaultAsync(Expression<Func<BookDetails, bool>> filter, string? includeProperties = null, bool tracked = true);
    }
}
