using Library.Domain.Entities;
using System.Linq.Expressions;

namespace Library.Application.Interfaces;

public interface IMemberService : IAsyncGenericRepository<Member>
{
    Task<IReadOnlyList<Member>> GetAllMembersAsync(
        Expression<Func<Member, bool>>? filter = null,
        Func<IQueryable<Member>, IOrderedQueryable<Member>>? orderBy = null,
        bool includeAllProperties = false,
        params Expression<Func<Member, object>>[] includeProperties);

    Task<Member?> GetMemberByIdAsync(int id, bool includeProperties = false);

    Task<Member?> GetMemberOrDefaultAsync(
        Expression<Func<Member, bool>> filter,
        params Expression<Func<Member, object>>[] includeProperties);

    // Additional useful methods
    Task<IReadOnlyList<Member>> GetActiveMembers();
    Task<IReadOnlyList<Member>> GetMembersWithOverdueLoans();
    Task<bool> CanMemberBorrowAsync(int memberId);
    Task<int> GetActiveLoanCountAsync(int memberId);

    Task AddRangeAsync(IEnumerable<Member> members);
    Task<HashSet<string>> GetExistingSSNsAsync();
    Task<HashSet<string>> GetExistingEmailsAsync();

}
