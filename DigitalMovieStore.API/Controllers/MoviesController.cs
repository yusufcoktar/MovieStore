using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DigitalMovieStore.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using DigitalMovieStore.Core.Entities;
using DigitalMovieStore.Core.DTOs;
using Microsoft.AspNetCore.Authorization;

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
            // SİHİRLİ DOKUNUŞ 1: .Where() ile sadece aktif olanları getiriyoruz.
            // SİHİRLİ DOKUNUŞ 2: .Include() ile bu filmlere bağlı Türleri (Genres) de peşine takıyoruz (Eager Loading).
            var movies = await _context.Movies
                .Where(m => m.IsActive)
                .Include(m => m.Genres)
                .Include(m => m.Actors) // <- Oyuncuları (Cast) getiren yeni komutumuz!
                .ToListAsync();

            return Ok(movies);
        }
        // SADECE ADMİNLER İÇİN: Silinenler (Arşiv) dahil tüm filmleri getiren VIP metod
        // GET: api/movies/AdminList
        [HttpGet("AdminList")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllMoviesForAdmin()
        {
            var movies = await _context.Movies
                // DİKKAT: Burada .Where(m => m.IsActive) yok! Her şeyi çekecek.
                .Include(m => m.Genres)
                .Include(m => m.Actors)
                .ToListAsync();

            return Ok(movies);
        }

        // POST: api/movies
        // Bu metod, dışarıdan (React/Angular veya Swagger) JSON formatında bir film verisi geldiğinde çalışacak.
        [HttpPost]
        [Authorize(Roles = "Admin")] // SİHİRLİ DOKUNUŞ: Sadece Admin pasaportu olanlar film ekleyebilir!
        public async Task<IActionResult> CreateMovie([FromBody] MovieCreateDto movieDto)
        {
            // Eğer frontend eksik veya hatalı veri gönderirse (Örn: fiyat harf içerirse) anında reddet
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. DTO'dan (Gümrükten) geçen verileri gerçek Movie nesnesine aktarıyoruz (Mapping)
            var newMovie = new Movie
            {
                Title = movieDto.Title,
                OriginalTitle = movieDto.OriginalTitle,
                Description = movieDto.Description,
                Price = movieDto.Price,
                DiscountedPrice = movieDto.DiscountedPrice,

                // Frontend sadece yılı gönderiyor (Örn: 2026), biz onu SQL'in anladığı DateTime formatına çeviriyoruz
                ReleaseDate = new DateTime(movieDto.ReleaseYear, 1, 1),

                Duration = movieDto.Duration,
                Director = movieDto.Director,
                Language = movieDto.Language,
                Country = movieDto.Country,
                Resolution = movieDto.Resolution,
                AgeLimit = movieDto.AgeLimit,
                SubtitleLanguages = movieDto.SubtitleLanguages,

                PosterUrl = movieDto.PosterUrl,
                BackdropUrl = movieDto.BackdropUrl,
                TrailerUrl = movieDto.TrailerUrl,
                ImdbRating = movieDto.ImdbRating,

                IsActive = true // Yeni eklenen bir film varsayılan olarak yayındadır
            };

            // --- MİMAR DOKUNUŞU: Türleri (Genres) Veritabanı ile Eşleme ---
            if (movieDto.Genres != null && movieDto.Genres.Any())
            {
                foreach (var genreName in movieDto.Genres)
                {
                    // 1. Bu isimde bir tür veritabanında zaten var mı diye bak
                    var existingGenre = await _context.Genres.FirstOrDefaultAsync(g => g.Name == genreName);

                    if (existingGenre != null)
                    {
                        // 2. Varsa, direkt köprü tablosuyla filme bağla
                        newMovie.Genres.Add(existingGenre);
                    }
                    else
                    {
                        // 3. Yoksa (Admin yeni bir tür yazmışsa), önce türü oluştur sonra bağla!
                        newMovie.Genres.Add(new Genre { Name = genreName });
                    }
                }
            }
            // --------------------------------------------------------------
            // --- MİMAR DOKUNUŞU: Oyuncuları (Actors) Veritabanı ile Eşleme ---
            if (movieDto.Cast != null && movieDto.Cast.Any())
            {
                foreach (var actorName in movieDto.Cast)
                {
                    // Bu isimde bir oyuncu veritabanında zaten var mı?
                    var existingActor = await _context.Actors.FirstOrDefaultAsync(a => a.Name == actorName);

                    if (existingActor != null)
                    {
                        // Varsa, direkt filme bağla
                        newMovie.Actors.Add(existingActor);
                    }
                    else
                    {
                        // Yoksa, yeni bir oyuncu kaydı oluştur ve bağla!
                        newMovie.Actors.Add(new Actor { Name = actorName });
                    }
                }
            }
            // ----------------------------------------------------------------

            // 2. Veritabanına ekleme emri (Insert)
            _context.Movies.Add(newMovie);
            await _context.SaveChangesAsync();

            // 3. Başarı mesajı ve SQL'in atadığı ID ile birlikte filmi geri dön
            return Ok(new
            {
                Message = "Film başarıyla mağazaya eklendi!",
                Movie = newMovie
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
        // PATCH: api/movies/QuickEdit/5
        // Sadece Fiyat ve Durum güncelleyen "Hızlı Düzenle" metodu
        [HttpPatch("QuickEdit/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> QuickUpdateMovie(int id, [FromBody] MovieQuickEditDto editDto)
        {
            // 1. Filmi bul
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound(new { Message = "Güncellenecek film bulunamadı." });
            }

            // 2. Sadece pop-up'tan gelen verileri değiştir! (Filmin diğer hiçbir verisine DOKUNMUYORUZ)
            movie.Price = editDto.Price;
            movie.DiscountedPrice = editDto.DiscountedPrice;

            // Mimar Dokunuşu: Senin veritabanında "IsActive" olduğu için Durum'u ona göre çeviriyoruz
            if (editDto.Status == "Yayında")
                movie.IsActive = true;
            else if (editDto.Status == "Taslak" || editDto.Status == "Arşiv")
                movie.IsActive = false;

            // 3. Değişiklikleri kaydet
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Film fiyatı ve durumu başarıyla güncellendi!" });
        }
    }
}
