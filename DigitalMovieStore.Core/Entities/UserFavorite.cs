using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.Entities
{
    public class UserFavorite
    {
        public int Id { get; set; }

        // İlişkiler
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int MovieId { get; set; }
        public Movie Movie { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
