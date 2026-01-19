using EKR_AuthorizeService.Entities;
using EKR_AuthorizeService.Repositories.Interfaces.User;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_AuthorizeService.Services.Interfaces.Encription;
using EKR_SharedShared.Auth.Post.Response;
using EKR_SharedShared.Auth.Get.Response;
using EKR_SharedShared.Auth.Post.Incoming;
using EKR_SharedShared.Auxiliary.DeviceInfo;

namespace EKR_AuthorizeService.Services.Auth
{
    /// <summary>
    /// Сервис для управления регистрацией и авторизацией пользователей.
    /// </summary>
    public class AuthService(IUserRepository userRepository,
                             IGeneratorService generatorService,
                             IPasswordHashService passwordHashService,
                             ISessionService sessionService,
                             ISessionRepository sessionRepository) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IGeneratorService _generatorService = generatorService;
        private readonly IPasswordHashService _passwordHashService = passwordHashService;
        private readonly ISessionService _sessionService = sessionService;
        private readonly ISessionRepository _sessionRepository = sessionRepository;

        /// <summary>
        /// Асинхронно регистрирует нового пользователя в системе.
        /// </summary>
        /// <param name="dto">Данные для регистрации пользователя.</param>
        public async Task<GetUserResponseDto> RegisterAsync(RegisterRequestDto dto)
        {
            if (await _userRepository.GetUserByUsernameAsync(dto.Username.ToUpper()) != null)
            {
                throw new InvalidOperationException("Пользователь с таким именем уже существует.");
            }

            var salt = _generatorService.GenerateSalt(32);

            User user = new()
            {
                Username = dto.Username,
                UsernameNormalized = dto.Username.ToUpper(),
                Salt = salt,
                PasswordHash = _passwordHashService.HashPassword(dto.Password, salt),
            };

            await _userRepository.CreateUserAsync(user);

            return new GetUserResponseDto
            {
                Id = user.Id,
                Username = user.Username
            };
        }

        /// <summary>
        /// Асинхронно авторизует пользователя и выдает JWT токен.
        /// </summary>
        /// <param name="dto">Данные для авторизации пользователя.</param>
        /// <param name="deviceInfo">Информация об устройстве.</param>
        /// <returns>JWT токен при успешной авторизации.</returns>
        public async Task<AuthResponseDto> AuthorizationAsync(AuthorizationDto dto, DeviceInfoDto deviceInfo)
        {
            var user = await _userRepository.GetUserByUsernameAsync(dto.Username);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Неверный логин или пароль.");
            }

            bool isPasswordValid = _userRepository.VerifyPassword(dto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Неверный логин или пароль.");
            }

            var session = (await _sessionRepository.GetUserSessionsAsync(user.Id)).FirstOrDefault(s => ((DeviceInfoDto)s.DeviceInfo).DeviceId == deviceInfo.DeviceId);

            string accessToken, refreshToken;
            Guid sessionId;

            if (session == null)
            {
                (accessToken, refreshToken, sessionId) = await _sessionService.CreateSessionAsync(user, deviceInfo);
            }
            else if((DeviceInfoDto)session.DeviceInfo != deviceInfo || dto.RefreshToken == null)
            {
                await _sessionRepository.RemoveSessionAsync(session);
                (accessToken, refreshToken, sessionId) = await _sessionService.CreateSessionAsync(user, deviceInfo);
            }
            else
            {
                session.IsRevoked = false;
                (accessToken, refreshToken, sessionId) = await _sessionService.RefreshAsync(dto.RefreshToken);
                await _sessionRepository.UpdateSessionAsync(session);
            }

                return new AuthResponseDto
                {
                    SessionId = sessionId,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    UserId = user.Id,
                    Username = user.Username
                };

        }

        /// <summary>
        /// Асинхронное обновление токенов.
        /// </summary>
        /// <param name="incomingRefreshToken">Текущий refresh токен.</param>
        /// <returns>Dto с новыми токенами.</returns>
        public async Task<RefreshDto> RefreshAsync(string incomingRefreshToken)
        {
            (string accessToken, string refreshToken, Guid sessionId) = await _sessionService.RefreshAsync(incomingRefreshToken);

            return new RefreshDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                SessionId = sessionId
            };
        }

        /// <summary>
        /// Асинхронный разлогин конкретной сессии.
        /// </summary>
        /// <param name="sessionId">Id сессии, которую надо прервать.</param>
        public async Task RevokeSessionAsync(Guid sessionId)
        {
            await _sessionService.RevokeSessionAsync(sessionId);
        }

        /// <summary>
        /// Асинхронный разлогин всех сессий пользователя, кроме указанной.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="keepSessionId">Id сессии, которую нужно оставить.</param>
        public async Task RevokeOtherSessionsAsync(Guid userId, Guid keepSessionId)
        {
            await _sessionService.RevokeOtherSessionsAsync(userId, keepSessionId);
        }

        /// <summary>
        /// Асинхронный разлогин всех сессий пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        public async Task RevokeAllSessionsAsync(Guid userId)
        {
            await _sessionService.RevokeAllSessionsAsync(userId);
        }

        /// <summary>
        /// Асинхронно получает коллекцию активных сессий пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <returns>Коллекция активных сессий пользователя.</returns>
        public async Task<ICollection<GetSessionResponseDto>> GetActiveUserSessionsAsync(Guid userId)
        {
            return (await _sessionRepository.GetActiveUserSessionsAsync(userId)).Select(s => new GetSessionResponseDto()
            {
                CreatedAt = s.CreatedAt,
                DeviceInfo = s.DeviceInfo,
                ExpiresAt = s.ExpiresAt,
                Id = s.Id,
                IsRevoked = s.IsRevoked,
                RefreshToken = s.RefreshToken,
                UserId = s.UserId
            }).ToList();
        }
    }
}
