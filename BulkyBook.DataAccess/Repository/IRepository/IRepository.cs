using System.Linq.Expressions;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    //Generic because we have to work with multiple classes in real time project
    //used to decouple application from framework and avoid repetation of the logic code
    //avoid using direct interaction with ApplicationDbContext
    public interface IRepository<T> where T : class
    {

        /// <summary>
        /// Returns the first object that matches the given condition else null
        /// </summary>
        /// <param name="filter">Lambda function to filter/query data</param>
        /// <param name="includeProperties">Properties that you want to include/bind (if you want Foreign key objects)</param>
        /// <returns></returns>
        T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true);

        /// <summary>
        /// Returns the list of the data stored in the database in the form of objects
        /// </summary>
        /// <param name="includeProperties">Properties that you want to include/bind (if you want Foreign key objects)</param>
        /// <returns></returns>
        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, string? includeProperties = null);

        /// <summary>
        /// Add Object to the table
        /// </summary>
        /// <param name="item"></param>
        void Add(T item);


        /// <summary>
        /// Removes object from the database
        /// </summary>
        /// <param name="item">Object to be removed</param>
        void Remove(T item);

        /// <summary>
        /// Remove list of items from the database
        /// </summary>
        /// <param name="items">List of objects to be removed</param>
        void RemoveRange(IEnumerable<T> items);

        //we don't define update methods here because it's logic can be different for each classes.
    }
}
