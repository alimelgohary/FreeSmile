using FreeSmile;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FreeSmile.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FreeSmileContext>(optionsBuilder =>
{
    optionsBuilder.UseSqlServer(Helper.GetEnvVariable("FreeSmileDatabase", true));
});


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




new FreeSmileContext().CaseTypes.ToList().ForEach(x => Console.WriteLine(x.NameEn + x.NameAr));
new FreeSmileContext().ArticleCats.ToList().ForEach(x => Console.WriteLine(x.NameEn + x.NameAr));
new FreeSmileContext().ProductCats.ToList().ForEach(x => Console.WriteLine(x.NameEn + x.NameAr));
app.Run();
