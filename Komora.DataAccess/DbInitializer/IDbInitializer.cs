using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komora.DataAccess.DbInitializer
{
    /// <summary>
    /// Interface that defines the database initializer
    /// </summary>
    public interface IDbInitializer
    {
        /// <summary>
        /// Method that initializes the database
        /// </summary>
        void Initialize();
    }
}
