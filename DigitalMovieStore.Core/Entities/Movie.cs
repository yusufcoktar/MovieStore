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
        // Primary Key (Birincil Anahtar)
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        // Dijital filmin fiyatı (Para birimleri için her zaman decimal kullanılır)
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public double ImdbRating { get; set; }

        // 1. Fragman linki zorunlu olmasın (string yanına ? işareti koyuyoruz)
        public string? TrailerUrl { get; set; }

        // Mimar Dokunuşu: Soft Delete (Geçici Silme)
        // Bir filmi veritabanından tamamen silmeyiz, sadece IsActive = false yaparız.
        // Böylece geçmişte filmi satın alan kullanıcıların kütüphanesinden film kaybolmaz.
        public bool IsActive { get; set; } = true;

        // Not: İlerleyen adımlarda buraya Yönetmen, Türler ve Oyuncular 
        // ile olan ilişkileri (Navigation Properties) ekleyeceğiz.

        // Çoka-Çok İlişkiler (Navigation Properties)
        // 2. Oyuncular ve Türler liste olarak boş gelebilsin (new List diyerek başlatıyoruz)
        public ICollection<Actor> Actors { get; set; } = new List<Actor>();
        public ICollection<Genre> Genres { get; set; } = new List<Genre>();

        // Satın almalar ve Yorumlar için yeni ilişkiler
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
