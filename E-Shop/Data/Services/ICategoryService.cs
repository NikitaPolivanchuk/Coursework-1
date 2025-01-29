using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface ICategoryService
    {
        public void Add(Category category);
        public Category? Get(int id);
        public Category? Get(string name);
        public Category[] GetAll();
        public void Update(int id, Category category);
        public void Delete(int id);
    }
}
