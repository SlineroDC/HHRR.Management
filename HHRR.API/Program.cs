using HHRR.Infrastructure;
using HHRR.Application;
using HHRR.Infrastructure.Persistence;
var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de servicios
builder.Services.AddControllers(); // Usamos Controladores clásicos
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();  // Swagger clásico

// 2. CONEXIÓN A LA BASE DE DATOS (Lo que definimos en Infrastructure)
// Busca la conexión en appsettings.json y configura el DbContext
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

// 3. Configuración del Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// data seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await DbInitializer.SeedAsync(context); 
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers(); 

app.Run();