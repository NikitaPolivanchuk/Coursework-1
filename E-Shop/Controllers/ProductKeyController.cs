using E_Shop.Data.Services;
using E_Shop.Models;
using E_Shop.Utility;
using System.Net;
using System.Text;
using Webserver.Controllers;
using Webserver.Controllers.Content;
using Webserver.Sessions;

namespace E_Shop.Controllers
{
    internal class ProductKeyController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductKeyService _productKeyService;

        public ProductKeyController(IProductService productService,
                                    IProductKeyService productKeyService)
        {
            _productService = productService;
            _productKeyService = productKeyService;
        }

        [Endpoint("GET", "ProductKeys/{id:int}/Index")]
        public IActionResult Index(Session session, int id)
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

            string row = File.ReadAllText($"{AbsolutePath}Views/ProductKey/_row.html");
            ProductKey[] productKeys = _productKeyService.GetAll(product.Id);

            StringBuilder rows = new StringBuilder();

            foreach (ProductKey productKey in productKeys)
            {
                rows.AppendLine(string.Format(row, productKey.Value, productKey.Id));    
            }

            return View($"{product.Name} keys", "ProductKey/index.html", product.Name, product.Number, rows);
        }

        [Endpoint("POST", "ProductKeys/{id:int}/Add")]
        public IActionResult Create(Session session, int id, string key)
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

            if (string.IsNullOrEmpty(key))
            {
                return Redirect("Index");
            }

            ProductKey productKey = new ProductKey(product.Id, key);
            _productKeyService.Add(productKey);

            product.Number += 1;
            _productService.Update(product.Id, product);

            return Redirect("Index");
        }

        [Endpoint("GET", "ProductKeys/Delete/{id:int}")]
        public IActionResult Delete(Session session, int id)
        {
            if (!session.IsAdmin())
            {
                return Error(HttpStatusCode.Forbidden);
            }

            ProductKey? productKey =  _productKeyService.Get(id);
            if (productKey == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            Product? product = _productService.Get(productKey.ProductId);
            if (product == null)
            {
                return Error(HttpStatusCode.NotFound);
            }

            _productKeyService.Delete(productKey);

            if (product.Number > 0)
            {
                product.Number -= 1;
                _productService.Update(product.Id, product);
            }

            return Redirect($"../{product.Id}/Index");
        }
    }
}
