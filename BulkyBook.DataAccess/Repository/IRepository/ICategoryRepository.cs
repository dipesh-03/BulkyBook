using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Update object in the database
        /// </summary>
        /// <param name="obj"Object to be updated</param>
        //because this method is class specific so we can't define it in IRepository interface
        void Update(Category obj);

    }
}
