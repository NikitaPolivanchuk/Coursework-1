using System.Data;

namespace E_Shop.Models
{
    internal class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Number {  get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }

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
