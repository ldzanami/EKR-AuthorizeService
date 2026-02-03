using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared.Data;
using EKR_Shared.Handlers.Interfaces;

namespace EKR_AuthorizeService.Handlers
{
    public class GetPublicKeyHandler : IGetCommandHandler
    {
        public string CommandType => AuthCommands.GetPublicKey;

        public async Task<object?> HandleAsync(
            string requestId,
            CancellationToken ct)
        {
            return new { Key = File.ReadAllText("keys/public.pem"), RequestId = requestId };
        }
    }
}
