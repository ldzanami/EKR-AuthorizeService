using Confluent.Kafka;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Data;
using EKR_Shared.Handlers.Interfaces;
using System.Text;
using System.Text.Json;

namespace EKR_AuthorizeService.Handlers
{
    public class RefreshHandler(IAuthService authService) : IPostCommandHandler
    {
        public string CommandType => AuthCommands.Refresh;

        private readonly IAuthService _authService = authService;

        public async Task<object?> HandleAsync(
            string decryptedContent,
            string requestId,
            CancellationToken ct)
        {
            return await _authService.RefreshAsync(decryptedContent, requestId);
        }
    }
}
