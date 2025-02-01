using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Models;
using System.Data;

namespace E_Shop.Data.Services
{
    public class CartService : ICartService
    {
        private readonly IDbConnection _connection;

        public CartService(DbConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.Connection;
        }

        public void Add(int userId, int productId)
        {
            var filters = new Filters();
            filters.AddFilter("user_id", SqlOperator.Equal, userId);
            filters.AddFilter("product_id", SqlOperator.Equal, productId);

            var cartItem = _connection.Select<CartItem>(filters).FirstOrDefault();

            if (cartItem == null)
            {
                var query = @"
                    INSERT INTO CartItems(user_id, product_id, number)
                    VALUES(@user_id, @product_id, @number)
                ";
                var parameters = filters.ToDictionary();
                parameters.Add("number", 1);

                _connection.Execute(query, parameters);
            }
            else
            {
                var query = @"
                    UPDATE CartItems
                    SET number = @number
                    WHERE product_id = @product_id AND user_id = @user_id
                ";
                var parameters = filters.ToDictionary();
                parameters.Add("number", cartItem.Number + 1);

                _connection.Execute(query, parameters);
            }           
        }

        public Dictionary<Product, int> GetCartItems(int userId)
        {
            var result = new Dictionary<Product, int>();

            var filters = new Filters();
            filters.AddFilter("user_id", SqlOperator.Equal, userId);
            var cartItems = _connection.Select<CartItem>(filters);

            foreach (var cartItem in cartItems)
            {
                filters = new Filters();
                filters.AddFilter("id", SqlOperator.Equal, cartItem.ProductId);
                var product = _connection.Select<Product>(filters).FirstOrDefault();

                if (product != null)
                {
                    result[product] = cartItem.Number;
                }
            }
            return result;
        }

        public void Delete(int userId, int productId)
        {
            var filters = new Filters();
            filters.AddFilter("user_id", SqlOperator.Equal, userId);
            filters.AddFilter("product_id", SqlOperator.Equal, productId);

            var cartItem = _connection.Select<CartItem>(filters).FirstOrDefault();
            if (cartItem == null)
            {
                return;
            }

            cartItem.Number -= 1;

            if (cartItem.Number < 1)
            {
                var query = @"
                    DELETE FROM CartItems
                    WHERE user_id = @user_id AND product_id = @product_id
                ";
                _connection.Execute(query, filters.ToDictionary());
            }
            else
            {
                var query = @"
                    UPDATE CartItems
                    SET number = @number
                    WHERE user_id = @user_id AND product_id = @product_id
                ";
                var parameters = filters.ToDictionary();
                parameters["number"] = cartItem.Number;

                _connection.Execute(query, parameters);
            }
        }
        public void DeleteAll(int userId)
        {
            var query = @"
                DELETE FROM CartItems
                WHERE user_id = @user_id
            ";
            var parameters = new Dictionary<string, object?>()
            {
                {"user_id", userId},
            };

            _connection.Execute(query, parameters);
        }
    }
}
