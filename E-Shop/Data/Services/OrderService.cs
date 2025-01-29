using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class OrderService : IOrderService
    {
        private readonly DbAccess _db;

        public OrderService()
        {
            _db = DbAccess.GetInstance();
        }

        public void Add(Order order)
        {
            _db.Insert("Orders", ["user_id", "total_cost", "status"],
                [order.UserId.ToString(), order.TotalCost.ToString(), order.Status]);
        }
        public Order? Get(int userId)
        {
            return _Get($"user_id={userId}");
        }

        public Order? GetCurrent(int userId)
        {
            return _Get($"user_id={userId} AND status='Active'");
        }

        private Order? _Get(string condition)
        {
            DataTable data = _db.Select("Orders", ["*"], condition);

            if (data.Rows.Count < 1)
            {
                return null;
            }

            return new Order(data.Rows[0]);
        }

        public void Update(int id, Order order)
        {
            Dictionary<string, string> updated = new Dictionary<string, string>()
            {
                {"status",  order.Status}
            };

            _db.Update("Orders", updated, $"id={id}");
        }

        public Order[] GetAll(int userId)
        {
            List<Order> orders = new List<Order>();
            DataTable data = _db.Select("Orders", ["*"], $"user_id={userId}");

            foreach (DataRow row in data.Rows)
            {
                orders.Add(new Order(row));
            }
            return orders.ToArray();
        }
    }
}
