using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace E_Shop.Models
{
    [Table("Categories")]
    public class Category
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        public Category() { }

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
