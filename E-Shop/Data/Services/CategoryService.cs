using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Models;
using System.Data;

namespace E_Shop.Data.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IDbConnection _connection;

        public CategoryService(DbConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.Connection;
        }

        public void Add(Category category)
        {
            _connection.Insert(category);
        }

        public void Delete(int id)
        {
            _connection.Delete(id);
        }

        public Category? Get(int id)
        {
            var filters = new Filters();
            filters.AddFilter("id", SqlOperator.Equal, id);

            return Get(filters);
        }

        public Category? Get(string name)
        {
            var filters = new Filters();
            filters.AddFilter("name", SqlOperator.Equal, name);

            return Get(filters);
        }

        private Category? Get (Filters filters)
        {
            return _connection.Select<Category>(filters).FirstOrDefault();
        }

        public Category[] GetAll()
        {
            return _connection.Select<Category>().ToArray();
        }

        public void Update(int id, Category category)
        {
            category.Id = id;
            _connection.Update(category);
        }
    }
}
