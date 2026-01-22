using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared.Data;
using EKR_Shared.Handlers.Interfaces;

namespace EKR_AuthorizeService.Handlers
{
    public class GetActiveSessionsHandler(IAuthService authService) : ICommandHandler
    {
        public string CommandType => AuthCommands.GetActive;

        private readonly IAuthService _authService = authService;

        public async Task<object?> HandleAsync(
            byte[] decryptedContent,
            string requestId,
            CancellationToken ct)
        {
            return await _authService.GetActiveUserSessionsAsync(Guid.Parse(decryptedContent), requestId);
        }
    }
}
