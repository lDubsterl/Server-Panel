using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Panel.Domain.Interfaces.Repositories;
using Panel.Domain.Models;
using System.Text.Json;

namespace Panel.Infrastructure.Repositories
{
	public class Repository<T>(DbContext db, IDistributedCache redis) : IRepository<T> where T: AbstractEntity
	{
		public IQueryable<T> Entities => db.Set<T>();
		public async Task AddAsync(T entity)
		{
			var key = $"{typeof(T).Name}:{entity.Id}";
			var existing = await redis.GetStringAsync(key);
            if (existing != null)
				return;

            db.Set<T>().Add(entity);

			await redis.SetStringAsync(key, JsonSerializer.Serialize(entity), new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
			});
		}

		public async Task DeleteAsync(T entity)
		{
			var key = $"{typeof(T).Name}:{entity.Id}";
			db.Entry(entity).State = EntityState.Deleted;
			db.Set<T>().Remove(entity);
			
			await redis.RemoveAsync(key);
		}

		public async Task<T?> GetByIdAsync(int id)
		{
			var key = $"{typeof(T).Name}:{id}";
			var cached = await redis.GetStringAsync(key);
			if (cached != null)
				return JsonSerializer.Deserialize<T>(cached);

			var user =  await db.Set<T>().FindAsync(id);
			if (user != null)
				await redis.SetStringAsync(key, JsonSerializer.Serialize(user), new DistributedCacheEntryOptions { 
					AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
				});

			return user;
		}

		public async Task UpdateAsync(T entity)
		{
			var key = $"{typeof(T).Name}:{entity.Id}";
			db.Entry(entity).State = EntityState.Modified;
			db.Set<T>().Update(entity);
			await redis.SetStringAsync(key, JsonSerializer.Serialize(entity), new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
			});
		}
	}
}
