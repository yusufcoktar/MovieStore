using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.Entities
{
    public class Genre//TÜRLER İÇİN CLASS
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Mimar Dokunuşu: Bir türün içinde birden fazla film olabilir.
        // Bu yapı, ileride "Aksiyon filmlerini getir" dediğimizde çok işimize yarayacak.
        public ICollection<Movie> Movies { get; set; }
    }
}
