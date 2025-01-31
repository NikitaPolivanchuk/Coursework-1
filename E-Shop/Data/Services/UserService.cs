using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class UserService : IUserService
    {
        private readonly IDbConnection _connection;

        public UserService()
        {
            _connection = DbConnectionProvider.GetInstance().Connection;
        }

        public void Add(User user)
        {
            _connection.Insert(user);
        }

        public User? Get(int id)
        {
            var filters = new Filters();
            filters.AddFilter("id", SqlOperator.Equal, id);

            return Get(filters);
        }

        public User? Get(string email)
        {
            var filters = new Filters();
            filters.AddFilter("email", SqlOperator.Equal, email);

            return Get(filters);
        }

        public User? Get(string email, string password)
        {
            var filters = new Filters();
            filters.AddFilter("email", SqlOperator.Equal, email);
            filters.AddFilter("id", SqlOperator.Equal, email);

            return Get(filters);
        }

        public User[] GetAll(string accessLevel)
        {
            var filters = new Filters();
            filters.AddFilter("access_level", SqlOperator.Equal, accessLevel);

            return _connection.Select<User>(filters).ToArray();
        }

        public void Update(int id, User user)
        {
            user.Id = id;
            _connection.Update(user);
        }

        private User? Get(Filters filters)
        {
            return _connection.Select<User>(filters).FirstOrDefault();
        }
    }
}
