using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; } // 1 ile 5 arası puan
        public string Comment { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; } = DateTime.Now;

        // İlişkiler (Foreign Keys)
        public int UserId { get; set; }
        public User User { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }
    }
}
