using EKR_AuthorizeService.Entities;
using EKR_AuthorizeService.Repositories.Interfaces.User;
using EKR_AuthorizeService.Services.Interfaces.Auth;
using EKR_AuthorizeService.Services.Interfaces.Encription;
using EKR_Shared.Auth.Post.Response;
using EKR_Shared.Auth.Get.Response;
using EKR_Shared.Auth.Post.Incoming;
using EKR_Shared.Auxiliary;
using EKR_Shared.Services.Interfaces.Infrastructure;

namespace EKR_AuthorizeService.Services.Auth
{
    /// <summary>
    /// Сервис для управления регистрацией и авторизацией пользователей.
    /// </summary>
    public class AuthService(IUserRepository userRepository,
                             IGeneratorService generatorService,
                             IPasswordHashService passwordHashService,
                             ISessionService sessionService,
                             ISessionRepository sessionRepository,
                             IKafkaProducerService kafkaProducerService) : IAuthService
    {
        private readonly IKafkaProducerService _kafkaProducerService = kafkaProducerService;
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IGeneratorService _generatorService = generatorService;
        private readonly IPasswordHashService _passwordHashService = passwordHashService;
        private readonly ISessionService _sessionService = sessionService;
        private readonly ISessionRepository _sessionRepository = sessionRepository;

        /// <summary>
        /// Асинхронно регистрирует нового пользователя в системе.
        /// </summary>
        /// <param name="dto">Данные для регистрации пользователя.</param>
        /// <param name="requestId">Id запроса.</param>
        public async Task<bool> RegisterAsync(RegisterRequestDto dto, string requestId)
        {
            try
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

                return true;
            }
            catch(Exception ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new { ex.Message, Type = ex.GetType() }.ToString()!, partition: requestId);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно авторизует пользователя и выдает JWT токен.
        /// </summary>
        /// <param name="dto">Данные для авторизации пользователя.</param>
        /// <param name="requestId">Id запроса.</param>
        /// <returns>JWT токен при успешной авторизации.</returns>
        public async Task<AuthResponseDto> AuthorizationAsync(AuthorizationDto dto, string requestId)
        {
            try
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

                var session = (await _sessionRepository.GetUserSessionsAsync(user.Id)).FirstOrDefault(s => (ConnectionInfoDto)s.ConnectionInfo == dto.ConnectionInfo);

                string accessToken, refreshToken;
                Guid sessionId;

                if (session == null)
                {
                    (accessToken, refreshToken, sessionId) = await _sessionService.CreateSessionAsync(user, dto.ConnectionInfo);
                }
                else if ((ConnectionInfoDto)session.ConnectionInfo != dto.ConnectionInfo || dto.RefreshToken == null)
                {
                    await _sessionRepository.RemoveSessionAsync(session);
                    (accessToken, refreshToken, sessionId) = await _sessionService.CreateSessionAsync(user, dto.ConnectionInfo);
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
            catch (Exception ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new { ex.Message, Type = ex.GetType() }.ToString()!, partition: requestId);
                throw;
            }

        }

        /// <summary>
        /// Асинхронное обновление токенов.
        /// </summary>
        /// <param name="incomingRefreshToken">Текущий refresh токен.</param>
        /// <param name="requestId">Id запроса.</param>
        /// <returns>Dto с новыми токенами.</returns>
        public async Task<RefreshDto> RefreshAsync(string incomingRefreshToken, string requestId)
        {
            try
            {
                (string accessToken, string refreshToken, Guid sessionId) = await _sessionService.RefreshAsync(incomingRefreshToken);

                return new RefreshDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    SessionId = sessionId
                };
            }
            catch (Exception ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new { ex.Message, Type = ex.GetType() }.ToString()!, partition: requestId);
                throw;
            }
        }

        /// <summary>
        /// Асинхронный разлогин конкретной сессии.
        /// </summary>
        /// <param name="sessionId">Id сессии, которую надо прервать.</param>
        /// <param name="requestId">Id запроса.</param>
        public async Task<bool> RevokeSessionAsync(Guid sessionId, string requestId)
        {
            try
            {
                await _sessionService.RevokeSessionAsync(sessionId);
                return true;
            }
            catch (Exception ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new { ex.Message, Type = ex.GetType() }.ToString()!, partition: requestId);
                throw;
            }
        }

        /// <summary>
        /// Асинхронный разлогин всех сессий пользователя, кроме указанной.
        /// </summary>
        /// <param name="dto">Id пользователя + Id сессии, которую надо оставить.</param>
        /// <param name="requestId">Id запроса.</param>
        public async Task<bool> RevokeOtherSessionsAsync(RevokeOtherSessionsDto dto, string requestId)
        {
            try
            {
                await _sessionService.RevokeOtherSessionsAsync(dto.UserId, dto.KeepSessionId);
                return true;
            }
            catch (Exception ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new { ex.Message, Type = ex.GetType() }.ToString()!, partition: requestId);
                throw;
            }
        }

        /// <summary>
        /// Асинхронный разлогин всех сессий пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="requestId">Id запроса.</param>
        public async Task<bool> RevokeAllSessionsAsync(Guid userId, string requestId)
        {
            try
            {
                await _sessionService.RevokeAllSessionsAsync(userId);
                return true;
            }
            catch (Exception ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new { ex.Message, Type = ex.GetType() }.ToString()!, partition: requestId);
                throw;
            }
        }

        /// <summary>
        /// Асинхронно получает коллекцию активных сессий пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя.</param>
        /// <param name="requestId">Id запроса.</param>
        /// <returns>Коллекция активных сессий пользователя.</returns>
        public async Task<ICollection<GetSessionResponseDto>> GetActiveUserSessionsAsync(Guid userId, string requestId)
        {
            try
            {
                return (await _sessionRepository.GetActiveUserSessionsAsync(userId)).Select(s => new GetSessionResponseDto()
                {
                    CreatedAt = s.CreatedAt,
                    DeviceInfo = s.ConnectionInfo,
                    ExpiresAt = s.ExpiresAt,
                    Id = s.Id,
                    IsRevoked = s.IsRevoked,
                    RefreshToken = s.RefreshToken,
                    UserId = s.UserId
                }).ToList();
            }
            catch (Exception ex)
            {
                await _kafkaProducerService.GiveAnswerAsync(new { ex.Message, Type = ex.GetType() }.ToString()!, partition: requestId);
                throw;
            }
        }
    }
}
