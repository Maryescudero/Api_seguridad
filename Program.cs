using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Api_seguridad.Models;
using Api_seguridad.Services;
using Api_seguridad.Repositorios;
using Api_seguridad;
using QuestPDF.Infrastructure;

//  Config de licencia QuestPDF
QuestPdfConfig.Initialize();

var builder = WebApplication.CreateBuilder(args);


// MVC (vistas) + API Controllers
builder.Services.AddControllersWithViews();   //  solo esto (no uses AddControllers() aparte)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Escuchar en el puerto 5000 (HTTP). Para DEV alcanza con esto.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

// CORS (abierto en dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                        .AllowAnyMethod()
                                        .AllowAnyHeader());
});

// MySQL (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("Mysql");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connectionString!, ServerVersion.AutoDetect(connectionString)));

// Servicios y repositorios
builder.Services.AddScoped<Auth>();
builder.Services.AddScoped<RepositorioUsuario>();
builder.Services.AddScoped<RepositorioGuardia>();
builder.Services.AddScoped<RepositorioServicio>();
builder.Services.AddScoped<RepositorioAsignacionServicio>();
builder.Services.AddScoped<ServicioAsignacionAutomatica>();
builder.Services.AddScoped<RepositorioHistorialUsuario>();
builder.Services.AddScoped<RepositorioFrancoGuardia>();
builder.Services.AddScoped<RepositorioTurno>();
builder.Services.AddScoped<RepositorioTurnoServicio>();
builder.Services.AddScoped<QrTokenService>();
builder.Services.AddScoped<QrCodeService>();
builder.Services.AddScoped<RepositorioReporte>();


// 👇 agregado
builder.Services.AddScoped<RepositorioNotificacion>(); //  agregado
builder.Services.AddSignalR();                         //  agregado

// JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (!string.IsNullOrEmpty(jwtKey) && !string.IsNullOrEmpty(jwtIssuer))
{
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.RequireHttpsMetadata = false; //  en producción poner en true y sirve HTTPS
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,           // en prod  pasar a true
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", p => p.RequireClaim("rol", "administrador"));
    options.AddPolicy("AdminOGuardia", p => p.RequireClaim("rol", "administrador", "guardia"));
});

var app = builder.Build();

// Swagger solo en Dev (para probar endpoints)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseCors("AllowAll");

//sirviendo solo HTTP:5000. Para evitar redirecciones a HTTPS sin puerto, desactiva esto por ahora:
//// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// API controllers por atributo
app.MapControllers();

// 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 👇 agregado
app.MapHub<Api_seguridad.Hubs.NotificacionesHub>("/notificacionesHub"); // 👈 agregado

app.Run();









/*using System.Text;  /// este de aca abajo funciona perfecto pero no incluye notificacion OJO SI FALLA EL DE ARRIBA!!!
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Api_seguridad.Models;
using Api_seguridad.Services;
using Api_seguridad.Repositorios;


var builder = WebApplication.CreateBuilder(args);

// MVC (vistas) + API Controllers
builder.Services.AddControllersWithViews();   //  solo esto (no uses AddControllers() aparte)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Escuchar en el puerto 5000 (HTTP). Para DEV alcanza con esto.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

// CORS (abierto en dev)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin()
                                        .AllowAnyMethod()
                                        .AllowAnyHeader());
});

// MySQL (Pomelo)
var connectionString = builder.Configuration.GetConnectionString("Mysql");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connectionString!, ServerVersion.AutoDetect(connectionString)));

// Servicios y repositorios
builder.Services.AddScoped<Auth>();
builder.Services.AddScoped<RepositorioUsuario>();
builder.Services.AddScoped<RepositorioGuardia>();
builder.Services.AddScoped<RepositorioServicio>();
builder.Services.AddScoped<RepositorioAsignacionServicio>();
builder.Services.AddScoped<ServicioAsignacionAutomatica>();
builder.Services.AddScoped<RepositorioHistorialUsuario>();
builder.Services.AddScoped<RepositorioFrancoGuardia>();
builder.Services.AddScoped<RepositorioTurno>();
builder.Services.AddScoped<RepositorioTurnoServicio>();
builder.Services.AddScoped<QrTokenService>();
builder.Services.AddScoped<QrCodeService>();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (!string.IsNullOrEmpty(jwtKey) && !string.IsNullOrEmpty(jwtIssuer))
{
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer("Bearer", options =>
        {
            options.RequireHttpsMetadata = false; //  en producción poner en true y sirve HTTPS
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = false,           // en prod podés pasar a true
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            };
        });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", p => p.RequireClaim("rol", "administrador"));
    options.AddPolicy("AdminOGuardia", p => p.RequireClaim("rol", "administrador", "guardia"));
});

var app = builder.Build();

// Swagger solo en Dev (útil para probar endpoints)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseCors("AllowAll");

//sirviendo solo HTTP:5000. Para evitar redirecciones a HTTPS sin puerto, desactiva esto por ahora:
//// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// API controllers por atributo
app.MapControllers();

// 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();*/
