using Fake.Detection.Auth.Configure;
using Fake.Detection.Auth.Helpers;
using Fake.Detection.Auth.Models;
using Fake.Detection.Auth.Repositories;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Fake.Detection.Auth.Services;

public class AuthService : UserService.UserServiceBase
{
    private readonly IUserRepository _userRepository;
    private readonly JWTHelper _jwtHelper;
    private readonly IOptionsMonitor<AuthOptions> _options;

    public AuthService(
        IUserRepository userRepository,
        JWTHelper jwtHelper,
        IOptionsMonitor<AuthOptions> options)
    {
        _userRepository = userRepository;
        _jwtHelper = jwtHelper;
        _options = options;
    }

    public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        try
        {
            var userInfo = await _userRepository.CreateAsync(request.Login, request.Name,
                request.Password.ComputeSha256Hash(),
                context.CancellationToken);

            var token = _jwtHelper.GenerateJwtToken(userInfo.Login);

            return new CreateUserResponse
            {
                Result = true,
                User = new UserModel
                {
                    Name = userInfo.Name,
                    Token = token,
                }
            };
        }
        catch (ArgumentException)
        {
            return new CreateUserResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.AlreadyExisted
            };
        }
        catch (Exception)
        {
            return new CreateUserResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.Unexpected,
            };
        }
    }

    public override async Task<GenerateTokenResponse> GenerateToken(GenerateTokenRequest request,
        ServerCallContext context)
    {
        var (errorStatus, userInfo) = await CheckToken(
            context.RequestHeaders.FirstOrDefault(header =>
                header.Key.Equals(_options.CurrentValue.Header, StringComparison.InvariantCultureIgnoreCase))?.Value,
            context.CancellationToken);

        if (errorStatus != null)
            return new GenerateTokenResponse
            {
                Result = false,
                ErrorStatus = errorStatus.Value
            };

        if (userInfo is null)
            return new GenerateTokenResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.IncorrectCredentials
            };


        return new GenerateTokenResponse
        {
            Result = true,
            Token = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    JsonConvert.SerializeObject(
                        new UserToken(userInfo.Login, userInfo.PasswordHash))))
        };
    }

    public override async Task<TGLinkResponse> TGLink(TGLinkRequest request, ServerCallContext context)
    {
        try
        {
            var userToken =
                JsonConvert.DeserializeObject<UserToken>(
                    System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(request.Token)));

            if (userToken is null)
                return new TGLinkResponse
                {
                    Result = false,
                    ErrorStatus = ErrorResponse.IncorrectCredentials,
                };

            var userInfo = await _userRepository.GetAsync(userToken.Login, context.CancellationToken);

            if (userInfo is null)
                return new TGLinkResponse
                {
                    Result = false,
                    ErrorStatus = ErrorResponse.IncorrectCredentials,
                };

            if (!userInfo.PasswordHash.Equals(userToken.Password, StringComparison.InvariantCultureIgnoreCase))
                return new TGLinkResponse
                {
                    Result = false,
                    ErrorStatus = ErrorResponse.IncorrectCredentials,
                };

            userInfo = await _userRepository.LinkTgAsync(userToken.Login, request.TgId, context.CancellationToken);

            var token = _jwtHelper.GenerateJwtToken(userInfo.Login);

            return new TGLinkResponse
            {
                Result = true,
                User = new UserModel
                {
                    Name = userInfo.Name,
                    Token = token,
                }
            };
        }
        catch (ArgumentException)
        {
            return new TGLinkResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.AlreadyExisted
            };
        }
        catch (Exception)
        {
            return new TGLinkResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.Unexpected,
            };
        }
    }

    public override async Task<TGLoginResponse> TGLogin(TGLoginRequest request, ServerCallContext context)
    {
        var userInfo = await _userRepository.GetAsync(request.TgId, context.CancellationToken);

        if (userInfo is null)
            return new TGLoginResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.IncorrectCredentials
            };

        var token = _jwtHelper.GenerateJwtToken(userInfo.Login);

        return new TGLoginResponse
        {
            Result = true,
            User = new UserModel
            {
                Name = userInfo.Name,
                Token = token,
            }
        };
    }

    public override async Task<TGSignOutResponse> TGSignOut(TGSignOutRequest request, ServerCallContext context)
    {
        try
        {
            var userInfo = await _userRepository.GetAsync(request.TgId, context.CancellationToken);

            if (userInfo is not null)
                await _userRepository.LinkTgAsync(userInfo.Login, null, context.CancellationToken);

            return new TGSignOutResponse
            {
                Result = userInfo is not null
            };
        }
        catch (Exception e)
        {
            return new TGSignOutResponse
            {
                Result = false
            };
        }
    }

    public override async Task<RestorePasswordResponse> RestorePassword(RestorePasswordRequest request,
        ServerCallContext context)
    {
        var userInfo = await _userRepository.GetAsync(request.Login, context.CancellationToken);

        if (userInfo is null)
            return new RestorePasswordResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.IncorrectCredentials,
            };

        await _userRepository.UpdateAsync(userInfo.Login, userInfo.Name, request.NewPassword.ComputeSha256Hash(),
            context.CancellationToken);

        var token = _jwtHelper.GenerateJwtToken(userInfo.Login);

        return new RestorePasswordResponse
        {
            Result = true,
            User = new UserModel
            {
                Name = userInfo.Name,
                Token = token,
            }
        };
    }

    [Authorize]
    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        var (errorStatus, userInfo) = await CheckToken(
            context.RequestHeaders.FirstOrDefault(header =>
                header.Key.Equals(_options.CurrentValue.Header, StringComparison.InvariantCultureIgnoreCase))?.Value,
            context.CancellationToken);

        if (errorStatus != null)
            return new UpdateUserResponse
            {
                Result = false,
                ErrorStatus = errorStatus.Value
            };

        if (userInfo is null)
            return new UpdateUserResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.IncorrectCredentials
            };

        var updateToken = _jwtHelper.GenerateJwtToken(userInfo.Login);

        return new UpdateUserResponse
        {
            Result = true,
            User = new UserModel
            {
                Name = userInfo.Name,
                Token = updateToken,
            }
        };
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var userInfo = await _userRepository.GetAsync(request.Login, context.CancellationToken);

        if (userInfo is null)
            return new LoginResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.IncorrectCredentials,
            };

        if (!userInfo.PasswordHash.Equals(request.Password.ComputeSha256Hash()))
            return new LoginResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.IncorrectCredentials,
            };

        var token = _jwtHelper.GenerateJwtToken(userInfo.Login);

        return new LoginResponse
        {
            Result = true,
            User = new UserModel
            {
                Name = userInfo.Name,
                Token = token,
            }
        };
    }

    public override async Task<AuthResponse> Auth(AuthRequest request, ServerCallContext context)
    {
        var (errorStatus, userInfo) = await CheckToken(
            context.RequestHeaders.FirstOrDefault(header =>
                header.Key.Equals(_options.CurrentValue.Header, StringComparison.InvariantCultureIgnoreCase))?.Value,
            context.CancellationToken);

        if (errorStatus != null)
            return new AuthResponse
            {
                Result = false,
                ErrorStatus = errorStatus.Value
            };

        if (userInfo is null)
            return new AuthResponse
            {
                Result = false,
                ErrorStatus = ErrorResponse.IncorrectCredentials
            };

        var updateToken = _jwtHelper.GenerateJwtToken(userInfo.Login);

        return new AuthResponse
        {
            Result = true,
            User = new UserModel
            {
                Name = userInfo.Name,
                Token = updateToken,
            }
        };
    }

    private async Task<(ErrorResponse? errorResponse, UserInfo? userInfo)> CheckToken(
        string? tokenWithBearer, CancellationToken cancellationToken)
    {
        if (tokenWithBearer == null)
            return (ErrorResponse.Unauthenticated, null);

        var token = tokenWithBearer.StartsWith(_options.CurrentValue.Marker)
            ? tokenWithBearer[_options.CurrentValue.Marker.Length..]
            : tokenWithBearer;

        var login = _jwtHelper.GetLoginFromToken(token);

        if (login == null)
            return (ErrorResponse.Unauthenticated, null);

        var userInfo = await _userRepository.GetAsync(login, cancellationToken);

        if (userInfo is null)
            return (ErrorResponse.IncorrectCredentials, null);

        return (null, userInfo);
    }
}