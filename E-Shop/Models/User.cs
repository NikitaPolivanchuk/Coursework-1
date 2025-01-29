using System.Data;

namespace E_Shop.Models
{
    internal class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string AccessLevel { get; set; }
        public bool Verified { get; set; }

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
