using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.ErrorHandling;
using Application.Common.Interfaces;
using Domain.Entities.Identity;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Auth
{
public class Register
    {
        public class Command : IRequest
        {
            private Command() { }

            public Command(
                string email,
                string userName,
                string firstName,
                string lastName,
                string phoneNumber,
                string password)
            {
                Email = email;
                UserName = userName;
                FirstName = firstName;
                LastName = lastName;
                PhoneNumber = phoneNumber;
                Password = password;
            }

            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Password { get; set; }
            public string UserName { get; set; }
            public string PhoneNumber { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.UserName).NotNull().NotEmpty();
                RuleFor(x => x.Email).NotNull().NotEmpty();
                RuleFor(x => x.FirstName).NotNull().NotEmpty();
                RuleFor(x => x.Password).NotNull().NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly IPasswordHasher _passwordHasher;
            private readonly IApplicationDbContext _context;

            public Handler(
                IPasswordHasher passwordHasher,
                IApplicationDbContext context)
            {
                _passwordHasher = passwordHasher;
                _context = context;
            }

            public async Task<Unit> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var user = await _context.Users.SingleOrDefaultAsync(
                    x => x.Email == request.Email,
                    cancellationToken);

                if (user != null)
                {
                    throw new HttpException(
                        HttpStatusCode.BadRequest,
                        new
                        {
                            Error = $"There is already a user with email {request.Email}."
                        });
                }


                var salt = Guid.NewGuid().ToByteArray();
                var newUser = new User(
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.UserName,
                    request.PhoneNumber,
                    _passwordHasher.Hash(
                        "test",
                        salt),
                    salt,
                    new Address());

                await _context.Users.AddAsync(
                    newUser,
                    cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }
        }
    }
}