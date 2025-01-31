using DbToolkit.Filtering;
using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface IProductService
    {
        public void Add(Product product);
        public Product? Get(int id);
        public Product? Get(string name);
        public Product[] GetAll(Filters? filters = null);
        public void Delete(Product product);
        public void Update(int id, Product product);
        public void Update(int id, Product product, string[] categories);
    }
}
