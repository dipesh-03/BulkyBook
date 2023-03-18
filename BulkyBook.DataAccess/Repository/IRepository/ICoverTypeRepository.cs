﻿using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    public interface ICoverTypeRepository : IRepository<CoverType>
    {
        /// <summary>
        /// Update object in the database
        /// </summary>
        /// <param name="obj"Object to be updated</param>
        void Update(CoverType obj);
    }
}
