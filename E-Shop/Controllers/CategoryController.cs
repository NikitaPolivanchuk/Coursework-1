using E_Shop.Data.Services;
using E_Shop.Models;
using E_Shop.Utility;
using System.Net;
using System.Text;
using Webserver;
using Webserver.Content;
using Webserver.Utility;

namespace E_Shop.Controllers
{
    internal class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        private readonly string _tableRow;

        public CategoryController(ICategoryService categoryService)
        {
            _tableRow = File.ReadAllText($"{AbsolutePath}Views/Categories/_row.html");

            _categoryService = categoryService;
        }

        [Endpoint("GET", "Categories/Index")]
        public IActionResult Index(Session session)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            StringBuilder data = new StringBuilder();

            Category[] categories = _categoryService.GetAll();

            foreach (Category category in categories)
            {
                data.AppendLine(string.Format(_tableRow, category.Name, category.Description, category.Id));
            }

            LayoutBuilder.Configure(session, AbsolutePath);
            return View("Categories", "Categories/index.html", data.ToString());
        }

        [Endpoint("GET", "Categories/Create")]
        public IActionResult Create(Session session)
        {
            LayoutBuilder.Configure(session, AbsolutePath);

            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            return View("Create category", "Categories/create.html", Validator.Fill(4));
        }

        [Endpoint("POST", "Categories/Create")]
        public IActionResult? Create(Session session, Dictionary<string ,string> data)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            Validator validator = new Validator(data);

            validator.Validate("name");
            validator.Validate("description", null, null, false);

            if (validator.IsValid && _categoryService.Get(data["name"]) != null)
            {
                validator.SetCustomError("name", "This category already exists");
            }

            if (!validator.IsValid)
            {
                LayoutBuilder.Configure(session, AbsolutePath);

                return View("Create error", "Categories/create.html", validator.ToValueError());
            }

            Category category = new Category(data["name"], data["description"]);
            _categoryService.Add(category);

            return Redirect("Index");
        }

        [Endpoint("GET", "Categories/Edit/{id:int}")]
        public IActionResult Edit(Session session, int id)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            Category? category = _categoryService.Get(id);

            if (category == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            LayoutBuilder.Configure(session, AbsolutePath);
            return View("Edit", "Categories/edit.html", category.Name, "", category.Description, "");
        }

        [Endpoint("POST", "Categories/Edit/{id:int}")]
        public IActionResult Edit(Session session, int id, Dictionary<string, string> data)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            Validator validator = new Validator(data);

            validator.Validate("name");
            validator.Validate("description", null, null, false);

            if (!validator.IsValid)
            {
                LayoutBuilder.Configure(session, AbsolutePath);

                return View("Edit error", "Categories/edit.html", validator.ToValueError());
            }

            Category category = new Category(data["name"], data["description"]);
            _categoryService.Update(id, category);

            return Redirect("../Index");
        }

        [Endpoint("GET", "Categories/Delete/{id:int}")]
        public IActionResult Delete(Session session, int id)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            _categoryService.Delete(id);

            return Redirect("../Index");
        }
    }
}
