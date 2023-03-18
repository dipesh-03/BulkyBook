using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{

    //we also need to inherit Repository<T> class because we have already defined some of the methods there (Add, FirstOrDefault etc)
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        //also pass db to repository class
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product obj)
        {
            //one way to update but it will update all the fields. Now if we want to update some specific fields then this method will be lengthy and not useful so we can update object via below method.
            //_db.Products.Update(obj);

            var ObjFromDb = _db.Products.FirstOrDefault(u => u.Id == obj.Id);

            if (ObjFromDb != null)
            {
                ObjFromDb.Title = obj.Title;
                ObjFromDb.ISBN = obj.ISBN;
                ObjFromDb.Price = obj.Price;
                ObjFromDb.Price50 = obj.Price50;
                ObjFromDb.Price100 = obj.Price100;
                ObjFromDb.ListPrice = obj.ListPrice;
                ObjFromDb.Description= obj.Description;
                ObjFromDb.CategoryId = obj.CategoryId;
                ObjFromDb.CoverTypeId = obj.CoverTypeId;
                ObjFromDb.Author = obj.Author;
                if(obj.ImageUrl!= null)
                {
                    ObjFromDb.ImageUrl = obj.ImageUrl;
                }
            }
        }
    }
}
