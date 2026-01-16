using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace AuthorizeService.Api.Controllers
{
    /// <summary>
    /// Контроллер, отвечающий за регистрацию и авторизацию пользователя в системе.
    /// </summary>
    /// <param name="authService">Сервис авторизации.</param>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        /// <summary>
        /// Post запрос для регистрации пользователя.
        /// </summary>
        /// <param name="dto">Dto с данными для регистрации.</param>
        /// <returns>200 UserResponseDto.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            return Ok(await _authService.RegisterAsync(dto));
        }

        /// <summary>
        /// Post запрос для авторизации пользователя.
        /// </summary>
        /// <param name="dto">Dto с данными для авторизации.</param>
        /// <returns>200 AuthResponseDto.</returns>
        [HttpPost("authorization")]
        public async Task<IActionResult> Authorization([FromBody] AuthorizationDto dto)
        {
            return Ok(await _authService.AuthorizationAsync(dto, dto.DeviceInfo));
        }

        /// <summary>
        /// Post запрос на обновление токенов.
        /// </summary>
        /// <param name="incomingRefreshToken">Refresh токен пользователя.</param>
        /// <returns>200 Access token + Refresh token + sessionId.</returns>
        [Authorize]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(string incomingRefreshToken)
        {
            return Ok(await _authService.RefreshAsync(incomingRefreshToken));
        }

        /// <summary>
        /// Post запрос на разлогин конкретной сессии.
        /// </summary>
        /// <param name="sessionId">Id сессии, которую надо прервать.</param>
        /// <returns>204 No Content.</returns>
        [Authorize]
        [HttpPost("revokeSession")]
        public async Task<IActionResult> RevokeSession(Guid sessionId)
        {
            await _authService.RevokeSessionAsync(sessionId);
            return NoContent();
        }

        /// <summary>
        /// Post запрос на разлогин всех сессий пользователя, кроме указанной.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="keepSessionId">Id сессии, которую нужно оставить.</param>
        /// <returns>204 No Content.</returns>
        [Authorize]
        [HttpPost("revokeOtherSessions")]
        public async Task<IActionResult> RevokeOtherSessions(Guid userId, Guid keepSessionId)
        {
            await _authService.RevokeOtherSessionsAsync(userId, keepSessionId);
            return NoContent();
        }

        /// <summary>
        /// Post запрос на разлогин всех сессий пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <returns>204 No Content.</returns>
        [Authorize]
        [HttpPost("revokeAllSessions")]
        public async Task<IActionResult> RevokeAllSessions(Guid userId)
        {
            await _authService.RevokeAllSessionsAsync(userId);
            return NoContent();
        }

        /// <summary>
        /// Get запрос на получение всех активных сессий пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <returns>200 Коллекция активных сессий пользователя.</returns>
        [Authorize]
        [HttpGet("sessions")]
        public async Task<IActionResult> GetAllUserSessions(Guid userId)
        {
            var response = await _authService.GetActiveUserSessionsAsync(userId);
            return Ok(response);
        }
    }
}
