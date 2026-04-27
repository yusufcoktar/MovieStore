using Microsoft.AspNetCore.Http;
using DigitalMovieStore.Core.DTOs;
using DigitalMovieStore.Core.Entities;
using DigitalMovieStore.Data.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DigitalMovieStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration; // appsettings'i okuyacak ajanımız

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Email == request.Email || u.Username == request.Username);
            if (userExists) return BadRequest("Bu e-posta veya kullanıcı adı zaten kullanımda!");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Kayıt işlemi başarıyla tamamlandı!" });
        }

        // --- YENİ EKLENEN GİRİŞ YAPMA METODU ---
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            // 1. Kullanıcıyı Email ile veritabanında bul
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("Kullanıcı bulunamadı.");

            // 2. Şifreyi doğrula (Kriptolu şifre ile girilen şifreyi BCrypt karşılaştırır)
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return BadRequest("Hatalı şifre girdiniz.");

            // 3. Her şey doğruysa Token (Pasaport) üret ve ver
            string token = CreateToken(user);

            return Ok(new { Token = token, Message = "Giriş başarılı!" });
        }

        // --- JWT PASAPORTU ÜRETEN MATBAA ---
        private string CreateToken(User user)
        {
            // Pasaportun içine kullanıcının temel bilgilerini (Kimlik, İsim, Rol) yazıyoruz
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // appsettings.json'daki gizli damgamızı alıyoruz
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value!));

            // Damgayı basıyoruz
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Pasaportu (Token) oluşturuyoruz (1 günlük ömrü var)
            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt:Issuer").Value,
                audience: _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            // Pasaportu metin (string) formatına çevirip teslim ediyoruz
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
    }
}
