using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class ProductService : IProductService
    {
        private readonly DbAccess _db;

        public ProductService()
        {
            _db = DbAccess.GetInstance();
        }

        public void Add(Product product)
        {
            _db.Insert("Products", ["name", "price", "number", "image_url", "description"],
                [product.Name, product.Price.ToString(), product.Number.ToString(), product.ImageUrl, product.Description]);
        }

        public Product? Get(int id)
        {
            return _Get($"id={id}");
        }

        public Product? Get(string name)
        {
            return _Get($"name='{name}'");
        }

        private Product? _Get(string condition)
        {
            DataTable data = _db.Select("Products", ["*"], condition);

            if (data.Rows.Count < 1)
            {
                return null;
            }
            return new Product(data.Rows[0]);
        }

        public Product[] GetAll(string? filter = null)
        {
            string? column = (filter != null) ? "name" : null;

            DataTable data = _db.Select("Products", ["*"], column, filter);

            List<Product> products = new List<Product>();

            foreach (DataRow row in data.Rows)
            {
                products.Add(new Product(row));
            }
            return products.ToArray();
        }

        public void Delete(int id)
        {
            _db.Delete("Products", $"id={id}");
        }

        public void Update(int id, Product product, string[] categoryIds)
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"name", product.Name },
                {"price", product.Price.ToString()},
                {"image_url", product.ImageUrl },
                {"description", product.Description}
            };

            _db.Update("Products", data, $"id={id}");

            _db.Delete("Product_Category", $"product_id={id}");

            foreach (string categoryId in categoryIds)
            {
                if (!string.IsNullOrEmpty(categoryId))
                {
                    _db.Insert("Product_Category", ["product_id", "category_id"],
                        [id.ToString(), categoryId]);
                }
            }
            
        }

        public void Update(int id, Product product)
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                {"name", product.Name },
                {"price", product.Price.ToString()},
                {"number", product.Number.ToString()},
                {"image_url", product.ImageUrl },
                {"description", product.Description}
            };

            _db.Update("Products", data, $"id={id}");
        }
    }
}
