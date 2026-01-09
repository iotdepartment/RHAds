using Microsoft.EntityFrameworkCore;
using RHAds.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

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
app.UseAuthorization();

// Ruta por defecto → Panel Admin (Menu)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Menu}/{id?}"
);

app.Run();