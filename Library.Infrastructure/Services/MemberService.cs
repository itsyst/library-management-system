using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Library.Infrastructure.Services;

public class MemberService : BaseService<Member>, IMemberService
{
    public ILoanService _loanService { get; }

    public MemberService(ApplicationDbContext context, ILoanService loanService) : base(context)
    {
        _loanService = loanService;
    }

    /// <summary>
    /// Gets all members with optional filtering, ordering, and includes
    /// </summary>
    public async Task<IReadOnlyList<Member>> GetAllMembersAsync(
        Expression<Func<Member, bool>>? filter = null,
        Func<IQueryable<Member>, IOrderedQueryable<Member>>? orderBy = null,
        params Expression<Func<Member, object>>[] includeProperties)
    {
        try
        {
            IQueryable<Member> query = _table.AsQueryable();

            // Apply filter if provided
            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Apply includes if provided
            if (includeProperties != null && includeProperties.Length > 0)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            // Apply ordering if provided
            if (orderBy != null)
            {
                query = orderBy(query);
            }
            else
            {
                // Default ordering by name
                query = query.OrderBy(m => m.Name);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            // Log the exception (if you have logging configured)
            throw new InvalidOperationException("Error retrieving members", ex);
        }
    }

    /// <summary>
    /// Gets a member by ID with optional includes
    /// </summary>
    public async Task<Member?> GetMemberByIdAsync(int id, bool includeProperties = false)
    {
        try
        {
            if (id <= 0)
                return null;

            if (includeProperties)
            {
                return await _table
                    .Include(m => m.Loans)
                        .ThenInclude(l => l.BookCopyLoans)
                            .ThenInclude(bcl => bcl.BookCopy)
                                .ThenInclude(bc => bc.Details)
                                    .ThenInclude(bd => bd.Author)
                    .FirstOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                return await GetByIdAsync(id);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving member with ID {id}", ex);
        }
    }

    /// <summary>
    /// Gets a member based on filter criteria with optional includes
    /// </summary>
    public async Task<Member?> GetMemberOrDefaultAsync(
        Expression<Func<Member, bool>> filter,
        params Expression<Func<Member, object>>[] includeProperties)
    {
        try
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            IQueryable<Member> query = _table.AsQueryable();

            // Apply includes if provided
            if (includeProperties != null && includeProperties.Length > 0)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                }
            }

            return await query.FirstOrDefaultAsync(filter);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving member with filter", ex);
        }
    }

    /// <summary>
    /// Gets all active members
    /// </summary>
    public async Task<IReadOnlyList<Member>> GetActiveMembers()
    {
        return await GetAllMembersAsync(
            filter: m => m.Status == MembershipStatus.Active,
            orderBy: query => query.OrderBy(m => m.Name));
    }

    /// <summary>
    /// Gets members with overdue loans
    /// </summary>
    public async Task<IReadOnlyList<Member>> GetMembersWithOverdueLoans()
    {
        return await GetAllMembersAsync(
            filter: m => m.Loans.Any(l => l.ReturnDate == null && l.DueDate < DateTime.UtcNow),
            orderBy: query => query.OrderBy(m => m.Name),
            includeProperties: m => m.Loans);
    }

    /// <summary>
    /// Checks if a member can borrow more books
    /// </summary>
    public async Task<bool> CanMemberBorrowAsync(int memberId)
    {
        try
        {
            var member = await GetMemberByIdAsync(memberId, includeProperties: true);

            if (member == null || member.Status != MembershipStatus.Active)
                return false;

            var activeLoanCount = await GetActiveLoanCountAsync(memberId);
            return activeLoanCount < member.MaxLoans;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking if member {memberId} can borrow", ex);
        }
    }

    /// <summary>
    /// Gets the count of active loans for a member
    /// </summary>
    public async Task<int> GetActiveLoanCountAsync(int memberId)
    {
        try
        {
           var activeLoans =  await _loanService.GetAllLoansAsync
                (
                filter: l => l.MemberId == memberId && l.ReturnDate == null,
                includeProperties: l => l.BookCopyLoans);

            return activeLoans.Count();
  
         }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting active loan count for member {memberId}", ex);
        }
    }
 
    /// <summary>
    /// Custom method to get members by status
    /// </summary>
    public async Task<IReadOnlyList<Member>> GetMembersByStatusAsync(MembershipStatus status)
    {
        return await GetAllMembersAsync(
            filter: m => m.Status == status,
            orderBy: query => query.OrderBy(m => m.Name));
    }

    /// <summary>
    /// Custom method to search members by name or email
    /// </summary>
    public async Task<IReadOnlyList<Member>> SearchMembersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var lowerSearchTerm = searchTerm.ToLower();

        return await GetAllMembersAsync(
            filter: m => m.Name.ToLower().Contains(lowerSearchTerm) ||
                        m.Email.ToLower().Contains(lowerSearchTerm) ||
                        m.SSN.Contains(searchTerm),
            orderBy: query => query.OrderBy(m => m.Name));
    }

    /// <summary>
    /// Custom method to get member statistics
    /// </summary>
    public async Task<MemberStatistics> GetMemberStatisticsAsync()
    {
        try
        {
            var totalMembers = await _table.CountAsync();
            var activeMembers = await _table.CountAsync(m => m.Status == MembershipStatus.Active);
            var suspendedMembers = await _table.CountAsync(m => m.Status == MembershipStatus.Suspended);

            var membersWithOverdue = await _table
                .Where(m => m.Loans.Any(l => l.ReturnDate == null && l.DueDate < DateTime.UtcNow))
                .CountAsync();

            //var totalOutstandingFees = await _context.Loans
            //    .Where(l => l.ReturnDate == null && l.Fee > 0)
            //    .SumAsync(l => l.Fee);
            var totalOutstandingFees = 0;
 
            var newMembersThisMonth = await _table
                .Where(m => m.MembershipStartDate.Month == DateTime.Now.Month &&
                           m.MembershipStartDate.Year == DateTime.Now.Year)
                .CountAsync();

            return new MemberStatistics
            {
                TotalMembers = totalMembers,
                ActiveMembers = activeMembers,
                SuspendedMembers = suspendedMembers,
                MembersWithOverdueBooks = membersWithOverdue,
                TotalOutstandingFees = totalOutstandingFees,
                NewMembersThisMonth = newMembersThisMonth
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error calculating member statistics", ex);
        }
    }
}
 
