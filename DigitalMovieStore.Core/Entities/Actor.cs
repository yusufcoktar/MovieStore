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
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // Bir oyuncu birden fazla filmde oynamış olabilir.
        public ICollection<Movie> Movies { get; set; }
    }
}
