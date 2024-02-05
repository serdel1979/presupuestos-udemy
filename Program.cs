using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;
using WebApplication1.Servicios;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IRepositorioTiposCuentas, RepositorioTiposCuentas>();

builder.Services.AddTransient<IRepositorioCuentas, RepositorioCuentas>();

builder.Services.AddTransient<IRepositorioCategorias, RepositorioCategorias>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();

builder.Services.AddTransient<IRepositorioTransacciones, RepositorioTransacciones>();

builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();

builder.Services.AddTransient<IServicioReporte, ServicioReporte>();

builder.Services.AddTransient<SignInManager<Usuario>>();

builder.Services.AddTransient<IUserStore<Usuario>, UsuarioStore>();

builder.Services.AddIdentityCore<Usuario>(opciones =>
{
    opciones.Password.RequireDigit = false;
    opciones.Password.RequireLowercase = false;
    opciones.Password.RequireUppercase = false;
    opciones.Password.RequireNonAlphanumeric = false;
}).AddErrorDescriber<MsgErroresIdentity>();

//configura servicio de cooki
builder.Services.AddAuthentication(op =>
{
    op.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    op.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    op.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
}).AddCookie(IdentityConstants.ApplicationScheme,
op =>
{
    op.LoginPath = "/usuarios/login";
});







builder.Services.AddAutoMapper(typeof(Program));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transacciones}/{action=Index}/{id?}");

app.Run();
