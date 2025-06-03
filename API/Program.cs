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
using BLL.Factory;
using API.DependencyInjection;
using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// 1. ��������� JWT-������������ �������.

// 2. ����������� Autofac (�� ���� �����)�
builder.Host
    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule(new DependencyModule(configuration));
    });

// 3. ������ ������ ASP.NET Core (Controllers, CORS, Swagger ����)�
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. ������ Cookie-�������������� ������ JWT:
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // ���� �� ������� ����� (���� ������� ������������):
        options.LoginPath = "/api/auth/login";
        // ���� ������ ������ ������� ������� 403:
        options.AccessDeniedPath = "/api/auth/denied";
        // ����� ����������� ��� 䳿 ���:
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// 5. �������� AutoMapper (�� ���� �����):
builder.Services.AddAutoMapper(typeof(API.Mapping.AutoMapperProfiles).Assembly);
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);

builder.Services.AddAutoMapper(
    typeof(BLL.Mapping.AutoMapperProfiles).Assembly,
    typeof(API.Mapping.AutoMapperProfiles).Assembly
);

var app = builder.Build();

// 6. Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapGet("/", () => Results.Redirect("/swagger/index.html", false));
}

app.UseCors("AllowAll");

// ������ ��������������/�����������
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();