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
using Microsoft.AspNetCore.Authorization;
using DigitalMovieStore.Service; // 🔥 E-posta servisi için katmanımızı dahil ettik
using System;
using DigitalMovieStore.Service.Email;

namespace DigitalMovieStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService; // 🔥 Mail servisimizi tanımladık

        // 🔥 Constructor'ı güncelledik (IEmailService eklendi)
        public AuthController(AppDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto request)
        {
            // 1. Kullanıcıyı bulmaya çalış
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email || u.Username == request.Username);

            // 6 haneli doğrulama kodu üret
            var random = new Random();
            string verificationCode = random.Next(100000, 999999).ToString();
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            if (existingUser != null)
            {
                // Eğer kullanıcı var ve ONAYLIYSA -> Hata ver, bu hesap dolu!
                if (existingUser.IsEmailConfirmed)
                {
                    return BadRequest("Bu e-posta veya kullanıcı adı zaten kullanımda!");
                }
                else
                {
                    // Kullanıcı var ama ONAYSIZ! (Senin yaşadığın senaryo: Yarıda bırakıp çıkmış)
                    // Bilgilerini güncelleyip yeni kod veriyoruz, veritabanını çöplüğe çevirmiyoruz.
                    existingUser.PasswordHash = passwordHash;
                    existingUser.ResetCode = verificationCode;
                    existingUser.ResetCodeExpires = DateTime.Now.AddMinutes(15);
                }
            }
            else
            {
                // Kullanıcı hiç yok, sıfırdan kayıt!
                var newUser = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Role = "User",
                    IsEmailConfirmed = false,
                    ResetCode = verificationCode,
                    ResetCodeExpires = DateTime.Now.AddMinutes(15)
                };
                _context.Users.Add(newUser);
            }

            await _context.SaveChangesAsync();

            // 2. MAİL GÖNDERME KISMI
            try
            {
                string mailBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #e50914;'>CineVerse'e Hoş Geldiniz!</h2>
                        <p>Merhaba {request.Username},</p>
                        <p>Kaydınızı tamamlamak için aşağıdaki 6 haneli doğrulama kodunu kullanın:</p>
                        <h1 style='background-color: #f4f4f4; padding: 10px; text-align: center; letter-spacing: 5px;'>{verificationCode}</h1>
                    </div>";

                await _emailService.SendEmailAsync(request.Email, "CineVerse - E-posta Doğrulama Kodu", mailBody);
                return Ok(new { Message = "Kayıt alındı. Lütfen e-postanıza gelen kodu girin." });
            }
            catch (Exception ex)
            {
                // GELİŞTİRİCİ BYPASS MODU
                Console.WriteLine($"\n\n==========================================");
                Console.WriteLine($"MAİL GÖNDERİLEMEDİ! HATA: {ex.Message}");
                Console.WriteLine($"MANUEL TEST İÇİN DOĞRULAMA KODU: {verificationCode}");
                Console.WriteLine($"==========================================\n\n");

                return Ok(new { Message = "Kayıt alındı. (Geliştirici Modu: Kodu konsoldan alın)" });
            }
        }
        // --- 2. YENİ: MAİL DOĞRULAMA METODU ---
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return BadRequest("Kullanıcı bulunamadı.");

            if (user.ResetCode != request.Code)
                return BadRequest("Girdiğiniz doğrulama kodu hatalı.");

            if (user.ResetCodeExpires < DateTime.Now)
                return BadRequest("Kodun süresi dolmuş. Lütfen yeniden kayıt olmayı deneyin.");

            // Her şey doğruysa hesabı aktif et
            user.IsEmailConfirmed = true;
            user.ResetCode = null;
            user.ResetCodeExpires = null;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "E-posta adresiniz başarıyla doğrulandı! Giriş yapabilirsiniz." });
        }
        // --- KODU TEKRAR GÖNDER ---
        [HttpPost("resend-code")]
        public async Task<IActionResult> ResendCode([FromBody] ForgotPasswordDto request) // Sadece Email alacağımız için bu DTO'yu tekrar kullanabiliriz
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return BadRequest("Kullanıcı bulunamadı.");

            if (user.IsEmailConfirmed)
                return BadRequest("Bu hesap zaten onaylanmış. Lütfen giriş yapın.");

            // 1. Yeni kod üret
            var random = new Random();
            string newVerificationCode = random.Next(100000, 999999).ToString();

            // 2. Veritabanını güncelle
            user.ResetCode = newVerificationCode;
            user.ResetCodeExpires = DateTime.Now.AddMinutes(15);
            await _context.SaveChangesAsync();

            // 3. Maili tekrar gönder
            try
            {
                string mailBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border-radius: 10px;'>
                        <h2 style='color: #e50914;'>CineVerse Yeni Doğrulama Kodu</h2>
                        <p>Merhaba {user.Username},</p>
                        <p>Yeni doğrulama kodunuz talebiniz üzerine aşağıda oluşturulmuştur:</p>
                        <h1 style='background-color: #f4f4f4; padding: 10px; text-align: center; letter-spacing: 5px;'>{newVerificationCode}</h1>
                    </div>";

                await _emailService.SendEmailAsync(request.Email, "CineVerse - Yeni Doğrulama Kodu", mailBody);
                return Ok(new { Message = "Yeni doğrulama kodu e-postanıza gönderildi." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\n=== [YENİDEN GÖNDERME] MAİL HATASI: {ex.Message} ===");
                Console.WriteLine($"YENİ KOD: {newVerificationCode}\n\n");
                return Ok(new { Message = "Yeni kod oluşturuldu. (Geliştirici Modu: Kodu konsoldan alın)" });
            }
        }
        // --- 3. GİRİŞ YAPMA (GÜNCELLENDİ: ONAY KONTROLÜ EKLENDİ) ---
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("Kullanıcı bulunamadı.");

            // 🔥 E-posta onayı kontrolü!
            if (!user.IsEmailConfirmed)
                return BadRequest("Lütfen giriş yapmadan önce e-posta adresinize gönderilen kod ile hesabınızı doğrulayın.");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return BadRequest("Hatalı şifre girdiniz.");

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

        // --- 1. PROFİL BİLGİLERİNİ GETİR ---
        [HttpGet("me")]
        [Authorize] // Sadece giriş yapmış (Token'ı olan) kullanıcılar girebilir
        public async Task<IActionResult> GetProfile()
        {
            // Token'ın içinden kullanıcının ID'sini cımbızla alıyoruz
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(int.Parse(userId));

            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            // Türkçe karakter sorunu olmaması için direkt veritabanından saf ismi yolluyoruz
            return Ok(new { username = user.Username, email = user.Email });
        }

        // --- 2. E-POSTA GÜNCELLE ---
        [HttpPut("update-email")]
        [Authorize]
        public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(int.Parse(userId));

            // Güvenlik: Önce adamın girdiği mevcut şifre doğru mu diye bakıyoruz
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return BadRequest("Mevcut şifreniz hatalı!");

            // Yeni istenen e-posta başka birinde var mı?
            if (await _context.Users.AnyAsync(u => u.Email == request.NewEmail && u.Id != user.Id))
                return BadRequest("Bu e-posta adresi başka bir hesap tarafından kullanılıyor.");

            user.Email = request.NewEmail;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "E-posta adresiniz başarıyla güncellendi." });
        }

        // --- 3. ŞİFRE DEĞİŞTİR ---
        [HttpPut("update-password")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(int.Parse(userId));

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return BadRequest("Mevcut şifreniz hatalı!");

            // Yeni şifreyi BCrypt ile tekrar kriptolayıp kaydediyoruz
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Şifreniz başarıyla güncellendi." });
        }

        // --- 4. HESABI SİL ---
        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(int.Parse(userId));

            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return Ok(new { Message = "Hesabınız ve tüm verileriniz sistemden silindi." });
        }

        // --- 5. ŞİFREMİ UNUTTUM (MAİLE KOD GÖNDER) ---
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            // 1. Veritabanında bu mail var mı?
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return BadRequest("Bu e-posta adresine ait bir kullanıcı bulunamadı.");

            // 2. Rastgele 6 haneli kod üret
            var random = new Random();
            string resetCode = random.Next(100000, 999999).ToString();

            // 3. Kodu ve süresini (15 dakika) veritabanına kaydet
            user.ResetCode = resetCode;
            user.ResetCodeExpires = DateTime.Now.AddMinutes(15);
            await _context.SaveChangesAsync();

            // 4. Kodu mail olarak gönder (GÜVENLİK AĞI İLE)
            try
            {
                string mailBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; background-color: #f4f4f4; border-radius: 10px;'>
                        <h2 style='color: #e50914;'>CineVerse Şifre Sıfırlama</h2>
                        <p>Merhaba {user.Username},</p>
                        <p>Hesabınız için şifre sıfırlama talebinde bulundunuz. Aşağıdaki güvenlik kodunu kullanarak şifrenizi yenileyebilirsiniz:</p>
                        <h1 style='background-color: #fff; padding: 10px; border: 1px solid #ccc; text-align: center; letter-spacing: 5px; color: #333;'>{resetCode}</h1>
                        <p><i>Bu kod 15 dakika boyunca geçerlidir. Eğer bu talebi siz yapmadıysanız lütfen bu e-postayı dikkate almayınız.</i></p>
                    </div>";

                await _emailService.SendEmailAsync(user.Email, "CineVerse - Şifre Sıfırlama Kodu", mailBody);

                return Ok(new { Message = "Şifre sıfırlama kodu e-posta adresinize gönderildi." });
            }
            catch (Exception ex)
            {
                // MAİL GİTMEZSE KONSOLA YAZDIR
                Console.WriteLine($"\n\n==========================================");
                Console.WriteLine($"ŞİFREMİ UNUTTUM MAİLİ GÖNDERİLEMEDİ! HATA: {ex.Message}");
                Console.WriteLine($"ŞİFRE SIFIRLAMA KODU: {resetCode}");
                Console.WriteLine($"==========================================\n\n");

                return Ok(new { Message = "Şifre sıfırlama kodu oluşturuldu. (Lütfen kodu konsoldan alın)" });
            }
        }

        // --- 6. KODU DOĞRULA VE YENİ ŞİFREYİ KAYDET ---
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return BadRequest("Kullanıcı bulunamadı.");

            // 1. Kod doğru mu?
            if (user.ResetCode != request.Code)
                return BadRequest("Girdiğiniz doğrulama kodu hatalı.");

            // 2. Kodun süresi geçmiş mi?
            if (user.ResetCodeExpires < DateTime.Now)
                return BadRequest("Bu kodun süresi dolmuş. Lütfen yeni bir kod isteyin.");

            // 3. Her şey doğruysa yeni şifreyi Hash'le (kriptola) ve kaydet
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // 4. Kullanılmış kodu temizle ki bir daha kullanılmasın (Güvenlik!)
            user.ResetCode = null;
            user.ResetCodeExpires = null;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Şifreniz başarıyla yenilendi! Artık yeni şifrenizle giriş yapabilirsiniz." });
        }
    }
}