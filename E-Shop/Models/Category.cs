using System.Data;

namespace E_Shop.Models
{
    internal class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Category(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public Category(DataRow data)
        {
            Id = (int)data[0];
            Name = (string)data[1];
            Description = (string)data[2];
        }
    }
}
