using Microsoft.EntityFrameworkCore;
using RBL.GitHub.Scrapper.Data.EF.Context;
using RBL.GitHub.Scrapper.Data.EF.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RBL.GitHub.Scrapper.Data.EF.Repositories
{
    public class RepositoryBase<T> : IDisposable, IRepositoryBase<T> where T : class
    {
        protected ScrapperContext Db = new ScrapperContext();

        public async Task AddAsync(T obj)
        {
            Db.Set<T>().Add(obj);

            await Task.Run(() => Db.SaveChangesAsync());
        }
        public async Task AddRangeAsync(IEnumerable<T> obj)
        {
            Db.Set<T>().AddRange(obj);

            await Task.Run(() => Db.SaveChangesAsync());
        }
        public async Task DeleteAsync(T obj)
        {
            Db.Set<T>().Remove(obj);

            await Task.Run(() => Db.SaveChangesAsync());
        }
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Db.Set<T>().ToListAsync();
        }
        public async Task<T> GetByIdAsync(int id)
        {
            return await Db.Set<T>().FindAsync(id);
        }
        public async Task UpdateAsync(T obj)
        {
            Db.Entry(obj).State = EntityState.Modified;
            await Task.Run(() => Db.SaveChangesAsync());
        }
        public async Task<IEnumerable<T>> Filter(Expression<Func<T, bool>> predicate)
        {
            return await this.Db.Set<T>().Where(predicate).ToListAsync();
        }
        public async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await Filter(predicate);
        }
        public void Dispose()
        {
            this.Db.Dispose();
        }
    }
}
