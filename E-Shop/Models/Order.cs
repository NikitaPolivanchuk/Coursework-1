using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace E_Shop.Models
{
    [Table("Orders")]
    internal class Order
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("total_cost")]
        public double TotalCost { get; set; }

        [Column("status")]
        public string Status { get; set; }

        public Order() { }

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
