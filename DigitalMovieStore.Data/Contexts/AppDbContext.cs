using DigitalMovieStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalMovieStore.Data.Contexts
{
   // DbContext'ten miras alarak bu sınıfın bir veritabanı köprüsü olduğunu belirtiyoruz.
    public class AppDbContext : DbContext
    {
        // Dependency Injection (Bağımlılık Enjeksiyonu) ile API katmanından 
        // bağlantı adresimizi (Connection String) almak için yapıcı metot (Constructor)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // C# sınıflarımızın (Entity) veritabanındaki tablo karşılıkları (DbSet)
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Actor> Actors { get; set; }

        // E-Ticaret ve Kullanıcı Yönetimi Tabloları
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Review> Reviews { get; set; }

        // İleride tabloların kısıtlamalarını (Maksimum karakter, zorunlu alanlar vb.)
        // ayarlamak istersek OnModelCreating metodunu burada ezeceğiz.
    }
}
