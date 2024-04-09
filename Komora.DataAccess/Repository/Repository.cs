using Komora.DataAccess.Data;
using Komora.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Komora.DataAccess.Repository
{
    /// <summary>
    /// This class implements the IRepository interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// stores the database context
        /// </summary>
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// stores the database set
        /// </summary>
        internal DbSet<T> dbSet;

        /// <summary>
        /// Constructor that initializes the database context
        /// </summary>
        /// <param name="db"></param>
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            dbSet = _db.Set<T>();
            //_db.Products.Include(u => u.Category).Include(u => u.CategoryId);
        }

        /// <summary>
        /// Method that adds an entity to the database
        /// </summary>
        /// <param name="entity">the entity to be added to the database</param>
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        /// <summary>
        /// This method that gets an entity from the database
        /// </summary>
        /// <param name="filter">filter</param>
        /// <param name="includeProperties">properties to include</param>
        /// <param name="tracked">determines if the entity should be tracked</param>
        /// <returns>
        /// returns the entity
        /// </returns>
        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked)
            {
                query = dbSet;

            }
            else
            {
                query = dbSet.AsNoTracking();
            }

            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();

        }

        /// <summary>
        /// Method that gets all entities from the database
        /// </summary>
        /// <param name="filter">filter</param>
        /// <param name="includeProperties">properties to include</param>
        /// <returns>
        /// returns all entities
        /// </returns>
        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }

            return query.ToList();
        }

        /// <summary>
        /// Method that removes an entity from the database
        /// </summary>
        /// <param name="entity">entity</param>
        public void Remove(T entity)
        {

            dbSet.Remove(entity);
        }

        /// <summary>
        /// Method that removes a range of entities from the database
        /// </summary>
        /// <param name="entity">entities</param>
        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
