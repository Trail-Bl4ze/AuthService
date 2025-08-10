using Microsoft.EntityFrameworkCore;
using Auth.Core.Interfaces;
using Auth.Core.Models;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Services;

public class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Конфигурация сервисов
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddControllers();

        // Настройка базы данных
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration["Database:ConnectionStrings:Default"])
                   .UseSnakeCaseNamingConvention());

        // Настройка аутентификации
        var authenticationSettingsSection = configuration.GetSection("AuthenticationSettings");
        services.Configure<AuthenticationSettings>(authenticationSettingsSection);

        // Регистрация репозиториев и сервисов
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
        }

        app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());


        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}