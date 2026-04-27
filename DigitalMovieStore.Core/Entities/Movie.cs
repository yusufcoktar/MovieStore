using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Core.Entities
{
    public class Movie
    {
        // --- 1. MEVCUT TEMEL BİLGİLER (KORUNANLAR) ---
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public DateTime ReleaseDate { get; set; } // Senin orijinal yapın
        public double ImdbRating { get; set; }

        public string? TrailerUrl { get; set; } // Fragman linki zorunlu olmasın
        public bool IsActive { get; set; } = true; // Soft Delete mantığı

        // --- 2. FRONTEND ŞABLONU İÇİN YENİ EKLENENLER ---
        public string OriginalTitle { get; set; } = string.Empty;
        public int Duration { get; set; } // Süre (dk)
        public string Director { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty; // 4K, HD vs.
        public string AgeLimit { get; set; } = string.Empty; // 13+, 18+ vs.
        public string SubtitleLanguages { get; set; } = string.Empty;

        public string PosterUrl { get; set; } = string.Empty;
        public string BackdropUrl { get; set; } = string.Empty; // Arka Plan

        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountedPrice { get; set; } // İndirimli Fiyat (Opsiyonel)

        // --- 3. ÇOKA-ÇOK İLİŞKİLER (ASLA SİLMEMEMİZ GEREKENLER) ---
        public ICollection<Actor> Actors { get; set; } = new List<Actor>();
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
