using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace E_Shop.Models
{
    [Table("Users")]
    public class User
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("access_level")]
        public string AccessLevel { get; set; }

        [Column("verified")]
        public bool Verified { get; set; }

        public User() { }

        public User(DataRow data)
        {
            Id = (int)data[0];
            Username = (string)data[1];
            Email = (string)data[2];
            Password = (string)data[3];
            AccessLevel = (string)data[4];
            Verified = (bool)data[5];
        }

        public User(Dictionary<string, string> data)
        {
            Username = data["name"];
            Email = data["email"];
            Password = data["psw"];
            AccessLevel = "user";
            Verified = false;
        }
    }
}
