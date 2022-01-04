using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.Common.Interfaces
{
    public interface IJwtTokenGenerator
    {
        Task<string> CreateToken(
            string userId,
            string userName,
            string email,
            IEnumerable<Permissions> permissions);

        string GenerateRefreshToken(
            int size = 32);
    }
}