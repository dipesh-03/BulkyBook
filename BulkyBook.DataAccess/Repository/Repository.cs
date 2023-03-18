using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;


namespace BulkyBook.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {

        #region Properties
        private readonly ApplicationDbContext _db;

        //to work with DataSets available inside the context
        internal DbSet<T> dbSet;
        #endregion

        #region Constructors
        public Repository(ApplicationDbContext db)
        {
            _db = db;

            //will set dbSet to the particular instance of Db among all
            this.dbSet = _db.Set<T>();
        }
        #endregion

        #region Methods

        public void Add(T item)
        {
            dbSet.Add(item);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null,  string? includeProperties = null)
        {
            // IQueryable is best to query data from out-memory (like remote database, service) collections
            // IEnumerable is best to query data from in-memory collections like List, Array, etc
            // While query data from a database, IQueryable execute the select query on the server side with all filters
            IQueryable<T> query = dbSet;

            if(filter != null) query = query.Where(filter);

            if(includeProperties != null)
            {
                foreach(var property in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)) {
                    query = query.Include(property);
                }
            }

            return query.ToList();
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
        {
            IQueryable<T> query;

            if (tracked)
            {
                query = dbSet;
            }
            else
            {
                // chnages in the objects will not be automatically saved in database
                query = dbSet.AsNoTracking();
            }

            query = dbSet.Where(filter);

            if (includeProperties != null)
            {
                foreach (var property in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }

            return query.FirstOrDefault();
        }


        public void Remove(T item)
        {
            dbSet.Remove(item);
        }

        
        public void RemoveRange(IEnumerable<T> items)
        {
            dbSet.RemoveRange(items);
        }

        #endregion
    }
}
