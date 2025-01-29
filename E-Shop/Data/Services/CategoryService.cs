using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class CategoryService : ICategoryService
    {
        private readonly DbAccess _db;

        public CategoryService()
        {
            _db = DbAccess.GetInstance();
        }

        public void Add(Category category)
        {
            _db.Insert("Categories", ["name", "description"], [category.Name, category.Description]);
        }

        public void Delete(int id)
        {
            _db.Delete("Categories", $"id={id}");
        }

        public Category? Get(int id)
        {
            return _Get($"id={id}");
        }

        public Category? Get(string name)
        {
            return _Get($"name='{name}'");
        }

        private Category? _Get (string condition)
        {
            DataTable data = _db.Select("Categories", ["*"], condition);

            if (data.Rows.Count < 1)
            {
                return null;
            }
            return new Category(data.Rows[0]);
        }

        public Category[] GetAll()
        {
            DataTable data = _db.Select("Categories", ["*"]);

            List<Category> categories = new List<Category>();

            foreach (DataRow row in data.Rows)
            {
                categories.Add(new Category(row));
            }
            return categories.ToArray();
        }

        public void Update(int id, Category category)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "name", category.Name },
                { "description", category.Description }
            };

            _db.Update("Categories", dic, $"id={id}");
        }
    }
}
