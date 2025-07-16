using Library.Application.Interfaces;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Library.Infrastructure.Services;

#pragma warning disable
public partial class BaseService<T> : IAsyncGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    internal readonly DbSet<T> _table;

    public BaseService(ApplicationDbContext context)
    {
        _context = context;
        _table = context.Set<T>();
    }
    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _table.ToListAsync();
    }

    public async Task<T> GetByIdAsync(int id)
    {
        return await _table.FindAsync(id);
    }
 
    public async Task<T> AddAsync(T entity)
    {
        await _table.AddAsync(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Fäst entiteten i kontexten
        _table.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Hantera samtidighetskonflikter
            throw new InvalidOperationException("Uppdatering misslyckades på grund av samtidighetskonflikt.", ex);
        }

        return entity;
    }

    public async Task<T> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            throw new KeyNotFoundException($"Entitet av typen {typeof(T).Name} med Id={id} hittades inte.");

        _table.Remove(entity);
        await _context.SaveChangesAsync();

        return entity;
    }
}
