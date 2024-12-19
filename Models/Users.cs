using System.ComponentModel.DataAnnotations;

namespace EBookSystem.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string Address { get; set; }
        public int PhoneNumber {  get; set; }
        public string Role { get; set; }
        public DateTime DateRegistered { get; set; } = DateTime.Now;
    }
}
