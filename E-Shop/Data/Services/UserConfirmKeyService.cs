using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class UserConfirmKeyService : IUserConfirmKeyService
    {
        private readonly IDbConnection _connection;

        public UserConfirmKeyService()
        {
            _connection = DbConnectionProvider.GetInstance().Connection;
        }

        public void Add(UserConfirmKey confirmKey)
        {
            _connection.Insert(confirmKey);
        }

        public void Delete(int userId)
        {
            var query = @"
                DELETE FROM UserConfirmKeys
                WHERE user_id = @user_id
            ";
            var parameters = new Dictionary<string, object?>()
            {
                { "user_id", userId },
            };

            _connection.Execute(query, parameters);
        }

        public User? Get(string key)
        {
            var filters = new Filters();
            filters.AddFilter("secret_key", SqlOperator.Equal, key);

            var confirmKey = _connection.Select<UserConfirmKey>(filters).FirstOrDefault();
            if (confirmKey == null)
            {
                return null;
            }

            filters = new Filters();
            filters.AddFilter("id", SqlOperator.Equal, confirmKey.UserId);
            
            return _connection.Select<User>(filters).FirstOrDefault();
        }

        public User? Get(int userId)
        {
            var filters = new Filters();
            filters.AddFilter("user_id", SqlOperator.Equal, userId);

            var confirmKey = _connection.Select<UserConfirmKey>(filters).FirstOrDefault();
            if (confirmKey == null)
            {
                return null;
            }

            filters = new Filters();
            filters.AddFilter("id", SqlOperator.Equal, confirmKey.UserId);

            return _connection.Select<User>(filters).FirstOrDefault();
        }

        public void Update(UserConfirmKey confirmKey)
        {
            _connection.Update(confirmKey);
        }
    }
}
