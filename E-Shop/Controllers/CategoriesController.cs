﻿using E_Shop.Data.Services;
using E_Shop.Models;
using E_Shop.Services;
using E_Shop.Utility;
using System.Net;
using System.Text;
using Webserver.Controllers;
using Webserver.Controllers.Attributes;
using Webserver.Controllers.Content;
using Webserver.Sessions;

namespace E_Shop.Controllers
{
    internal class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly LayoutBuilder _layoutBuilder;

        private readonly string _tableRow;

        public CategoriesController(
            ICategoryService categoryService,
            LayoutBuilder layoutBuilder)
        {
            _tableRow = File.ReadAllText($"{AbsolutePath}Views/Categories/_row.html");

            _categoryService = categoryService;
            _layoutBuilder = layoutBuilder;
        }

        [HttpGet]
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

            _layoutBuilder.Configure(session);
            return View("Categories", "Categories/index.html", data.ToString());
        }

        [HttpGet]
        public IActionResult Create(Session session)
        {
            _layoutBuilder.Configure(session);

            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            return View("Create category", "Categories/create.html", Validator.Fill(4));
        }

        [HttpPost]
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
                _layoutBuilder.Configure(session);

                return View("Create error", "Categories/create.html", validator.ToValueError());
            }

            Category category = new Category(data["name"], data["description"]);
            _categoryService.Add(category);

            return Redirect("Index");
        }

        [HttpGet("Edit/{id:int}")]
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

            _layoutBuilder.Configure(session);
            return View("Edit", "Categories/edit.html", category.Name, "", category.Description, "");
        }

        [HttpPost("Edit/{id:int}")]
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
                _layoutBuilder.Configure(session);

                return View("Edit error", "Categories/edit.html", validator.ToValueError());
            }

            Category category = new Category(data["name"], data["description"]);
            _categoryService.Update(id, category);

            return Redirect("../Index");
        }

        [HttpGet("Delete/{id:int}")]
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
