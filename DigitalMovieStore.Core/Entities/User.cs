using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Güvenlik için şifreleri açık tutmayacağız
        public string Role { get; set; } = "User"; // Admin veya User olabilir
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Bir kullanıcının birden fazla siparişi ve yorumu olabilir
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
