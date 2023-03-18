using BulkyBook.Models;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        /// <summary>
        /// Update object in the database
        /// </summary>
        /// <param name="obj"Object to be updated</param>
        void Update(Product obj);
    }
}
