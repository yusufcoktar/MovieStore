using DigitalMovieStore.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DigitalMovieStore.Service.Email;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýmýzý sisteme tanýtýyoruz
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Döngü Kýrýcý
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Güvenlik görevlisini (JWT Bearer) iþe alýyoruz
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// --- TEK VE TEMÝZ CORS ÝZNÝ ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // SÝHÝRLÝ SATIR: 5173, 5174 veya baþka port fark etmez, bizim projeye izin ver
        policy.SetIsOriginAllowed(origin => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- MÝMARÝ SIRALAMA (ÇOK ÖNEMLÝ) ---
app.UseRouting();

app.UseCors("AllowFrontend"); // Ýznimizi kapýya astýk

app.UseAuthentication(); // BÜYÜK EKSÝKLÝK BURADAYDI! Önce Kimlik Sor...

app.UseAuthorization();

app.MapControllers();

app.Run();