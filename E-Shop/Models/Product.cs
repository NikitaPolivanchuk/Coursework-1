using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace E_Shop.Models
{
    [Table("Products")]
    public class Product
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("price")]
        public double Price { get; set; }

        [Column("number")]
        public int Number {  get; set; }

        [Column("image_url")]
        public string ImageUrl { get; set; }

        [Column("description")]
        public string Description { get; set; }

        public Product() { }

        public Product(Dictionary<string, string> data)
        {
            Name = data["name"];
            Price = double.Parse(data["price"]);
            Number = 0;
            ImageUrl = data["image-url"];
            Description = data["description"];
        }

        public Product(DataRow data)
        {
            Id = (int)data[0];
            Name = (string)data[1];
            Price = (double)data[2];
            Number = (int)data[3];
            ImageUrl = (string)data[4];
            Description = (string)data[5];
        }
    }
}
