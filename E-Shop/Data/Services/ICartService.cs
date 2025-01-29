using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface ICartService
    {
        public void Add(int userId, int productId);
        public Dictionary<Product, int> GetCartItems(int userId);
        public void Delete(int userId, int productId);
        public void DeleteAll(int userId);
    }
}
