using Application.Activities.Queries;
using Application.Core;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Adding controllers
builder.Services.AddControllers();

//Adding database
builder.Services.AddDbContext<AppDbContext>(opt => 
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Adding CORS certificate for HTTPS
builder.Services.AddCors();

//Adding mediator for communication between API and Application project
builder.Services.AddMediatR(
    x=>x.RegisterServicesFromAssemblyContaining<GetActivityList.Handler>());

//Adding the automapper
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseCors(x=>x.AllowAnyHeader().AllowAnyMethod()
    .WithOrigins("http://localhost:3000","https://localhost:3000"));
app.MapControllers();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedData(context);
}
catch (System.Exception)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError("An error has occurred during migration.");
    throw;
}

app.Run();
