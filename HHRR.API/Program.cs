using System.Text;
using System.Text.Json.Serialization;
using HHRR.Application;
using HHRR.Infrastructure;
using HHRR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // <--- ESTE ES LA CLAVE PARA SWAGGER

// 1. CARGAR .ENV LOCAL
DotNetEnv.Env.Load(); 

var builder = WebApplication.CreateBuilder(args);

// 2. SERVICIOS + FIX JSON CYCLES
builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

// 3. CONFIGURACIÓN DE SWAGGER CON SOPORTE JWT (El Candado)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TalentoPlus API", Version = "v1" });

    // Definir el esquema de seguridad (Bearer Token)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1Ni...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Aplicar la seguridad a todos los endpoints
    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// 4. INFRAESTRUCTURA Y APLICACIÓN
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// 5. IDENTITY
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 6. JWT CONFIG
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "Secret_Key_Default_123456";
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "HHRR_API";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "HHRR_Users";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

var app = builder.Build();

// 7. PIPELINE (ACTIVAR SWAGGER SIEMPRE PARA PRUEBAS)
// Quitamos el "if (IsDevelopment)" para que siempre salga en la demo
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TalentoPlus API v1");
    // Esto hace que Swagger sea la página de inicio al abrir localhost:5070
    c.RoutePrefix = string.Empty; 
});

// Data Seeding
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAsync(scope.ServiceProvider, context); 
}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers(); 

app.Run();