using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared.Auxiliary;
using EKR_Shared.Data;
using EKR_Shared.Handlers.Interfaces;

namespace EKR_AuthorizeService.Handlers
{
    public class RevokeAllHandler(IAuthService authService) : IPostCommandHandler
    {
        public string CommandType => AuthCommands.RevokeAll;

        private readonly IAuthService _authService = authService;

        public async Task<object?> HandleAsync(
            string decryptedContent,
            string requestId,
            AESEncryptPack AESPack,
            CancellationToken ct)
        {
            await _authService.RevokeAllSessionsAsync(Guid.Parse(decryptedContent), requestId);
            return new { success = true };
        }
    }
}
