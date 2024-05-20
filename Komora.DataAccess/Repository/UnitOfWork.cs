using Komora.DataAccess.Data;
using Komora.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.DataAccess.Repository
{
    /// <summary>
    /// Class that implements the IUnitOfWork interface
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {

        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Constructor that initializes the database context and the CategoryRepository
        /// </summary>
        /// <param name="db"></param>
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            Unit = new UnitRepository(_db);
            Product = new ProductRepository(_db);
            Recipe = new RecipeRepository(_db);
        }
        
        /// <summary>
        /// Property that returns the CategoryRepository
        /// </summary>
        public ICategoryRepository Category { get; private set; }

        /// <summary>
        /// Property that returns the UnitRepository
        /// </summary>
        public IUnitRepository Unit { get; private set; }

        /// <summary>
        /// Property that returns the IProductRepository
        /// </summary>
        public IProductRepository Product { get; private set; }

        /// <summary>
        /// Property that returns the IRecipeRepository
        /// </summary>
        public IRecipeRepository Recipe { get; private set; }

        /// <summary>
        /// Method that disposes the database context
        /// </summary>
        public void Dispose()
        {
            _db.Dispose();
        }

        /// <summary>
        /// Method that saves the changes made to the database
        /// </summary>
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
