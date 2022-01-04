using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Application.Common.Auth.Authentication;
using Application.Common.Auth.Authorization;
using Application.Common.Interfaces;
using Domain.Enums;
using Domain.Helpers;
using Microsoft.Extensions.Options;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Infrastructure.Auth
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenGenerator(
            IOptions<JwtSettings> jwtOptions)
        {
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<string> CreateToken(
            string userId,
            string userName,
            string email,
            IEnumerable<Permissions> permissions)
        {
            var claims = new[]
            {
                new Claim(
                    JwtRegisteredClaimNames.Sub,
                    userId),
                new Claim(
                    JwtRegisteredClaimNames.GivenName,
                    userName),
                new Claim(
                    JwtRegisteredClaimNames.Email,
                    email),
                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    await _jwtSettings.JtiGenerator()),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(_jwtSettings.IssuedAt).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new Claim(
                    JwtRegisteredCustomClaimNames.Permissions,
                    permissions.PackPermissions()),
            };
            var jwt = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                claims,
                _jwtSettings.NotBefore,
                _jwtSettings.Expiration,
                _jwtSettings.SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public string GenerateRefreshToken(
            int size = 32)
        {
            var randomNumber = new byte[size];
            using var numberGenerator = RandomNumberGenerator.Create();
            numberGenerator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
