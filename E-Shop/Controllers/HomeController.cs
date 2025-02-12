﻿using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Data.Services;
using E_Shop.Models;
using E_Shop.Services;
using System.Text;
using Webserver.Controllers;
using Webserver.Controllers.Attributes;
using Webserver.Controllers.Content;
using Webserver.Sessions;

namespace E_Shop.Controllers
{
    internal class HomeController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly LayoutBuilder _layoutBuilder;

        private readonly string _section;
        private readonly string _card;

        public HomeController(
            ICategoryService categoryService,
            IProductService productService,
            IProductCategoryService productCategoryService,
            LayoutBuilder layoutBuilder)
        {
            _section = File.ReadAllText($"{AbsolutePath}Views/Home/_section.html");
            _card = File.ReadAllText($"{AbsolutePath}Views/Home/_card.html");

            _categoryService = categoryService;
            _productService = productService;
            _productCategoryService = productCategoryService;
            _layoutBuilder = layoutBuilder;
        }

        [HttpGet]
        public IActionResult Index(Session session)
        {
            Category[] categories = _categoryService.GetAll();
            StringBuilder sections = new StringBuilder();

            foreach ( Category category in categories )
            {
                Product[] products = _productCategoryService.GetProducts(category.Id);
                StringBuilder cards = new StringBuilder();

                foreach( Product product in products )
                {
                    cards.AppendLine(CreateCard(product));
                }
                sections.AppendLine(string.Format(_section, category.Name, cards.ToString()));
            }

            _layoutBuilder.Configure(session);
            return View("Home", "Home/index.html", sections.ToString());
        }

        [HttpGet]
        public IActionResult Search(Session session, string name)
        {
            var filters = new Filters();
            filters.AddFilter("name", SqlOperator.Like, $"%{name}%");

            Product[] products = _productService.GetAll(filters);

            StringBuilder cards = new StringBuilder();

            foreach (Product product in products)
            {
                cards.AppendLine(CreateCard(product));
            }
            string result = cards.Length > 0 
                ? cards.ToString() 
                : "no games found";

            string body = string.Format(_section, "Found", result);

            _layoutBuilder.Configure(session);
            return View("Search", "Home/index.html", body);
        }

        [HttpGet]
        public string GetHints()
        {
            StringBuilder names = new StringBuilder();
            Product[] products = _productService.GetAll();

            foreach (Product product in products)
            {
                names.Append(product.Name);
                names.Append("&");
            }

            return names.ToString();
        }

        private string CreateCard(Product product)
        {
            string url = $"../Products/{product.Id}";
            string price = string.Format("${0:F2}", product.Price);

            return string.Format(_card, url, product.ImageUrl, product.Name, price);
        }
    }
}
