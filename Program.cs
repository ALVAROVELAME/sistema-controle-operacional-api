using Microsoft.EntityFrameworkCore;
using SistemaControleOperacionalApi.Data;
using SistemaControleOperacionalApi.Models;


var builder = WebApplication.CreateBuilder(args);


var connectionString =
$"Server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
$"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
$"Database={Environment.GetEnvironmentVariable("DB_DATABASE")};" +
$"User={Environment.GetEnvironmentVariable("DB_USER")};" +
$"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};";


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    );
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


// cria banco/tabelas automaticamente
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.Database.EnsureCreated();
}


if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        status = "API Sistema de Controle Operacional funcionando"
    });
});



app.MapPost("/clientes", async (
    Cliente cliente,
    AppDbContext db
) =>
{
    db.Clientes.Add(cliente);

    await db.SaveChangesAsync();

    return Results.Ok(cliente);
});



app.MapGet("/clientes", async (
    AppDbContext db
) =>
{
    return await db.Clientes.ToListAsync();
});



var porta = Environment.GetEnvironmentVariable("PORT") ?? "8080";


app.Run($"http://0.0.0.0:{porta}");