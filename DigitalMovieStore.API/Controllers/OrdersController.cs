using DigitalMovieStore.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalMovieStore.Data.Contexts;

namespace DigitalMovieStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // 1. KULLANICININ SİPARİŞLERİNİ GETİRME (GET) - React'taki "Siparişlerim" sayfası buraya istek atar
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserOrders(int userId)
        {
            // Kullanıcının siparişlerini, film detaylarıyla (Include) birlikte en yeniler en üstte olacak şekilde çekiyoruz
            var orders = await _context.Orders
                .Include(o => o.Movie)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Eğer kullanıcının hiç siparişi yoksa boş liste dön
            if (!orders.Any())
            {
                return Ok(new List<object>());
            }

            // React'ın tam olarak beklediği kolay formata (JSON) çeviriyoruz
            var result = orders.Select(o => new {
                id = "ORD-" + o.Id.ToString().PadLeft(3, '0'), // Örn: ORD-001, ORD-002
                title = o.Movie.Title,
                poster = o.Movie.PosterUrl, // Senin veritabanına uygun
                price = o.PurchasePrice,
                date = o.OrderDate.ToString("dd MMMM yyyy")
            });

            return Ok(result);
        }

        // 2. YENİ SİPARİŞ OLUŞTURMA (POST) - React'taki "Ödemeyi Tamamla" butonu buraya veri fırlatır
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
        {
            // 1. GÜVENLİK KONTROLÜ: Gerçekten veritabanında böyle bir kullanıcı var mı?
            var user = await _context.Users.FindAsync(request.UserId);

            if (user == null)
            {
                // Sahte hesap açmak YOK! Direkt işlemi reddediyoruz. Tüm kullanıcılar için standart güvenlik.
                return BadRequest(new { Message = "Güvenlik İhlali: Geçerli bir kullanıcı bulunamadı. Lütfen hesabınıza tekrar giriş yapın." });
            }

            // 2. React'tan gelen ID listesine göre filmleri veritabanından çek (Fiyat güvenliği)
            var movies = await _context.Movies
                .Where(m => request.MovieIds.Contains(m.Id))
                .ToListAsync();

            if (!movies.Any())
            {
                return BadRequest(new { Message = "Sepetteki filmler veritabanında bulunamadı." });
            }

            var newOrders = new List<Order>();

            // 3. Sepetteki her bir film için veritabanına ayrı bir satır (Order) ekle
            foreach (var movie in movies)
            {
                // Eğer Movie modelinde indirimli fiyat (DiscountedPrice) özelliği varsa onu, yoksa normal Price'ı alır.
                decimal finalPrice = movie.Price;

                var order = new Order
                {
                    UserId = request.UserId, // Kim giriş yaptıysa dinamik olarak onun ID'si!
                    MovieId = movie.Id,
                    PurchasePrice = finalPrice,
                    OrderDate = DateTime.Now
                };

                newOrders.Add(order);
            }

            // 4. Tüm siparişleri tek seferde veritabanına ekle ve SQL'e MÜHÜRLE
            _context.Orders.AddRange(newOrders);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Siparişler başarıyla oluşturuldu!",
                PurchasedCount = newOrders.Count
            });
        }

        // 3. KULLANICININ KÜTÜPHANESİNİ GETİRME (GET) - React'taki "Kütüphanem" sayfası buraya istek atar
        [HttpGet("Library/{userId}")]
        public async Task<IActionResult> GetUserLibrary(int userId)
        {
            // Kullanıcının sipariş ettiği filmleri buluyoruz. 
            // Distinct() kullanıyoruz çünkü aynı filmi 2 kez aldıysa kütüphanede 1 kez görünsün.
            var libraryMovies = await _context.Orders
                .Where(o => o.UserId == userId)
                .Select(o => o.Movie)
                .Distinct()
                .ToListAsync();

            if (!libraryMovies.Any())
            {
                return Ok(new List<object>()); // Kütüphane boşsa boş liste dön
            }

            // React'ın Kütüphane Film modeline uygun olarak JSON dönüyoruz
            var result = libraryMovies.Select(m => new {
                id = m.Id.ToString(),
                title = m.Title,
                poster = m.PosterUrl, // Düzeltilmiş isim
                price = m.Price,
                discountPrice = m.DiscountedPrice,
                year = m.ReleaseDate.Year,
                genres = new[] { "Dijital Kopya" }, // UI tarafında görünmesi için
                rating = m.ImdbRating
            });

            return Ok(result);
        }
    }

    // React'tan gelecek olan verinin şablonu (DTO)
    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public List<int> MovieIds { get; set; } = new List<int>();
    }
}