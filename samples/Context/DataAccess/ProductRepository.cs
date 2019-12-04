using System;
using System.Collections.Generic;
using System.Text;
using Domain.Models;
using MongoDB.Driver;

namespace DataAccess
{
    public class ProductRepository
    {
        private IMongoCollection<Product> _mongoCollection;

        public ProductRepository(ShopDbContext shopDbContext)
        {
            _mongoCollection = shopDbContext.CreateCollection<Product>();
        }
    }
}
