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
            string pem = File.ReadAllText("keys/public.pem");

            string base64 = pem
                .Replace("-----BEGIN PUBLIC KEY-----", "")
                .Replace("-----END PUBLIC KEY-----", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim();
            byte[] der = Convert.FromBase64String(base64);

            return new { Key = der, RequestId = requestId };
        }
    }
}
