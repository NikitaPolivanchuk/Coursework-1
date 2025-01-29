﻿using E_Shop.Data.Services;
using E_Shop.Models;
using E_Shop.Utility;
using System.Net;
using System.Text;
using Webserver;
using Webserver.Content;
using Webserver.Utility;

namespace E_Shop.Controllers
{
    internal class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IProductCategoryService _productCategoryService;

        private readonly string _tableRow;
        private readonly string _soldOut;

        public ProductsController(IProductService productService,
                                  ICategoryService categoryService,
                                  IProductCategoryService productCategoryService)
        {
            _tableRow = File.ReadAllText($"{AbsolutePath}Views/Products/_row.html");
            _soldOut = File.ReadAllText($"{AbsolutePath}Views/Products/_soldOut.html");

            _productService = productService;
            _categoryService = categoryService;
            _productCategoryService = productCategoryService;
        }

        [Endpoint("GET", "Products/Index")]
        public IActionResult Index(Session session)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            StringBuilder sb = new StringBuilder();

            Product[] products = _productService.GetAll();

            foreach (Product product in products)
            {
                Category[] categories = _productCategoryService.GetCategories(product.Id);

                List<string> categoryNames = new List<string>();

                foreach (Category category in categories)
                {
                    categoryNames.Add(category.Name);
                }

                sb.Append(string.Format(_tableRow, product.ImageUrl, product.Name, product.Price, product.Number,
                string.Join(", ", categoryNames), product.Id));          
            }
            LayoutBuilder.Configure(session, AbsolutePath);
            return View("Products", "Products/index.html", sb.ToString());
        }

        [Endpoint("GET", "Products/{id:int}")]
        public IActionResult Main(Session session, int id)
        {
            Product? product = _productService.Get(id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            Category[] categories = _productCategoryService.GetCategories(id);
            StringBuilder sb = new StringBuilder();

            string soldOut = string.Empty;
            string disabled = string.Empty;
            if (product.Number < 1)
            {
                soldOut = _soldOut;
                disabled = "disabled";
            }
            else if (!session.Authorized)
            {
                disabled = "disabled";
            }

            foreach (Category category in categories)
            {
                sb.AppendLine($"<li><a href=\"#\" class=\"btn btn-light border\">{category.Name}</a></li>");
            }

            string price = string.Format("${0:F2}", product.Price);

            LayoutBuilder.Configure(session, AbsolutePath);
            return View(product.Name, "Products/main.html", product.Name, product.ImageUrl, 
                price, soldOut, disabled, product.Id, sb.ToString(), product.Description);
        }

        [Endpoint("GET", "Products/Create")]
        public IActionResult Create(Session session)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            Category[] categories = _categoryService.GetAll();
            StringBuilder options = new StringBuilder();

            foreach (Category category in categories)
            {
                options.AppendLine($"<option value=\"{category.Id}\">{category.Name}</option>");
            }

            List<string> args = new List<string>(Validator.Fill(8));
            args.Add(options.ToString());

            return View("Create product", "Products/create.html", args.ToArray());
        }

        [Endpoint("POST", "Products/Create")]
        public IActionResult Create(Session session, Dictionary<string, string> data)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            Dictionary<string, string> dataValid = new Dictionary<string, string>()
            {
                {"name", data["name"] },
                {"price", data["price"] },
                {"image-url", data["image-url"] },
                {"description", data["description"] }
            };

            Validator validator = new Validator(dataValid);

            validator.Validate("name");
            validator.ValidateDouble("price", 0, null);
            validator.Validate("image-url", null, 512, "[^'&]*$");
            validator.Validate("description", null, null, "[^'&]*$", false);

            List<int> selectedCatIds = new List<int>();
            if (data.ContainsKey("categories"))
            {
                foreach (string catIdStr in data["categories"].Split('&'))
                {
                    if (!string.IsNullOrEmpty(catIdStr) && int.TryParse(catIdStr, out int catId))
                    {
                        selectedCatIds.Add(catId);
                    }
                }
            }

            if (!validator.IsValid)
            {
                Category[] categories = _categoryService.GetAll();
                StringBuilder options = new StringBuilder();

                foreach (Category category in categories)
                {
                    string selected = selectedCatIds.Contains(category.Id)
                        ? "selected"
                        : string.Empty;

                    options.AppendLine($"<option value=\"{category.Id}\" {selected}>{category.Name}</option>");
                }

                List<string> args = new List<string>(validator.ToValueError()) {options.ToString()};

                return View("Create product", "Products/create.html", args.ToArray());
            }

            _productService.Add(new Product(data));
            Product? product = _productService.Get(data["name"]);

            if (product != null)
            {
                if (data.TryGetValue("categories", out string? categoryIds))
                {
                    foreach (string categoryId in categoryIds.Split('&'))
                    {
                        if (!string.IsNullOrEmpty(categoryId))
                        {
                            _productCategoryService.Add(product.Id, int.Parse(categoryId));
                        }
                    }
                }
            }

            return Redirect("Index");
        }

        [Endpoint("GET", "Products/Edit/{id:int}")]
        public IActionResult Edit(Session session, int id)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            Product? product = _productService.Get(id);
            
            if (product == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            Category[] chosen = _productCategoryService.GetCategories(product.Id);
            List<int> chosenId = new List<int>();

            foreach (Category active in chosen)
            {
                chosenId.Add(active.Id);    
            }

            Category[] categories = _categoryService.GetAll();
            StringBuilder options = new StringBuilder();

            foreach (Category category in categories)
            {
                string selected = chosenId.Contains(category.Id)
                    ? "selected"
                    : "";

                options.AppendLine($"<option {selected} value=\"{category.Id}\">{category.Name}</option>");
            }

            LayoutBuilder.Configure(session, AbsolutePath);
            return View("Edit", "Products/edit.html", product.Name, "", product.Price, "",
                product.ImageUrl, "", product.Description, "", options.ToString());
        }

        [Endpoint("POST", "Products/Edit/{id:int}")]
        public IActionResult Edit(Session session, int id, Dictionary<string, string> data)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            Product? product = _productService.Get(id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            Dictionary<string, string> dataValid = new Dictionary<string, string>()
            {
                {"name", data["name"] },
                {"price", data["price"] },
                {"image-url", data["image-url"] },
                {"description", data["description"] }
            };

            Validator validator = new Validator(dataValid);

            validator.Validate("name");
            validator.ValidateDouble("price", 0, null);
            validator.Validate("image-url", null, 512, "[^'&]*$");
            validator.Validate("description", null, null, "[^'&]*$", false);

            List<int> selectedCatIds = new List<int>();
            if (data.ContainsKey("categories"))
            {
                foreach (string catIdStr in data["categories"].Split('&'))
                {
                    if (!string.IsNullOrEmpty(catIdStr) && int.TryParse(catIdStr, out int catId))
                    {
                        selectedCatIds.Add(catId);
                    }
                }
            }

            if (!validator.IsValid)
            {
                Category[] categories = _categoryService.GetAll();
                StringBuilder options = new StringBuilder();

                foreach (Category category in categories)
                {
                    string selected = selectedCatIds.Contains(category.Id)
                        ? "selected"
                        : string.Empty;

                    options.AppendLine($"<option value=\"{category.Id}\" {selected}>{category.Name}</option>");
                }

                List<string> args = new List<string>(validator.ToValueError()) { options.ToString() };

                return View("Edit", "Products/edit.html", args.ToArray());
            }

            Product updated = new Product(data);

            if (!data.TryGetValue("categories", out string? categoryIds))
            {
                categoryIds = "";
            }
            _productService.Update(id, updated, categoryIds.Split('&'));

            return Redirect("../Index");
        }

        [Endpoint("GET", "Products/Delete/{id:int}")]
        public  IActionResult Delete(Session session, int id)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            Product? product = _productService.Get(id);
            if (product == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            _productService.Delete(id);

            return Redirect("../Index");
        }
    }
}
