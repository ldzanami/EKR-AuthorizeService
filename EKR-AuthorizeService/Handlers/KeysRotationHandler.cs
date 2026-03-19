using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_AuthorizeService.Services.Interfaces.Encription;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Auxiliary;
using EKR_Shared.Data;
using EKR_Shared.Handlers.Interfaces;
using System.Text.Json;

namespace EKR_AuthorizeService.Handlers
{
    public class KeysRotationHandler(IKeysRotationService keysRotationService) : IPostCommandHandler
    {
        public string CommandType => AuthCommands.KeysRotation;

        private readonly IKeysRotationService _keysRotationService = keysRotationService;

        public async Task<object?> HandleAsync(
            string decryptedContent,
            string requestId,
            AESEncryptPack AESPack,
            CancellationToken ct)
        {
            return await _keysRotationService.ComprometatedRotationAsync(decryptedContent.Trim("\"").ToString());
        }
    }
}
