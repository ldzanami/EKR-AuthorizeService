using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Data;
using EKR_Shared.Handlers.Interfaces;
using System.Text.Json;

namespace EKR_AuthorizeService.Handlers
{
    public class RevokeOthersHandler(IAuthService authService) : IPostCommandHandler
    {
        public string CommandType => AuthCommands.RevokeOthers;

        private readonly IAuthService _authService = authService;

        public async Task<object?> HandleAsync(
            byte[] decryptedContent,
            string requestId,
            CancellationToken ct)
        {
            var dto = JsonSerializer.Deserialize<RevokeOtherSessionsDto>(decryptedContent);
            await _authService.RevokeOtherSessionsAsync(dto!, requestId);
            return new { success = true };
        }
    }
}
