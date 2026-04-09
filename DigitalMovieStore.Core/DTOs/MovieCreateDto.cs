using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.DTOs
{
    // Bu sınıf sadece dışarıdan film EKLENİRKEN kabul edeceğimiz verileri temsil eder.
    // İçinde IsActive, Id, veya TrailerUrl gibi dışarıdan gelmesini istemediğimiz bilgiler YOKTUR.
    public class MovieCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Dikkat: Movie sınıfında "ReleaseDate" var ama biz frontend'den sadece "Yıl" isteyeceğiz.
        public int ReleaseYear { get; set; }
    }
}
