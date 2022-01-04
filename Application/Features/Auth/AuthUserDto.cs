using System.Text.Json.Serialization;

namespace Application.Features.Auth
{
    public class AuthUserDto
    {
        protected AuthUserDto() { }

        public AuthUserDto(
            int id,
            string fullName,
            string email,
            string token,
            string refreshToken)
        {
            Id = id;
            FullName = fullName;
            Email = email;
            Token = token;
            RefreshToken = refreshToken;
        }

        public int Id { get; }
        public string FullName { get; }
        public string Email { get; }
        public string Token { get; }
        public string RefreshToken { get; }

        [JsonIgnore]
        public byte[] Hash { get; }

        [JsonIgnore]
        public byte[] Salt { get; }
    }
}