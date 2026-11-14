using Arch.EntityFrameworkCore.UnitOfWork;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyToDo.Api.Context;
using MyToDo.Api.Context.Repository;
using MyToDo.Api.Extensions;
using MyToDo.Api.Service;
using MyToDo.Api.Service.implementation;
using System.Text;

namespace MyToDo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("ToDoConnection");

            builder.Services.AddDbContext<MyToDoContext>(option =>
            {
                option.UseSqlite(connectionString);
            }).AddUnitOfWork<MyToDoContext>().
            AddCustomRepository<ToDo, ToDoRepository>().
            AddCustomRepository<Memo, MemoRepository>().
            AddCustomRepository<User, UserRepository>();

            //注册认证服务

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
                opt.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT校验失败: {context.Exception.Message}");
                        return Task.CompletedTask;
                    }
                };
            });
            builder.Services.AddAuthorization();

            //注册所有service

            builder.Services.Scan(s => s.FromAssemblyOf<ToDoService>()
                .AddClasses(c => c.Where(t => t.Name.EndsWith("Service")))
                .AsSelfWithInterfaces() //
                .WithTransientLifetime());

            builder.Services.AddAutoMapper(cfg => { }, typeof(AutoMapperConfigProfile).Assembly);

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(g =>
            {
                //var xmlPath = Path.Combine(AppContext.BaseDirectory + "MyToDo.Api.xml");
                var xmlPath = Path.Combine(AppContext.BaseDirectory, "MyToDo.Api.xml");
                if (File.Exists(xmlPath))
                    g.IncludeXmlComments(xmlPath, true);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
            }
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}