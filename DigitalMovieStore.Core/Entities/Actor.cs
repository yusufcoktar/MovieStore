using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.Entities
{
    public class Actor//OYUNCULAR İÇİN CLASS
    {
        public int Id { get; set; }

        // İşte eksik olan Zemberek!
        public string Name { get; set; } = string.Empty;

        // Köprünün diğer ucu: Bir oyuncu birden fazla filmde oynayabilir
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}
