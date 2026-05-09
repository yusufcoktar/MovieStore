using DigitalMovieStore.Core.Entities;
using DigitalMovieStore.Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace DigitalMovieStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sadece giriş yapanlar kullanabilir
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FavoritesController(AppDbContext context) { _context = context; }

        // Kullanıcının tüm favorilerini getir
        [HttpGet]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var favorites = await _context.UserFavorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Movie)
                .Select(f => f.Movie) // Sadece film bilgilerini dönüyoruz
                .ToListAsync();
            return Ok(favorites);
        }

        // Favoriye ekle veya çıkar (Toggle)
        [HttpPost("{movieId}")]
        public async Task<IActionResult> ToggleFavorite(int movieId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var existing = await _context.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.MovieId == movieId);

            if (existing != null)
            {
                _context.UserFavorites.Remove(existing);
                await _context.SaveChangesAsync();
                return Ok(new { IsFavorite = false });
            }

            _context.UserFavorites.Add(new UserFavorite { UserId = userId, MovieId = movieId });
            await _context.SaveChangesAsync();
            return Ok(new { IsFavorite = true });
        }
    }
}
