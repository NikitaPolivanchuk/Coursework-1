using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class CartService : ICartService
    {
        private readonly DbAccess _db;
        private readonly IProductService _productService;

        public CartService(IProductService productService)
        {
            _db = DbAccess.GetInstance();
            _productService = productService;
        }

        public void Add(int userId, int productId)
        {
            DataTable data = _db.Select("CartItems", ["number"],
                $"user_id={userId} AND product_id={productId}");

            if (data.Rows.Count < 1)
            {
                _db.Insert("CartItems", ["user_id", "product_id", "number"],
                    [userId.ToString(), productId.ToString(), "1"]);
            }
            else
            {
                int number = (int)data.Rows[0][0] + 1;

                Dictionary<string, string> newData = new Dictionary<string, string>()
                {
                    {"number", number.ToString()}
                };

                _db.Update("CartItems", newData, $"user_id={userId} AND product_id={productId}");
            }           
        }

        public Dictionary<Product, int> GetCartItems(int userId)
        {
            Dictionary<Product, int> cartItems = new Dictionary<Product, int>();

            DataTable data = _db.Select("CartItems", ["product_id", "number"], $"user_id={userId}");

            foreach (DataRow row in data.Rows)
            {
                Product? product = _productService.Get((int)row[0]);

                if (product != null)
                {
                    cartItems[product] = (int)row[1];
                }
            }
            return cartItems;
        }

        public void Delete(int userId, int productId)
        {
            DataTable data = _db.Select("CartItems", ["number"],
                $"user_id={userId} AND product_id={productId}");

            if (data.Rows.Count < 1)
            {
                return;
            }

            int number = (int)data.Rows[0][0] - 1;

            if (number < 1)
            {
                _db.Delete("CartItems", $"user_id={userId} AND product_id={productId}");
            }
            else
            {
                Dictionary<string, string> newData = new Dictionary<string, string>()
                {
                    {"number", number.ToString()}
                };

                _db.Update("CartItems", newData, $"user_id={userId} AND product_id={productId}");
            }
        }
        public void DeleteAll(int userId)
        {
            _db.Delete("CartItems", $"user_id={userId}");
        }
    }
}
