using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<FreeSmileContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// TODO : Test passwords exist in env vars in production env
// Console.WriteLine(System.Environment.GetEnvironmentVariable("SomeSecret"));


// Inject Secrets

new FreeSmile.Models.FreeSmileContext().CaseTypes.ToList().ForEach(x => Console.WriteLine(x.NameEn + x.NameAr));
new FreeSmile.Models.FreeSmileContext().ArticleCats.ToList().ForEach(x => Console.WriteLine(x.NameEn + x.NameAr));
new FreeSmile.Models.FreeSmileContext().ProductCats.ToList().ForEach(x => Console.WriteLine(x.NameEn + x.NameAr));
app.Run();
