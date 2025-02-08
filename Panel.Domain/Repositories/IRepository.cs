﻿namespace Panel.Domain.Interfaces.Repositories
{
	public interface IRepository<T> where T : class
	{
		public IQueryable<T> Entities { get; }
		public Task<T?> GetByIdAsync(int id);
		public Task AddAsync(T entity);
		public Task UpdateAsync(T entity);
		public Task DeleteAsync(T entity);
	}
}
