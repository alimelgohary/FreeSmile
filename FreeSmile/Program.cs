using FreeSmile;
using FreeSmile.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FreeSmile.Services;
using Microsoft.Extensions.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Serilog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FreeSmile.ActionFilters;

var builder = WebApplication.CreateBuilder(args);
var corsPolicy = "_myAllowSpecificOrigins";
#region Logging
builder.Services.AddLogging();
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(config));
#endregion

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy,
                      policy =>
                      {
                          policy.WithOrigins("http://freesmile.me",
                                             "https://freesmile.me",
                                             "http://www.freesmile.me",
                                             "https://www.freesmile.me",
                                             "http://127.0.0.1:5500")
                                .AllowCredentials()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

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
builder.Services.AddTransient<IAdminService, AdminService>();
#endregion

#region Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = AuthHelper.tokenValidationParameters;
    options.Events = new JwtBearerEvents()
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey(MyConstants.AUTH_COOKIE_KEY))
            {
                context.Token = context.Request.Cookies[MyConstants.AUTH_COOKIE_KEY];
            }
            return Task.CompletedTask;
        }
    };
});
#endregion

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FreeSmileContext>(optionsBuilder =>
    optionsBuilder.UseSqlServer(MyConstants.FREESMILE_CONNECTION)
);

builder.Services.AddScoped<ValidUser>();
builder.Services.AddScoped<NotSuspended>();
builder.Services.AddScoped<VerifiedEmail>();
builder.Services.AddScoped<VerifiedIfDentist>();


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
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
