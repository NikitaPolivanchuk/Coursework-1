using E_Shop.Models;

namespace E_Shop.Data.Services
{
    internal interface IOrderItemsService
    {
        public void Add(OrderItem item);
        public OrderItem[] GetAll(int orderId);
        public void Delete(OrderItem item);
    }
}
