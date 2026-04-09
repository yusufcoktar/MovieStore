using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DigitalMovieStore.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using DigitalMovieStore.Core.Entities;
using DigitalMovieStore.Core.DTOs;

namespace DigitalMovieStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Dependency Injection (Bağımlılık Enjeksiyonu): 
        // Veritabanı motorumuzu (AppDbContext) bu kapının içine alıyoruz.
        public MoviesController(AppDbContext context)
        {
            _context = context;
        }

        
        // Bu metod, dışarıdan biri API'mize "Bana filmleri ver" dediğinde çalışacak.
        // GET: api/movies
        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            // SİHİRLİ DOKUNUŞ: .Where(m => m.IsActive) 
            // SQL'e "Sadece IsActive değeri true olanları getir" diyoruz.
            var movies = await _context.Movies
                                       .Where(m => m.IsActive)
                                       .ToListAsync();

            return Ok(movies);
        }

        // POST: api/movies
        // Bu metod, dışarıdan (React/Angular veya Swagger) JSON formatında bir film verisi geldiğinde çalışacak.
        [HttpPost]
        public async Task<IActionResult> AddMovie(MovieCreateDto movieDto)
        {
            // 1. Adım: Dışarıdan gelen o kısıtlı DTO verisini, veritabanımızın anladığı asıl Movie sınıfına dönüştürüyoruz (Mapping).
            var newMovie = new Movie
            {
                Title = movieDto.Title,
                Description = movieDto.Description,
                Price = movieDto.Price,

                // Mimar Dokunuşu: Dışarıdan sadece "Yıl" (Örn: 2010) aldık, 
                // bunu veritabanının istediği "Tarih" formatına (1 Ocak 2010) kod içinde biz çeviriyoruz.
                ReleaseDate = new DateTime(movieDto.ReleaseYear, 1, 1),

                // Güvenlik: Dışarıdan kimsenin IsActive göndermesine izin vermedik, biz kendimiz True yapıyoruz.
                IsActive = true
            };

            // 2. Adım: Dönüştürdüğümüz bu temiz ve güvenli asıl nesneyi veritabanına ekliyoruz.
            await _context.Movies.AddAsync(newMovie);
            await _context.SaveChangesAsync();

            // İşlem başarılı! Şimdilik eklenen filmin ID'sini ve ufak bir başarı mesajı dönelim.
            return Ok(new
            {
                Id = newMovie.Id,
                Title = newMovie.Title,
                Message = "Film güvenli bir şekilde (DTO üzerinden) eklendi!"
            });
        }
        // DELETE: api/movies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            // 1. Adım: Veritabanından gönderilen ID'ye sahip filmi bul.
            var movie = await _context.Movies.FindAsync(id);

            // Eğer film yoksa (veya yanlış ID girildiyse) kibarca 404 Not Found dön.
            if (movie == null)
            {
                return NotFound(new { Message = "Film bulunamadı veya zaten silinmiş." });
            }

            // 2. Adım: Profesyonel Dokunuş (Soft Delete).
            // _context.Movies.Remove(movie); // ASLA BUNU YAPMIYORUZ!
            movie.IsActive = false; // Filmi sadece arşive kaldırıyoruz.

            // 3. Adım: Değişiklikleri SQL'e kaydet.
            await _context.SaveChangesAsync();

            // 4. Adım: Başarı mesajı dön.
            return Ok(new
            {
                Message = $"'{movie.Title}' adlı film mağaza vitrininden başarıyla kaldırıldı (Soft Delete uygulandı).",
                DeletedMovieId = movie.Id
            });
        }
    }
}
