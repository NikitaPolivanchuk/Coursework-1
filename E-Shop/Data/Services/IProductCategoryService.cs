using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface IProductCategoryService
    {
        public void Add(ProductCategory productCategory);
        public Category[] GetCategories(int productId);
        public Product[] GetProducts(int categoryId);
    }
}
