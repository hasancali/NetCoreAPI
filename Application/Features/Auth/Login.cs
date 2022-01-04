using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.ErrorHandling;
using Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth
{
    public class Login
    {
        public class Command : IRequest<AuthUserDto>
        {
            private Command() { }

            public Command(
                string userName,
                string password)
            {
                UserName = userName;
                Password = password;
            }

            public string UserName { get; set; }
            public string Password { get; set; }

        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.UserName).NotNull().NotEmpty();
                RuleFor(x => x.Password).NotNull().NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, AuthUserDto>
        {
            private readonly IPasswordHasher _passwordHasher;
            private readonly IJwtTokenGenerator _jwtTokenGenerator;
            private readonly IApplicationDbContext _context;

            public Handler(
                IPasswordHasher passwordHasher,
                IJwtTokenGenerator jwtTokenGenerator,
                IApplicationDbContext context)
            {
                _passwordHasher = passwordHasher;
                _jwtTokenGenerator = jwtTokenGenerator;
                _context = context;
            }

            public async Task<AuthUserDto> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .Include(x => x.Roles)
                    .ThenInclude(x => x.Role)
                    .SingleOrDefaultAsync(
                        x => x.UserName == request.UserName && !x.Archived,
                        cancellationToken);

                if (user == null)
                {
                    throw new HttpException(
                        HttpStatusCode.Unauthorized,
                        new {Error = "Invalid credentials."});
                }

                if (!user.Hash.SequenceEqual(
                    _passwordHasher.Hash(
                        request.Password,
                        user.Salt)))
                {
                    throw new HttpException(
                        HttpStatusCode.Unauthorized,
                        new {Error = "Invalid credentials."});
                }

                // generate refresh token
                var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
                user.AddRefreshToken(
                    refreshToken,
                    user.Id);

                var token = await _jwtTokenGenerator.CreateToken(
                    user.Id.ToString(),
                    user.FullName,
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
        }
    }
}