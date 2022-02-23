using Library.Domain;
using System.Linq.Expressions;

namespace Library.Application.Interfaces
{
    public interface IMemberService : IAsyncGenericRepository<Member>
    {
        Task<IReadOnlyList<Member>> GetAllMembersAsync(Expression<Func<Member, bool>>? filter = null, Func<IQueryable<Member>, IOrderedQueryable<Member>>? orderBy = null, params Expression<Func<Member, object>>[] includeProperties);
        Task<Member> GetMemberByIdAsync(int Id, bool includeProperties);
    }
}
