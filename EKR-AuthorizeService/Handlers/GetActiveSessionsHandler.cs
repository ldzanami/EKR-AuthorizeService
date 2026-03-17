using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared.Data;
using EKR_Shared.Handlers.Interfaces;
using EKR_Shared.Auxiliary;

namespace EKR_AuthorizeService.Handlers
{
    public class GetActiveSessionsHandler(IAuthService authService) : IPostCommandHandler
    {
        public string CommandType => AuthCommands.GetActive;

        private readonly IAuthService _authService = authService;

        public async Task<object?> HandleAsync(
            string decryptedContent,
            string requestId,
            AESEncryptPack AESPack,
            CancellationToken ct)
        {
            return await _authService.GetActiveUserSessionsAsync(Guid.Parse(decryptedContent), AESPack, requestId);
        }
    }
}
