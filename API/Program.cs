using API.Filters;
using API.Models;
using BLL.Interfaces;
using BLL.Services;
using DAL.Context;
using DAL.UnitOfWork;
using Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BLL.Mapping;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext та UnitOfWork
builder.Services.AddDbContext<DigitalLibraryContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 2. AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

// 3. BLL-сервіси
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// 4. JWT-аутентифікація
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                          Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// 5. Controllers only (Web API)
builder.Services.AddControllers(options =>
{
    // глобальний фільтр обробки виключень
    options.Filters.Add<ExceptionFilter>();
});

// 6. Swagger для API-документації
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// порядок важливий!
app.UseAuthentication();
app.UseAuthorization();

// мапимо тільки API-контролери
app.MapControllers();

app.Run();
