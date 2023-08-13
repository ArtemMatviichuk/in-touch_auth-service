using AuthService.Common.Constants;
using AuthService.Data;
using AuthService.Services.Interfaces;
using AuthService.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using AuthService.Data.Repositories.Interfaces;
using AuthService.Data.Repositories.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthService.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using AuthService.Security;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var securityOptions = new SecurityOptions();
builder.Configuration.Bind(nameof(SecurityOptions), securityOptions);
builder.Services.AddSingleton(securityOptions);

builder.Services.AddRsaKeys(securityOptions);

builder.Services.AddDbContext<AuthContext>(opt => opt.UseSqlServer(
    builder.Configuration.GetConnectionString(AppConstants.ConnectionStringName)
));

// REPOSITORIES
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IRoleRepository, RoleRepository>();

// SERVICES
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
   .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(cfg =>
    {
        var rsa = RSA.Create();
        var key = File.ReadAllText(securityOptions.PublicKeyFilePath);
        rsa.FromXmlString(key);

        cfg.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "in-touch",
            ValidAudience = "in-touch",

            IssuerSigningKey = new RsaSecurityKey(rsa),
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(a => a.Run(async context =>
{
    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
    var exception = exceptionHandlerPathFeature?.Error;

    if (exception is CustomException)
    {
        context.Response.StatusCode = (exception as CustomException)!.StatusCode;
    }

    await context.Response.WriteAsJsonAsync(new { error = exception!.Message });
}));
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await DbPreparator.PrepareDb(app, app.Configuration);

app.Run();
