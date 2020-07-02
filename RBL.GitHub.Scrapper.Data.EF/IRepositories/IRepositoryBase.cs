using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RBL.GitHub.Scrapper.Data.EF.IRepositories
{
    public interface IRepositoryBase<T> where T : class
    {
        Task AddAsync(T obj);
        Task AddRangeAsync(IEnumerable<T> obj);
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);
        Task UpdateAsync(T obj);
        Task DeleteAsync(T obj);
        void Dispose();
    }
}
