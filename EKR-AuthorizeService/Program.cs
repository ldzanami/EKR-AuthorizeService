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
using EKR_Shared.Handlers.Interfaces;
using EKR_Shared.Services.Encryption;
using EKR_Shared.Services.Helpers;
using EKR_Shared.Services.Infrastructure;
using EKR_Shared.Services.Interfaces.Encryption;
using EKR_Shared.Services.Interfaces.Helpers;
using EKR_Shared.Services.Interfaces.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

                builder.Configuration["ConnectionString:Default"] = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? builder.Configuration["ConnectionString:Default"];
                builder.Configuration["Kafka:Address"] = Environment.GetEnvironmentVariable("KAFKA_ADDRESS") ?? builder.Configuration["Kafka:Address"];
                builder.Configuration["Kafka:GroupId"] = Environment.GetEnvironmentVariable("KAFKA_GROUP_ID") ?? builder.Configuration["Kafka:GroupId"];
                builder.Configuration["Kafka:ConsumerTopicName"] = Environment.GetEnvironmentVariable("KAFKA_CONSUMER_TOPIC_NAME") ?? builder.Configuration["Kafka:ConsumerTopicName"];
                builder.Configuration["Kafka:ProducerTopicName"] = Environment.GetEnvironmentVariable("KAFKA_PRODUCER_TOPIC_NAME") ?? builder.Configuration["Kafka:ProducerTopicName"];
                builder.Configuration["Kafka:Timeout"] = Environment.GetEnvironmentVariable("KAFKA_TIMEOUT") ?? builder.Configuration["Kafka:Timeout"];
                builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];
                builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"];
                builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY") ?? builder.Configuration["Jwt:Key"];
                builder.Configuration["Jwt:AccessTokenLifetimeMinutes"] = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_LIFETIME") ?? builder.Configuration["Jwt:AccessTokenLifetimeMinutes"];
                builder.Configuration["Jwt:RefreshTokenLifetimeDays"] = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_LIFETIME") ?? builder.Configuration["Jwt:RefreshTokenLifetimeDays"];
                builder.Configuration["AllowedHosts"] = Environment.GetEnvironmentVariable("ALLOWED_HOSTS") ?? builder.Configuration["AllowedHosts"];
                builder.Configuration["SelfId"] = Environment.GetEnvironmentVariable("SELF_ID") ?? builder.Configuration["SelfId"];
                builder.Configuration["RotationTime"] = Environment.GetEnvironmentVariable("ROTATION_TIME") ?? builder.Configuration["RotationTime"];



                builder.Services.AddSerilog();
                builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration["ConnectionString:Default"]));
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
                builder.Services.AddScoped<IPostCommandHandler, AuthHandler>();
                builder.Services.AddScoped<IPostCommandHandler, GetActiveSessionsHandler>();
                builder.Services.AddScoped<IPostCommandHandler, RefreshHandler>();
                builder.Services.AddScoped<IPostCommandHandler, RegisterHandler>();
                builder.Services.AddScoped<IPostCommandHandler, RevokeAllHandler>();
                builder.Services.AddScoped<IPostCommandHandler, RevokeHandler>();
                builder.Services.AddScoped<IPostCommandHandler, RevokeOthersHandler>();
                builder.Services.AddScoped<IPostCommandHandler, KeysRotationHandler>();
                builder.Services.AddScoped<IGetCommandHandler, GetPublicKeyHandler>();
                builder.Services.AddScoped<IHashCheckingService, HashCheckingService>();
                builder.Services.AddScoped<IKeysRotationService, KeysRotationService>();


                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var rotation = scope.ServiceProvider.GetRequiredService<IKeysRotationService>();
                    rotation.DefaultRotation();
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