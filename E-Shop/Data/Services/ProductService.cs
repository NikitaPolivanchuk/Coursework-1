using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Models;
using System.Data;

namespace E_Shop.Data.Services
{
    public class ProductService : IProductService
    {
        private readonly IDbConnection _connection;

        public ProductService(DbConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.Connection;
        }

        public void Add(Product product)
        {
            _connection.Insert(product);
        }

        public Product? Get(int id)
        {
            var filters = new Filters();
            filters.AddFilter("id", SqlOperator.Equal, id);

            return Get(filters);
        }

        public Product? Get(string name)
        {
            var filters = new Filters();
            filters.AddFilter("name", SqlOperator.Equal, name);

            return Get(filters);
        }

        private Product? Get(Filters filters)
        {
            return _connection.Select<Product>(filters).FirstOrDefault();
        }

        public Product[] GetAll(Filters? filter = null)
        {
            return _connection.Select<Product>(filter).ToArray();
        }

        public void Delete(Product product)
        {
            _connection.Delete(product);
        }

        public void Update(int id, Product product, string[] categoryIds)
        {
            product.Id = id;
            _connection.Update(product);

            var query = @"
                DELETE FROM Product_Category
                WHERE product_id = @id
            ";
            var parameters = new Dictionary<string, object?>()
            {
                { "id", id }
            };
            _connection.Execute("DELETE FROM Product_Category WHERE product_id = @id", parameters);

            query = @"
                INSERT INTO Product_Category(product_id, category_id)
                VALUES(@productId, @categoryId)
            ";
            parameters = new Dictionary<string, object?>();

            foreach (string categoryId in categoryIds)
            {
                if (!string.IsNullOrEmpty(categoryId))
                {
                    parameters["productId"] = id;
                    parameters["categoryId"] = categoryId;

                    _connection.Execute(query, parameters);
                }
            }
            
        }

        public void Update(int id, Product product)
        {
            product.Id = id;
            _connection.Update(product);
        }
    }
}
