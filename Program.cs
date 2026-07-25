var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.MapGet("/", () =>
{
    return Results.Ok(new
    {
        status = "API Sistema de Controle Operacional funcionando , testando github actions",
    });
});


var porta = Environment.GetEnvironmentVariable("PORT") ?? "8080";


app.Run($"http://0.0.0.0:{porta}");