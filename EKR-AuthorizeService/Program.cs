using EKR_AuthorizeService.Api.Services.Auth;
using EKR_AuthorizeService.Data;
using EKR_AuthorizeService.Middlewares;
using EKR_AuthorizeService.Repositories.Helpers;
using EKR_AuthorizeService.Repositories.Interfaces.Helpers;
using EKR_AuthorizeService.Repositories.Interfaces.User;
using EKR_AuthorizeService.Repositories.User;
using EKR_AuthorizeService.Services.Auth;
using EKR_AuthorizeService.Services.Encriptoin;
using EKR_AuthorizeService.Services.Infrastructure;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_AuthorizeService.Services.Interfaces.Encription;
using EKR_AuthorizeService.Services.Interfaces.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace EKR_AuthorizeService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                 .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                                 .AddEnvironmentVariables();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
            builder.Services.AddControllers();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IJWTGeneratorService, JWTGeneratorService>();
            builder.Services.AddScoped<ISessionService, SessionService>();
            builder.Services.AddScoped<ISessionRepository, SessionRepository>();
            builder.Services.AddScoped<IGeneratorService, GeneratorService>();
            builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
            builder.Services.AddScoped<ICheckExistRepository, CheckExistRepository>();
            builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService>();
            builder.Services.AddHostedService<KafkaConsumerService>();

            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapControllers();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.Run();
        }
    }
}