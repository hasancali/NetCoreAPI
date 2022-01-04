using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Auth.Authentication;
using Application.Common.ErrorHandling;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Application.Features.Auth
{
    public class ExchangeRefreshToken
    {
        public class Command : IRequest<AuthUserDto>
        {
            private Command() { }

            public Command(
                string token,
                string refreshToken)
            {
                Token = token;
                RefreshToken = refreshToken;
            }

            public string Token { get; set; }
            public string RefreshToken { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.RefreshToken).NotNull().NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, AuthUserDto>
        {
            private readonly IApplicationDbContext _context;
            private readonly IJwtTokenGenerator _jwtTokenGenerator;
            private readonly JwtSettings _jwtSettings;

            public Handler(
                IApplicationDbContext context,
                IJwtTokenGenerator jwtTokenGenerator,
                IOptions<JwtSettings> jwtSettings)
            {
                _context = context;
                _jwtTokenGenerator = jwtTokenGenerator;
                _jwtSettings = jwtSettings.Value;
            }

            public async Task<AuthUserDto> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var userId = GetIdentifierFromExpiredToken(request.Token).Value;

                var user = await _context.Users
                    .Include(u => u.RefreshTokens)
                    .Include(u => u.Roles)
                    .ThenInclude(x => x.Role)
                    .SingleAsync(
                        x => x.Id.ToString() == userId && !x.Archived,
                        cancellationToken);

                if (user == null)
                {
                    throw new HttpException(
                        HttpStatusCode.Unauthorized,
                        new {Error = "Invalid credentials."});
                }

                if (!user.IsValidRefreshToken(request.RefreshToken))
                {
                    throw new HttpException(
                        HttpStatusCode.Unauthorized,
                        new {Error = "Invalid credentials."});
                }

                var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
                user.RemoveRefreshToken(request.RefreshToken);
                user.AddRefreshToken(
                    refreshToken,
                    user.Id);
                var token = await _jwtTokenGenerator.CreateToken(
                    user.Id.ToString(),
                    user.UserName,
                    user.Email,
                    user.Roles.SelectMany(x => x.Role.PermissionsInRole));
                await _context.SaveChangesAsync(cancellationToken);

                return new AuthUserDto(
                    user.Id,
                    user.FullName,
                    user.Email,
                    token,
                    refreshToken);
            }


            private Claim GetIdentifierFromExpiredToken(
                string token)
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _jwtSettings.SigningCredentials.Key,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = false, // do not check for expiry date time
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(
                    token,
                    tokenValidationParameters,
                    out var securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Contains(
                        SecurityAlgorithms.HmacSha256Signature,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub);
            }
        }
    }
}