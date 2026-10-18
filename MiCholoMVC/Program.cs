using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiCholoMVC.Data;
using MiCholoMVC.Repositories;
using MiCholoMVC.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<OperacionService>();
builder.Services.AddSingleton<AccesoService>();

builder.Services.AddDbContext<MiCholoDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MiCholoConnection")));

builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();

builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IProveedorService, ProveedorService>();

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

builder.Services.AddScoped<ILoteRepository, LoteRepository>();
builder.Services.AddScoped<ILoteService, LoteService>();

builder.Services.AddScoped<IMovimientoInventarioRepository, MovimientoInventarioRepository>();
builder.Services.AddScoped<IMovimientoInventarioService, MovimientoInventarioService>();

builder.Services.AddScoped<IPerdidaRepository, PerdidaRepository>();
builder.Services.AddScoped<IPerdidaService, PerdidaService>();

builder.Services.AddScoped<ICajaRepository, CajaRepository>();
builder.Services.AddScoped<ICajaService, CajaService>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnValidatePrincipal = async context =>
        {
            var acceso = context.HttpContext.RequestServices.GetRequiredService<AccesoService>();
            if (context.Principal?.FindFirst("VersionCredencial")?.Value != acceso.VersionCredencial())
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
