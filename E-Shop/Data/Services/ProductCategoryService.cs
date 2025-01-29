using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class ProductCategoryService : IProductCategoryService
    {
        private readonly DbAccess _db;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public ProductCategoryService(ICategoryService categoryService, IProductService productService)
        {
            _db = DbAccess.GetInstance();
            _categoryService = categoryService;
            _productService = productService;
        }

        public void Add(int productId, int categoryId)
        {
            _db.Insert("Product_Category", ["product_id", "category_id"],
                [productId.ToString(), categoryId.ToString()]);
        }

        public Category[] GetCategories(int productId)
        {
            List<Category> categories = new List<Category>();

            DataTable categoryIds = _db.Select("Product_Category", ["category_id"], $"product_id={productId}");
            
            foreach (DataRow CategoryId in categoryIds.Rows)
            {
                Category? category = _categoryService.Get((int)CategoryId[0]);
                if (category == null)
                {
                    break;
                }
                categories.Add(category);
            }
            return categories.ToArray();
        }

        public Product[] GetProducts(int categoryId)
        {
            List<Product> products = new List<Product>();

            DataTable productIds = _db.Select("Product_Category", ["product_id"], $"category_id={categoryId}");

            foreach (DataRow productId in productIds.Rows)
            {
                Product? product = _productService.Get((int)productId[0]);
                if (product == null)
                {
                    break;
                }
                products.Add(product);
            }
            return products.ToArray();
        }
    }
}
