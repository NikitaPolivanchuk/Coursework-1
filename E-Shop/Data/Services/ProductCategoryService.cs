using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class ProductCategoryService : IProductCategoryService
    {
        private readonly IDbConnection _connection;

        public ProductCategoryService()
        {
            _connection = DbConnectionProvider.GetInstance().Connection;
        }

        public void Add(ProductCategory productCategory)
        {
            _connection.Insert(productCategory);
        }

        public Category[] GetCategories(int productId)
        {
            var filters = new Filters();
            filters.AddFilter("product_id", SqlOperator.Equal, productId);

            var productCategoryList = _connection.Select<ProductCategory>(filters);
            var categories = new List<Category>();
            
            foreach (var productCategory in productCategoryList)
            {
                filters = new Filters();
                filters.AddFilter("id", SqlOperator.Equal, productCategory.CategoryId);

                var category = _connection.Select<Category>(filters).FirstOrDefault();
                if (category != null)
                {
                    categories.Add(category);
                }
            }
            return categories.ToArray();
        }

        public Product[] GetProducts(int categoryId)
        {
            var filters = new Filters();
            filters.AddFilter("category_id", SqlOperator.Equal, categoryId);

            var productCategoryList = _connection.Select<ProductCategory>(filters);
            var products = new List<Product>();

            foreach (var productCategory in productCategoryList)
            {
                filters = new Filters();
                filters.AddFilter("id", SqlOperator.Equal, productCategory.ProductId);

                var product = _connection.Select<Product>(filters).FirstOrDefault();
                if (product != null)
                {
                    products.Add(product);
                }
            }
            return products.ToArray();
        }
    }
}
