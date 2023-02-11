using FreeSmile;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FreeSmile.Services;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region Logging
builder.Services.AddLogging();
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(config));
#endregion

builder.Services.AddControllers();

#region Localization1
builder.Services.AddLocalization();
builder.Services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
builder.Services.AddMvc()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(JsonStringLocalizerFactory));
    });
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };

    options.DefaultRequestCulture = new RequestCulture(supportedCultures[0]);
    options.SupportedCultures = supportedCultures;
});
#endregion

#region TransientServices
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IDentistService, DentistService>();
builder.Services.AddTransient<IPatientService, PatientService>();
#endregion

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FreeSmileContext>(optionsBuilder =>
    optionsBuilder.UseSqlServer(Helper.GetEnvVariable("FreeSmileDatabase", true))
);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region Localization2
var supportedCultures = new[] { "en", "ar" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures);
app.UseRequestLocalization(localizationOptions); 
#endregion

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
