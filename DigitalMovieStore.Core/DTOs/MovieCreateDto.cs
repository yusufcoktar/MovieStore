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
        public string OriginalTitle { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public int Duration { get; set; }
        public string Director { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public double ImdbRating { get; set; }
        public string Language { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public string AgeLimit { get; set; } = string.Empty;
        public string SubtitleLanguages { get; set; } = string.Empty;

        public string PosterUrl { get; set; } = string.Empty;
        public string BackdropUrl { get; set; } = string.Empty;
        public string TrailerUrl { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public decimal? DiscountedPrice { get; set; }

        // Frontend'den seçilen türlerin (Aksiyon, Dram vb.) isimlerini taşıyacak liste
        public List<string> Genres { get; set; } = new List<string>();

        // Frontend'den eklenecek oyuncu isimleri listesi (Örn: ["Keanu Reeves", "Carrie-Anne Moss"])
        public List<string> Cast { get; set; } = new List<string>();
    }
}
