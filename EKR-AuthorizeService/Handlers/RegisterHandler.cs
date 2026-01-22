using Confluent.Kafka;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Data;
using EKR_Shared.Handlers.Interfaces;
using EKR_Shared.Services.Interfaces.Encryption;
using EKR_Shared.Services.Interfaces.Infrastructure;
using System.Text.Json;

namespace EKR_AuthorizeService.Handlers
{
    public class RegisterHandler(IAuthService authService) : ICommandHandler
    {
        public string CommandType => AuthCommands.Register;

        private readonly IAuthService _authService = authService;

        public async Task<object?> HandleAsync(
            byte[] decryptedContent,
            string requestId,
            CancellationToken ct)
        {
            var dto = JsonSerializer.Deserialize<RegisterRequestDto>(decryptedContent)!;
            return await _authService.RegisterAsync(dto, requestId);
        }
    }
}
