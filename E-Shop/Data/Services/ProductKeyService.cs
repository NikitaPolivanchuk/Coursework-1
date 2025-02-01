using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Models;
using System.Data;

namespace E_Shop.Data.Services
{
    public class ProductKeyService : IProductKeyService
    {
        private readonly IDbConnection _connection;

        public ProductKeyService(DbConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.Connection;
        }

        public void Add(ProductKey productKey)
        {
            _connection.Insert(productKey);
        }

        public ProductKey? Get(int id)
        {
            var filters = new Filters();
            filters.AddFilter("id", SqlOperator.Equal, id);

            return _connection.Select<ProductKey>(filters).FirstOrDefault();
        }

        public void Delete(ProductKey productKey)
        {
            _connection.Delete(productKey);
        }

        public ProductKey[] GetAll(int productId)
        {
            var filters = new Filters();
            filters.AddFilter("product_id", SqlOperator.Equal, productId);

            return _connection.Select<ProductKey>(filters).ToArray();
        }
    }
}
