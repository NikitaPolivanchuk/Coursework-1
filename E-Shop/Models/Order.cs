using System.Data;

namespace E_Shop.Models
{
    internal class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public double TotalCost { get; set; }
        public string Status { get; set; }

        public Order(int userId)
        {
            UserId = userId;
            TotalCost = 0;
            Status = "Active";
        }

        public Order(DataRow data)
        {
            Id = (int)data[0];
            UserId = (int)data[1];
            TotalCost = (double)data[2];
            Status = (string)data[3];
        }
    }
}
