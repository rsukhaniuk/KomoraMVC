using Komora.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Komora.DataAccess.Data
{
    /// <summary>
    /// Class that defines the application database context
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        /// <summary>
        /// Property that represents the Categories table
        /// </summary>
        public DbSet<Category> Categories { get; set; }

        /// <summary>
        /// Property that represents the Units table
        /// </summary>
        public DbSet<Unit> Units { get; set; }

        /// <summary>
        /// Property that represents the Proudcts table
        /// </summary>
        public DbSet<Product> Products { get; set; }

        /// <summary>
        /// Property that represents the Recipes table
        /// </summary>
        public DbSet<Recipe> Recipes { get; set; }

        /// <summary>
        /// Property that represents the ProductRecipe table
        /// </summary>
        public DbSet<ProductRecipe> ProductRecipe { get; set; }

        /// <summary>
        /// Property that represents the Meals table
        /// </summary>
        public DbSet<Meal> Meals { get; set; }

        /// <summary>
        /// Property that represents the Menu table
        /// </summary>
        public DbSet<Menu> Menu { get; set; }

        /// <summary>
        /// Property that represents the Inventory table
        /// </summary>
        public DbSet<InventoryItem> Inventory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Category 1"},
                new Category { Id = 2, Name = "Category 2"},
                new Category { Id = 3, Name = "Category 3"});
            
        }

    }
}
