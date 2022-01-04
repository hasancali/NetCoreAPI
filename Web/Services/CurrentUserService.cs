using System.Security.Claims;
using Application.Common.Auth.Authorization;
using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Web.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        ClaimsPrincipal User => _httpContextAccessor?.HttpContext?.User;

        public string Id => User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? string.Empty;

        public string Name => User?.FindFirst(JwtRegisteredClaimNames.GivenName)?.Value
                              ?? string.Empty;

        public string Permissions => User?.FindFirst(JwtRegisteredCustomClaimNames.Permissions)?.Value
                                     ?? string.Empty;
    }
}