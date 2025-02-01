using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Data.Enums;
using E_Shop.Models;
using System.Data;

namespace E_Shop.Data.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDbConnection _connection;

        public OrderService(DbConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.Connection;
        }

        public void Add(Order order)
        {
            _connection.Insert(order);
        }
        public Order? Get(int userId)
        {
            var filters = new Filters(LogicalOperator.And);
            filters.AddFilter("user_id", SqlOperator.Equal, userId);

            return Get(filters);
        }

        public Order? GetCurrent(int userId)
        {
            var filters = new Filters();
            filters.AddFilter("user_id", SqlOperator.Equal, userId);
            filters.AddFilter("status", SqlOperator.Equal, OrderStatus.Active.ToString());

            return Get(filters);
        }

        private Order? Get(Filters filters)
        {
            return _connection.Select<Order>(filters).FirstOrDefault();
        }

        public void Update(int id, Order order)
        {
            order.Id = id;
            _connection.Update(order);
        }

        public Order[] GetAll(int userId)
        {
            var filters = new Filters();
            filters.AddFilter("user_id", SqlOperator.Equal, userId);

            return _connection.Select<Order>(filters).ToArray();
        }
    }
}
