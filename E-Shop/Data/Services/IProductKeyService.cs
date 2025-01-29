using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface IProductKeyService
    {
        public void Add(ProductKey productKey);
        public ProductKey? Get(int id);
        public ProductKey[] GetAll(int productId);
        public void Delete(int id);
    }
}
