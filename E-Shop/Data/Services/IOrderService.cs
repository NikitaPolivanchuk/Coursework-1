using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface IOrderService
    {
        public void Add(Order order);
        public Order? Get(int userId);
        public Order? GetCurrent(int userId);
        public Order[] GetAll(int userId);
        public void Update(int id, Order order);
    }
}
