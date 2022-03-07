using Library.Application.Interfaces;
using Library.Domain;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Library.Infrastructure.Services
{
    public class MemberService : BaseService<Member>, IMemberService
    {
        public MemberService(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Member>> GetAllMembersAsync(Expression<Func<Member, bool>>? filter = null, Func<IQueryable<Member>, IOrderedQueryable<Member>>? orderBy = null, params Expression<Func<Member, object>>[] includeProperties)
        {
            IQueryable<Member> query = _table;
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

        public async Task<Member> GetMemberByIdAsync(int id, bool includeProperties)
        {
            Member Member = new();

            Member = includeProperties ? await _table.Include(m => m.Loans).ThenInclude(l => l.BookCopyLoans).ThenInclude(b => b.BookCopy).ThenInclude(b => b.Details).FirstOrDefaultAsync(x => x.Id == id) : await GetByIdAsync(id);

            return Member;
        }
    }
}
