using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RHAds.Data;
using RHAds.Models.Usuarios;
using RHAds.Services; // Aquí estará tu AuthService

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Servicios de autenticación
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

// Autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";       // ruta al login
        options.AccessDeniedPath = "/Auth/Denied"; // ruta si no tiene permisos
    });

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // NECESARIO para Bootstrap, JS, imágenes, etc.

app.UseRouting();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Ruta por defecto → Panel Admin (Menu)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Areas}/{id?}"
);

app.Run();