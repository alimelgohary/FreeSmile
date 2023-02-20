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
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(MyConstants.JWT_SECRET)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
    options.Events = new JwtBearerEvents() {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("Authorization-Token"))
            {
                context.Token = context.Request.Cookies["Authorization-Token"];
            }
            return Task.CompletedTask;
        }
};  
    //options.Events.OnMessageReceived = context =>
    //{
    //    if (context.Request.Cookies.ContainsKey("Authorization-Token"))
    //    {
    //        context.Token = context.Request.Cookies["Authorization-Token"];
    //    }
    //    return Task.CompletedTask;
    //};
});
#endregion

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FreeSmileContext>(optionsBuilder =>
    optionsBuilder.UseSqlServer(MyConstants.FREESMILE_CONNECTION)
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();
