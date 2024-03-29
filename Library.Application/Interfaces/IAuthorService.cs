﻿using Library.Domain;
using System.Linq.Expressions;

namespace Library.Application.Interfaces
{
    public interface IAuthorService : IAsyncGenericRepository<Author>
    {
        Task<IReadOnlyList<Author>> GetAllAuthorsAsync(Expression<Func<Author, bool>>? filter = null, Func<IQueryable<Author>, IOrderedQueryable<Author>>? orderBy = null, params Expression<Func<Author, object>>[] includeProperties);
    }

}
