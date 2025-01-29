using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface IProductCategoryService
    {
        public void Add(int productId, int categoryId);
        public Category[] GetCategories(int productId);
        public Product[] GetProducts(int categoryId);
    }
}
