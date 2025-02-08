using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Panel.Domain.DbConfigurations;
using Panel.Domain.Interfaces.Repositories;
using System.Data;
using System.Text;

namespace Panel.Infrastructure.Repositories
{
	public class Repository<T>(DbContext db) : IRepository<T> where T: class
	{
		public IQueryable<T> Entities => db.Set<T>();
		public Task AddAsync(T entity)
		{
			db.Set<T>().Add(entity);
			return Task.CompletedTask;
		}

		public Task DeleteAsync(T entity)
		{
			db.Entry(entity).State = EntityState.Deleted;
			db.Set<T>().Remove(entity);
			return Task.CompletedTask;
		}
		public async Task<T?> GetByIdAsync(int id)
		{
			return await db.Set<T>().FindAsync(id);
		}

		public Task UpdateAsync(T entity)
		{
			db.Entry(entity).State = EntityState.Modified;
			db.Set<T>().Update(entity);
			return Task.CompletedTask;
		}
	}
}
