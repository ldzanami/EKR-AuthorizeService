using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_Shared.Data;
using EKR_Shared.Handlers.Interfaces;
using System.Text.RegularExpressions;

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

            string base64 = Regex.Replace(pem,
                            "-+BEGIN PUBLIC KEY-+|-+END PUBLIC KEY-+|\\s+",
                            "");

            return new { Key = base64, RequestId = requestId };
        }
    }
}
