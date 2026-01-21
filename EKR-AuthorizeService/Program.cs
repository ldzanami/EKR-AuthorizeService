using EKR_AuthorizeService.Api.Services.Auth;
using EKR_AuthorizeService.Data;
using EKR_AuthorizeService.Handlers;
using EKR_AuthorizeService.Repositories.Helpers;
using EKR_AuthorizeService.Repositories.Interfaces.Helpers;
using EKR_AuthorizeService.Repositories.Interfaces.User;
using EKR_AuthorizeService.Repositories.User;
using EKR_AuthorizeService.Services.Auth;
using EKR_AuthorizeService.Services.Encriptoin;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_AuthorizeService.Services.Interfaces.Encription;
using EKR_Shared.Handlers;
using EKR_Shared.Middlewares;
using EKR_Shared.Services.Encryption;
using EKR_Shared.Services.Infrastructure;
using EKR_Shared.Services.Interfaces.Encryption;
using EKR_Shared.Services.Interfaces.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.IO;
using System.Security.Cryptography;

namespace EKR_AuthorizeService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information()
                                                  .WriteTo.Console()
                                                  .WriteTo.File(
                                                                    "logs/app-.log",
                                                                    rollingInterval: RollingInterval.Day,
                                                                    retainedFileCountLimit: 7,
                                                                    fileSizeLimitBytes: 10_000_000,
                                                                    rollOnFileSizeLimit: true
                                                                )
                                                  .CreateLogger();

            try
            {
                Log.Information("Starting web application");
                var builder = WebApplication.CreateBuilder(args);

                builder.Logging.ClearProviders();

                builder.Services.AddSerilog();
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
                builder.Services.AddScoped<IKafkaMessageHandler<string, string>, KafkaMessageHandler>();
                builder.Services.AddScoped<IRSAEncryptorService, RSAEncryptorService>();
                builder.Services.AddScoped<IAESEncryptorService, AESEncryptorService>();

                Log.Information("Generate RSA keys");
                using var rsa = RSA.Create(2048);

                var dir = Path.GetDirectoryName("keys/");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir!);
                }

                var privateKeyPem = rsa.ExportRSAPrivateKeyPem();
                File.WriteAllText("keys/private.pem", privateKeyPem);

                var publicKeyPem = rsa.ExportRSAPublicKeyPem();
                File.WriteAllText("keys/public.pem", publicKeyPem);

                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate();
                }

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}