using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Handlers.Interfaces;
using System.Text.Json;
using EKR_Shared.Data;

namespace EKR_AuthorizeService.Handlers
{
    public class AuthHandler(IAuthService authService) : IPostCommandHandler
    {
        public string CommandType => AuthCommands.Authorize;

        private readonly IAuthService _authService = authService;

        public async Task<object?> HandleAsync(
            string decryptedContent,
            string requestId,
            CancellationToken ct)
        {
            var dto = JsonSerializer.Deserialize<AuthorizationDto>(decryptedContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return await _authService.AuthorizationAsync(dto!, requestId);
        }
    }
}
