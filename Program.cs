using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AuthService.AppSettingsOptions;
using AuthService.Common.Constants;
using AuthService.Common.Exceptions;
using AuthService.Data;
using AuthService.Data.Repositories.Interfaces;
using AuthService.Data.Repositories.Implementations;
using AuthService.Services.Interfaces;
using AuthService.Services.Implementations;
using AuthService.Security;
using AuthService.SyncDataServices.Grpc;
using AuthService.AsyncDataServices.Interfaces;
using AuthService.AsyncDataServices.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var securityOptions = new SecurityOptions();
builder.Configuration.Bind(nameof(SecurityOptions), securityOptions);
builder.Services.AddSingleton(securityOptions);

var rabbitMQOptions = new RabbitMQOptions();
builder.Configuration.Bind(nameof(RabbitMQOptions), rabbitMQOptions);
builder.Services.AddSingleton(rabbitMQOptions);

builder.Services.AddDbContext<AuthContext>(opt => opt.UseSqlServer(
    builder.Configuration.GetConnectionString(AppConstants.ConnectionStringName)
));

// REPOSITORIES
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IRoleRepository, RoleRepository>();
builder.Services.AddTransient<IEmailVerificationRepository, EmailVerificationRepository>();

// SERVICES
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

builder.Services.AddScoped<IAuthMessageBusClient, AuthMessageBusClient>();
builder.Services.AddScoped<IEmailMessageBusClient, EmailMessageBusClient>();
builder.Services.AddTransient<IMessagePublisher, MessagePublisher>();

builder.Services.AddGrpc();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Scheme = "Bearer",

    });

    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme,
                }
            },
            new string[]{}
        }
    });
});

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

            ValidIssuer = securityOptions.Issuer,
            ValidAudience = securityOptions.Audience,

            IssuerSigningKey = new RsaSecurityKey(rsa),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    //builder.Services.AddRsaKeys(securityOptions);
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

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcUserService>();
app.MapGet("/protos/users.proto", async context =>
    await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto")));

await DbPreparator.PrepareDb(app, app.Configuration);

app.Run();
